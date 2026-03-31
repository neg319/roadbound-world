using System.Collections.Generic;
using Verse;

namespace RoadboundWorld.World;

public sealed class PersistentHostileGroup : IExposable
{
    public int tile;
    public int entryDirection;
    public int expiresAtTick;
    public List<PersistentHostileRecord> pawns = new();

    public void ExposeData()
    {
        Scribe_Values.Look(ref tile, nameof(tile));
        Scribe_Values.Look(ref entryDirection, nameof(entryDirection));
        Scribe_Values.Look(ref expiresAtTick, nameof(expiresAtTick));
        Scribe_Collections.Look(ref pawns, nameof(pawns), LookMode.Deep);
    }
}
