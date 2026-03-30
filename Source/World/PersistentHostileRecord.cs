using RimWorld;
using Verse;

namespace RoadboundWorld.World;

public sealed class PersistentHostileRecord : IExposable
{
    public PawnKindDef pawnKind;
    public Faction faction;
    public string sourceLabel;
    public float healthFraction = 1f;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref pawnKind, nameof(pawnKind));
        Scribe_References.Look(ref faction, nameof(faction));
        Scribe_Values.Look(ref sourceLabel, nameof(sourceLabel));
        Scribe_Values.Look(ref healthFraction, nameof(healthFraction), 1f);
    }
}
