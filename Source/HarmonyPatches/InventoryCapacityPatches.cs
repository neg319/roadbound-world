using HarmonyLib;
using RimWorld;
using Verse;

namespace RoadboundWorld.HarmonyPatches;

[HarmonyPatch(typeof(MassUtility), nameof(MassUtility.Capacity))]
public static class InventoryCapacityPatches
{
    public static void Postfix(Pawn p, ref float __result)
    {
        if (p == null || !RoadboundWorldMod.Settings.personalStockpileMode)
        {
            return;
        }

        if (!p.IsColonistPlayerControlled)
        {
            return;
        }

        __result *= RoadboundWorldMod.Settings.personalInventoryCapacityMultiplier;
    }
}
