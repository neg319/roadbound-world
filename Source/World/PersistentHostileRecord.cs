using Verse;

namespace RoadboundWorld.World;

public sealed class PersistentHostileRecord : IExposable
{
    public string kindDefName = "";
    public string factionDefName = "";
    public int count = 1;

    public void ExposeData()
    {
        Scribe_Values.Look(ref kindDefName, nameof(kindDefName), "");
        Scribe_Values.Look(ref factionDefName, nameof(factionDefName), "");
        Scribe_Values.Look(ref count, nameof(count), 1);
    }
}
