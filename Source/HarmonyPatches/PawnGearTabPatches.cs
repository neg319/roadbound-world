using RimWorld;
using System.Reflection;
using HarmonyLib;
using RoadboundWorld.UI;
using UnityEngine;
using Verse;

namespace RoadboundWorld.HarmonyPatches;

[HarmonyPatch(typeof(ITab_Pawn_Gear), "FillTab")]
public static class PawnGearTabPatches
{
    private static readonly FieldInfo SizeField = AccessTools.Field(typeof(ITab), "size");

    public static bool Prefix(ITab_Pawn_Gear __instance)
    {
        if (Current.ProgramState != ProgramState.Playing || !RoadboundWorldMod.Settings.morrowindInventoryUi)
        {
            return true;
        }

        if (Find.Selector.SingleSelectedThing is not Pawn pawn)
        {
            return true;
        }

        SizeField?.SetValue(__instance, new Vector2(980f, 720f));
        Rect rect = new(0f, 0f, 980f, 720f);
        MorrowindGearTabRenderer.Draw(rect.ContractedBy(6f), pawn);
        return false;
    }
}
