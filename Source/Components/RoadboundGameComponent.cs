using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RoadboundWorld.Systems;
using RoadboundWorld.Utility;
using RoadboundWorld.World;
using Verse;

namespace RoadboundWorld.Components;

public sealed class RoadboundGameComponent : GameComponent
{
    private const int CheckInterval = 64;
    private const int StockpileCheckInterval = 180;
    private int lastCheckTick;
    private int lastStockpileTick;
    private IntVec3 lastTransitionCell = IntVec3.Invalid;

    public RoadboundGameComponent(Game game)
    {
    }

    public override void GameComponentTick()
    {
        TryHandlePersonalStockpiles();

        if (Current.Game?.CurrentMap == null || !WorldRendererUtility.DrawingMap)
        {
            return;
        }

        if (Find.TickManager.TicksGame - lastCheckTick < CheckInterval)
        {
            return;
        }

        lastCheckTick = Find.TickManager.TicksGame;
        Pawn pawn = Find.Selector.SelectedPawns.FirstOrDefault(p => p.Drafted && p.IsColonistPlayerControlled);
        if (pawn == null || pawn.Map == null || !MapEdgeUtility.IsOnEdge(pawn.Position, pawn.Map) || pawn.Position == lastTransitionCell)
        {
            return;
        }

        Direction8Way dir = MapEdgeUtility.DirectionFromCenter(pawn.Map, pawn.Position);
        int targetTile = ResolveTargetTile(pawn.Map, dir);
        if (!MapEdgeUtility.IsWalkableTile(targetTile))
        {
            return;
        }

        if (RoadboundWorldMod.Settings.showPrompt)
        {
            string body = "RBW_ConfirmBody".Translate(MapEdgeUtility.DescribeTile(targetTile));
            Find.WindowStack.Add(new Dialog_MessageBox(
                body,
                "RBW_ConfirmAccept".Translate(),
                () => BeginTransition(pawn, targetTile, dir),
                "RBW_ConfirmCancel".Translate(),
                () => { lastCheckTick = Find.TickManager.TicksGame + CheckInterval; }));
        }
        else
        {
            BeginTransition(pawn, targetTile, dir);
        }
    }

    private void TryHandlePersonalStockpiles()
    {
        if (!RoadboundWorldMod.Settings.personalStockpileMode || Find.TickManager.TicksGame - lastStockpileTick < StockpileCheckInterval)
        {
            return;
        }

        lastStockpileTick = Find.TickManager.TicksGame;
        foreach (Map map in Find.Maps)
        {
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                PersonalInventoryStockpileSystem.TryAutoStashNearbyHaulables(pawn);
            }
        }
    }

    private int ResolveTargetTile(Map map, Direction8Way dir)
    {
        RoadboundMapComponent component = map.GetComponent<RoadboundMapComponent>();
        if (component != null && component.TryGetConnectedTile(dir, out int linkedTile))
        {
            return linkedTile;
        }

        return MapEdgeUtility.GetNeighborTile(map.Tile, dir);
    }

    private void BeginTransition(Pawn triggerPawn, int targetTile, Direction8Way dir)
    {
        var worldState = Find.World.GetComponent<RoadWorldComponent>();
        worldState.SetPendingTransition(new RoadTransitionRequest
        {
            fromTile = Find.CurrentMap.Tile,
            targetTile = targetTile,
            exitDirection = dir,
            sourceCell = triggerPawn.Position,
            sourceMapSize = Find.CurrentMap.Size,
            createdTick = Find.TickManager.TicksGame,
        });

        var generator = new RoadMapGenerator();
        generator.Generate(targetTile);
        FinalizeTravel(generator.GeneratedMap);
        lastTransitionCell = triggerPawn.Position;
    }

    private void FinalizeTravel(Map targetMap)
    {
        List<Pawn> leavingPawns = Find.Selector.SelectedPawns.Where(p => p.IsColonistPlayerControlled).ToList();
        if (leavingPawns.Count == 0)
        {
            return;
        }

        Pawn leadPawn = leavingPawns[0];
        Map previousMap = leadPawn.Map;
        Caravan caravan = CaravanExitMapUtility.ExitMapAndCreateCaravan(
            leavingPawns,
            Faction.OfPlayer,
            previousMap.Tile,
            Direction8Way.North,
            targetMap.Tile,
            sendMessage: false);

        IntVec3 cameraCell;
        System.Predicate<IntVec3> validator = MapEdgeUtility.GetEntryValidator(targetMap, leadPawn.Position, previousMap.Size, out cameraCell);
        CaravanEnterMapUtility.Enter(caravan, targetMap, CaravanEnterMode.Edge, extraCellValidator: validator, draftColonists: true);
        Current.Game.CurrentMap = targetMap;
        Find.CameraDriver.JumpToCurrentMapLoc(cameraCell);
    }
}
