using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RoadboundWorld.Systems;
using RoadboundWorld.Utility;
using RoadboundWorld.World;
using Verse;

namespace RoadboundWorld.Components;

public sealed class RoadboundMapComponent : MapComponent
{
    private bool initialized;
    private int nextTravelerTick;
    private int backtrackTile = -1;
    private int forwardTile = -1;
    private Direction8Way entryDirection = Direction8Way.North;
    private Direction8Way exitDirection = Direction8Way.South;
    private List<IntVec3> paintedRoadCells = new();
    private HashSet<int> exportedHostileIds = new();

    public RoadboundMapComponent(Map map) : base(map)
    {
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        TryInitializeRoadMap();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref initialized, nameof(initialized), false);
        Scribe_Values.Look(ref nextTravelerTick, nameof(nextTravelerTick), 0);
        Scribe_Values.Look(ref backtrackTile, nameof(backtrackTile), -1);
        Scribe_Values.Look(ref forwardTile, nameof(forwardTile), -1);
        Scribe_Values.Look(ref entryDirection, nameof(entryDirection), Direction8Way.North);
        Scribe_Values.Look(ref exitDirection, nameof(exitDirection), Direction8Way.South);
        Scribe_Collections.Look(ref paintedRoadCells, nameof(paintedRoadCells), LookMode.Value);
        Scribe_Collections.Look(ref exportedHostileIds, nameof(exportedHostileIds), LookMode.Value);
    }

    public override void MapComponentTick()
    {
        if (!initialized)
        {
            return;
        }

        if (Find.TickManager.TicksGame >= nextTravelerTick)
        {
            RoadTravelerSpawner.TrySpawnTravelers(map, paintedRoadCells, entryDirection, exitDirection);
            nextTravelerTick = Find.TickManager.TicksGame + (int)(RoadboundWorldMod.Settings.travelerIntervalDays * 60000f);
        }

        if (Find.TickManager.TicksGame % 180 == 0)
        {
            ExportHostilesThatFledOffMap();
        }
    }

    public bool TryGetConnectedTile(Direction8Way edgeDirection, out int targetTile)
    {
        targetTile = -1;
        if (!initialized)
        {
            return false;
        }

        if (edgeDirection == entryDirection && backtrackTile >= 0)
        {
            targetTile = backtrackTile;
            return true;
        }

        if (edgeDirection == exitDirection && forwardTile >= 0)
        {
            targetTile = forwardTile;
            return true;
        }

        return false;
    }

    private void TryInitializeRoadMap()
    {
        if (initialized || map == null)
        {
            return;
        }

        var worldState = Find.World.GetComponent<RoadWorldComponent>();
        var pending = worldState.ConsumePendingTransitionForTile(map.Tile);
        bool transitSite = map.Parent is RoadboundTransitSite;

        if (!transitSite && pending == null)
        {
            return;
        }

        if (pending != null)
        {
            backtrackTile = pending.fromTile;
            entryDirection = pending.exitDirection;
            exitDirection = Opposite(pending.exitDirection);
            forwardTile = MapEdgeUtility.GetNeighborTile(map.Tile, exitDirection);
        }
        else
        {
            entryDirection = Direction8Way.West;
            exitDirection = Direction8Way.East;
            backtrackTile = MapEdgeUtility.GetNeighborTile(map.Tile, entryDirection);
            forwardTile = MapEdgeUtility.GetNeighborTile(map.Tile, exitDirection);
        }

        IntVec3 start = PickEdgeCell(entryDirection);
        IntVec3 end = PickEdgeCell(exitDirection);
        paintedRoadCells = RoadPainter.Paint(map, start, end, RoadboundWorldMod.Settings.roadHalfWidth);
        RoadPoiSpawner.SpawnPois(map, paintedRoadCells, RoadboundWorldMod.Settings.poiCount);
        RestorePersistentHostiles(ToRot4(entryDirection));

        nextTravelerTick = Find.TickManager.TicksGame + (int)(RoadboundWorldMod.Settings.travelerIntervalDays * 60000f);
        initialized = true;
    }

    private void RestorePersistentHostiles(Rot4 incomingDirection)
    {
        var records = Find.World.GetComponent<RoadWorldComponent>().ConsumeHostiles(map.Tile, incomingDirection);
        if (records.Count == 0)
        {
            return;
        }

        foreach (PersistentHostileRecord record in records)
        {
            PawnKindDef kindDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(record.kindDefName);
            Faction faction = null;
            if (!string.IsNullOrEmpty(record.factionDefName))
            {
                FactionDef factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(record.factionDefName);
                if (factionDef != null)
                {
                    faction = Find.FactionManager.FirstFactionOfDef(factionDef);
                }
            }

            if (kindDef == null)
            {
                continue;
            }

            Pawn pawn = faction != null ? PawnGenerator.GeneratePawn(kindDef, faction) : PawnGenerator.GeneratePawn(kindDef);
            IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(PickEdgeCell(ToDirection8Way(incomingDirection)), map, 6);
            GenSpawn.Spawn(pawn, spawnCell, map);
        }
    }

    private void ExportHostilesThatFledOffMap()
    {
        var hostiles = map.mapPawns.AllPawnsSpawned
            .Where(p => p.HostileTo(Faction.OfPlayer) && !p.Downed && MapEdgeUtility.IsOnEdge(p.Position, map, 2))
            .ToList();

        if (hostiles.Count == 0)
        {
            return;
        }

        int destinationTile = forwardTile >= 0 ? forwardTile : MapEdgeUtility.GetNeighborTile(map.Tile, exitDirection);
        if (destinationTile < 0)
        {
            return;
        }

        var group = new PersistentHostileGroup
        {
            tile = destinationTile,
            entryDirection = ToRot4(exitDirection).AsInt,
            expiresAtTick = Find.TickManager.TicksGame + (int)(RoadboundWorldMod.Settings.persistentHostileDays * 60000f),
        };

        foreach (Pawn pawn in hostiles)
        {
            if (!exportedHostileIds.Add(pawn.thingIDNumber))
            {
                continue;
            }

            group.pawns.Add(new PersistentHostileRecord
            {
                kindDefName = pawn.kindDef.defName,
                factionDefName = pawn.Faction?.def?.defName ?? string.Empty,
                count = 1,
            });

            pawn.DeSpawn();
        }

        if (group.pawns.Count > 0)
        {
            Find.World.GetComponent<RoadWorldComponent>().AddHostileGroup(group);
        }
    }

    private IntVec3 PickEdgeCell(Direction8Way direction)
    {
        return direction switch
        {
            Direction8Way.North => new IntVec3(map.Center.x, 0, map.Size.z - 2),
            Direction8Way.South => new IntVec3(map.Center.x, 0, 1),
            Direction8Way.East => new IntVec3(map.Size.x - 2, 0, map.Center.z),
            Direction8Way.West => new IntVec3(1, 0, map.Center.z),
            Direction8Way.NorthEast => new IntVec3(map.Size.x - 2, 0, map.Size.z - 2),
            Direction8Way.NorthWest => new IntVec3(1, 0, map.Size.z - 2),
            Direction8Way.SouthEast => new IntVec3(map.Size.x - 2, 0, 1),
            Direction8Way.SouthWest => new IntVec3(1, 0, 1),
            _ => map.Center,
        };
    }

    private static Direction8Way Opposite(Direction8Way direction)
    {
        return direction switch
        {
            Direction8Way.North => Direction8Way.South,
            Direction8Way.South => Direction8Way.North,
            Direction8Way.East => Direction8Way.West,
            Direction8Way.West => Direction8Way.East,
            Direction8Way.NorthEast => Direction8Way.SouthWest,
            Direction8Way.NorthWest => Direction8Way.SouthEast,
            Direction8Way.SouthEast => Direction8Way.NorthWest,
            Direction8Way.SouthWest => Direction8Way.NorthEast,
            _ => Direction8Way.North,
        };
    }

    private static Rot4 ToRot4(Direction8Way direction)
    {
        return direction switch
        {
            Direction8Way.North or Direction8Way.NorthEast or Direction8Way.NorthWest => Rot4.North,
            Direction8Way.South or Direction8Way.SouthEast or Direction8Way.SouthWest => Rot4.South,
            Direction8Way.East => Rot4.East,
            Direction8Way.West => Rot4.West,
            _ => Rot4.North,
        };
    }

    private static Direction8Way ToDirection8Way(Rot4 direction)
    {
        return direction.AsInt switch
        {
            0 => Direction8Way.North,
            1 => Direction8Way.East,
            2 => Direction8Way.South,
            3 => Direction8Way.West,
            _ => Direction8Way.North,
        };
    }
}
