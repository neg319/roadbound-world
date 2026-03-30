using System.Reflection;
using HarmonyLib;
using RoadboundWorld.World;
using Verse;

namespace RoadboundWorld.HarmonyPatches;

[HarmonyPatch(typeof(ExitMapGrid), nameof(ExitMapGrid.MapUsesExitGrid), MethodType.Getter)]
public static class ExitGridPatches
{
    private static readonly FieldInfo MapField = typeof(ExitMapGrid).GetField("map", BindingFlags.Instance | BindingFlags.NonPublic);

    public static bool Prefix(ExitMapGrid __instance, ref bool __result)
    {
        if (MapField?.GetValue(__instance) is not Map map)
        {
            return true;
        }

        if (map.Parent is RoadboundTransitSite)
        {
            __result = true;
            return false;
        }

        return true;
    }
}
