using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RoadboundWorld.Systems;

public static class RoadTravelerSpawner
{
    public static void TrySpawnTravelers(Map map, List<IntVec3> roadCells, Direction8Way entryDirection, Direction8Way exitDirection)
    {
        if (roadCells == null || roadCells.Count == 0)
        {
            return;
        }

        Faction faction = Find.FactionManager.AllFactionsVisible
            .Where(f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction)
            .RandomElementWithFallback();

        if (faction == null)
        {
            return;
        }

        var parms = new PawnGroupMakerParms
        {
            faction = faction,
            groupKind = PawnGroupKindDefOf.Trader,
            tile = map.Tile,
            points = 120f,
        };

        List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(parms).ToList();
        if (pawns.Count == 0)
        {
            return;
        }

        IntVec3 entry = EdgeCell(map, entryDirection);
        IntVec3 exit = EdgeCell(map, exitDirection);

        foreach (Pawn pawn in pawns)
        {
            IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(entry, map, 6);
            GenSpawn.Spawn(pawn, spawnCell, map);
            Job job = JobMaker.MakeJob(JobDefOf.Goto, exit);
            job.expiryInterval = 6000;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }
    }

    private static IntVec3 EdgeCell(Map map, Direction8Way direction)
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
}
