using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadboundWorld.World;

public sealed class RoadMapGenerator
{
    public Map GeneratedMap { get; private set; }

    public void Generate(int targetTile)
    {
        WorldObjectDef transitDef = DefDatabase<WorldObjectDef>.GetNamedSilentFail("RoadboundTransitSite")
                                     ?? DefDatabase<WorldObjectDef>.GetNamed("RoadboundTransitSite");

        var existingObjects = Find.WorldObjects.AllWorldObjects.Where(w => w.Tile == targetTile).ToList();
        var settlement = existingObjects.OfType<Settlement>().FirstOrDefault();

        if (settlement != null)
        {
            GeneratedMap = GetOrGenerateMapUtility.GetOrGenerateMap(targetTile, settlement.def);
            return;
        }

        var existingMapParent = existingObjects.OfType<MapParent>().FirstOrDefault();
        if (existingMapParent != null)
        {
            GeneratedMap = GetOrGenerateMapUtility.GetOrGenerateMap(targetTile, existingMapParent.def);
            return;
        }

        IntVec3 size = new(RoadboundWorldMod.Settings.mapSize, 1, RoadboundWorldMod.Settings.mapSize);
        GeneratedMap = GetOrGenerateMapUtility.GetOrGenerateMap(targetTile, size, transitDef);
    }
}
