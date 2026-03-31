using Verse;

namespace RoadboundWorld;

public sealed class RoadboundWorldSettings : ModSettings
{
    public bool showPrompt = true;
    public int mapSize = 120;
    public int roadHalfWidth = 3;
    public int poiCount = 3;
    public float travelerIntervalDays = 1.2f;
    public float persistentHostileDays = 1.5f;
    public bool morrowindInventoryUi = true;
    public bool personalStockpileMode = true;
    public float personalInventoryCapacityMultiplier = 25f;
    public int personalStockpileSearchRadius = 10;
    public bool globalMorrowindWindows = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref showPrompt, nameof(showPrompt), true);
        Scribe_Values.Look(ref mapSize, nameof(mapSize), 120);
        Scribe_Values.Look(ref roadHalfWidth, nameof(roadHalfWidth), 3);
        Scribe_Values.Look(ref poiCount, nameof(poiCount), 3);
        Scribe_Values.Look(ref travelerIntervalDays, nameof(travelerIntervalDays), 1.2f);
        Scribe_Values.Look(ref persistentHostileDays, nameof(persistentHostileDays), 1.5f);
        Scribe_Values.Look(ref morrowindInventoryUi, nameof(morrowindInventoryUi), true);
        Scribe_Values.Look(ref personalStockpileMode, nameof(personalStockpileMode), true);
        Scribe_Values.Look(ref personalInventoryCapacityMultiplier, nameof(personalInventoryCapacityMultiplier), 25f);
        Scribe_Values.Look(ref personalStockpileSearchRadius, nameof(personalStockpileSearchRadius), 10);
        Scribe_Values.Look(ref globalMorrowindWindows, nameof(globalMorrowindWindows), true);
    }

    public void Reset()
    {
        showPrompt = true;
        mapSize = 120;
        roadHalfWidth = 3;
        poiCount = 3;
        travelerIntervalDays = 1.2f;
        persistentHostileDays = 1.5f;
        morrowindInventoryUi = true;
        personalStockpileMode = true;
        personalInventoryCapacityMultiplier = 25f;
        personalStockpileSearchRadius = 10;
        globalMorrowindWindows = true;
    }
}
