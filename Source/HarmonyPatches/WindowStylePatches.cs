using HarmonyLib;
using RoadboundWorld.UI;
using UnityEngine;
using Verse;

namespace RoadboundWorld.HarmonyPatches;

[HarmonyPatch(typeof(Widgets), nameof(Widgets.DrawWindowBackground), new[] { typeof(Rect) })]
public static class WindowStylePatches
{
    public static bool Prefix(Rect rect)
    {
        if (!RoadboundWorldMod.Settings.globalMorrowindWindows)
        {
            return true;
        }

        MorrowindWindowSkin.DrawWindow(rect);
        return false;
    }
}
