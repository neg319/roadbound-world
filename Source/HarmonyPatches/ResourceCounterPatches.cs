using HarmonyLib;
using RimWorld;
using RoadboundWorld.Systems;
using Verse;

namespace RoadboundWorld.HarmonyPatches;

[HarmonyPatch(typeof(ResourceCounter), nameof(ResourceCounter.GetCount))]
public static class ResourceCounterPatches
{
    public static void Postfix(ThingDef r, ref int __result)
    {
        if (!RoadboundWorldMod.Settings.personalStockpileMode || r == null)
        {
            return;
        }

        Map map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }

        foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
        {
            __result += PersonalInventoryStockpileSystem.CountThingInInventory(pawn, r);
        }
    }
}
