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
            entryDirection = pending.exitDirection;
            exitDirection = Opposite(pending.exitDirection);
        }
        else
        {
            entryDirection = Direction8Way.West;
            exitDirection = Direction8Way.East;
        }

        IntVec3 start = PickEdgeCell(entryDirection);
        IntVec3 end = PickEdgeCell(exitDirection);
        paintedRoadCells = RoadPainter.Paint(map, start, end, RoadboundWorldMod.Settings.roadHalfWidth);
        RoadPoiSpawner.SpawnPois(map, paintedRoadCells, RoadboundWorldMod.Settings.poiCount);
        RestorePersistentHostiles(entryDirection);

        nextTravelerTick = Find.TickManager.TicksGame + (int)(RoadboundWorldMod.Settings.travelerIntervalDays * 60000f);
        initialized = true;
    }

    private void RestorePersistentHostiles(Direction8Way incomingDirection)
    {
        var records = Find.World.GetComponent<RoadWorldComponent>().ConsumeHostiles(map.Tile, incomingDirection);
        if (records.Count == 0)
        {
            return;
        }

        foreach (PersistentHostileRecord record in records)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(record.pawnKind, record.faction);
            IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(PickEdgeCell(incomingDirection), map, 6);
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

        var group = new PersistentHostileGroup
        {
            tile = MapEdgeUtility.GetNeighborTile(map.Tile, exitDirection),
            entryDirection = exitDirection,
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
                pawnKind = pawn.kindDef,
                faction = pawn.Faction,
                sourceLabel = pawn.LabelShortCap,
                healthFraction = pawn.health.summaryHealth.SummaryHealthPercent,
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
            _ => Direction8Way.South,
        };
    }
}
