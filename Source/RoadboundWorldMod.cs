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
        listing.Label("Road travel systems and the Morrowind style inventory replacement live in this source package. This environment still could not compile the C# assembly.");

        listing.End();
    }
}
