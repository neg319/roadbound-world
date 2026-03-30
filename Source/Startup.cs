using HarmonyLib;
using Verse;

namespace RoadboundWorld;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        var harmony = new Harmony("vyberware.roadboundworld");
        harmony.PatchAll();
    }
}
