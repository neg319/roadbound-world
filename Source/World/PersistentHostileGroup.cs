using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RoadboundWorld.World;

public sealed class PersistentHostileGroup : IExposable
{
    public int tile = -1;
    public Direction8Way entryDirection = Direction8Way.North;
    public int expiresAtTick;
    public List<PersistentHostileRecord> pawns = new();

    public void ExposeData()
    {
        Scribe_Values.Look(ref tile, nameof(tile), -1);
        Scribe_Values.Look(ref entryDirection, nameof(entryDirection), Direction8Way.North);
        Scribe_Values.Look(ref expiresAtTick, nameof(expiresAtTick), 0);
        Scribe_Collections.Look(ref pawns, nameof(pawns), LookMode.Deep);
    }
}
