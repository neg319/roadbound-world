using RimWorld;
using UnityEngine;
using Verse;

namespace RoadboundWorld;

public sealed class RoadboundWorldMod : Mod
{
    public static RoadboundWorldSettings Settings;

    public RoadboundWorldMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<RoadboundWorldSettings>();
    }

    public override string SettingsCategory() => "RBW_ModName".Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.CheckboxLabeled("RBW_ShowPrompt".Translate(), ref Settings.showPrompt);
        listing.CheckboxLabeled("RBW_MorrowindInventory".Translate(), ref Settings.morrowindInventoryUi);
        listing.CheckboxLabeled("RBW_GlobalWindows".Translate(), ref Settings.globalMorrowindWindows);
        listing.CheckboxLabeled("RBW_PersonalStockpileMode".Translate(), ref Settings.personalStockpileMode);
        listing.Label($"{"RBW_CarryMultiplier".Translate()}: {Settings.personalInventoryCapacityMultiplier:F1}x");
        Settings.personalInventoryCapacityMultiplier = listing.Slider(Settings.personalInventoryCapacityMultiplier, 2f, 100f);

        listing.Label($"{"RBW_StockpileRadius".Translate()}: {Settings.personalStockpileSearchRadius}");
        Settings.personalStockpileSearchRadius = (int)listing.Slider(Settings.personalStockpileSearchRadius, 2, 24);

        listing.Label($"{"RBW_MapSize".Translate()}: {Settings.mapSize}");
        Settings.mapSize = (int)listing.Slider(Settings.mapSize, 60, 220);

        listing.Label($"{"RBW_RoadHalfWidth".Translate()}: {Settings.roadHalfWidth}");
        Settings.roadHalfWidth = (int)listing.Slider(Settings.roadHalfWidth, 2, 8);

        listing.Label($"{"RBW_PoiCount".Translate()}: {Settings.poiCount}");
        Settings.poiCount = (int)listing.Slider(Settings.poiCount, 0, 6);

        listing.Label($"{"RBW_TravelerDays".Translate()}: {Settings.travelerIntervalDays:F1}");
        Settings.travelerIntervalDays = listing.Slider(Settings.travelerIntervalDays, 0.2f, 5f);

        listing.Label($"{"RBW_PersistDays".Translate()}: {Settings.persistentHostileDays:F1}");
        Settings.persistentHostileDays = listing.Slider(Settings.persistentHostileDays, 0.2f, 5f);

        if (listing.ButtonText("Reset to defaults"))
        {
            Settings.Reset();
        }

        listing.GapLine();
        listing.Label("This update adds a live pawn portrait paper doll, keeps equipped gear visible inside the main inventory grid, restores backtracking across connected road maps, raises personal carry capacity, auto stashes nearby haulables into colonist inventories, and applies the Morrowind skin to more windows.");

        listing.End();
    }
}
