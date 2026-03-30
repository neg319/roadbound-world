using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoadboundWorld.Systems;

public static class RoadPoiSpawner
{
    public static void SpawnPois(Map map, List<IntVec3> roadCells, int poiCount)
    {
        if (poiCount <= 0 || roadCells == null || roadCells.Count == 0)
        {
            return;
        }

        ThingDef[] stashCandidates =
        {
            ThingDefOf.Steel,
            ThingDefOf.WoodLog,
            ThingDefOf.PackageSurvivalMeal,
            ThingDefOf.ComponentIndustrial,
        };

        ThingDef campfire = DefDatabase<ThingDef>.GetNamedSilentFail("Campfire");
        var samples = roadCells.Where((_, i) => i % System.Math.Max(1, roadCells.Count / poiCount) == 0).Take(poiCount).ToList();

        foreach (IntVec3 roadCell in samples)
        {
            IntVec3 poiCell = CellFinder.RandomClosewalkCellNear(roadCell, map, 8);
            ThingDef pick = stashCandidates.RandomElement();
            Thing thing = ThingMaker.MakeThing(pick);
            thing.stackCount = pick.stackLimit > 1 ? System.Math.Min(pick.stackLimit, Rand.RangeInclusive(8, 30)) : 1;
            GenSpawn.Spawn(thing, poiCell, map);

            if (campfire != null)
            {
                IntVec3 fireCell = CellFinder.RandomClosewalkCellNear(poiCell, map, 2);
                if (fireCell.Standable(map))
                {
                    GenSpawn.Spawn(ThingMaker.MakeThing(campfire), fireCell, map);
                }
            }
        }
    }
}
