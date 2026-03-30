using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RoadboundWorld.Systems;

public static class RoadPainter
{
    public static List<IntVec3> Paint(Map map, IntVec3 start, IntVec3 end, int halfWidth)
    {
        List<IntVec3> cells = new();
        TerrainDef terrain = DefDatabase<TerrainDef>.GetNamedSilentFail("PackedDirt")
                             ?? DefDatabase<TerrainDef>.GetNamedSilentFail("Gravel")
                             ?? TerrainDefOf.Soil;

        Vector3 p0 = start.ToVector3Shifted();
        Vector3 p3 = end.ToVector3Shifted();
        Vector3 p1 = Vector3.Lerp(p0, p3, 0.33f) + new Vector3(Rand.Range(-10f, 10f), 0f, Rand.Range(-10f, 10f));
        Vector3 p2 = Vector3.Lerp(p0, p3, 0.66f) + new Vector3(Rand.Range(-10f, 10f), 0f, Rand.Range(-10f, 10f));

        const int samples = 80;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 point = Bezier(p0, p1, p2, p3, t);
            IntVec3 center = point.ToIntVec3();

            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                for (int dz = -halfWidth; dz <= halfWidth; dz++)
                {
                    if (dx * dx + dz * dz > halfWidth * halfWidth + halfWidth)
                    {
                        continue;
                    }

                    IntVec3 c = new(center.x + dx, 0, center.z + dz);
                    if (!c.InBounds(map))
                    {
                        continue;
                    }

                    map.terrainGrid.SetTerrain(c, terrain);
                    map.roofGrid.SetRoof(c, null);
                    if (map.thingGrid.ThingsListAtFast(c).Count > 0)
                    {
                        foreach (Thing thing in map.thingGrid.ThingsListAtFast(c).ToList())
                        {
                            if (thing.def.category == ThingCategory.Plant)
                            {
                                thing.Destroy();
                            }
                        }
                    }

                    cells.Add(c);
                }
            }
        }

        return cells;
    }

    private static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
    }
}
