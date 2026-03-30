using RimWorld;
using Verse;

namespace RoadboundWorld.World;

public sealed class RoadTransitionRequest : IExposable
{
    public int fromTile = -1;
    public int targetTile = -1;
    public Direction8Way exitDirection = Direction8Way.North;
    public IntVec3 sourceCell = IntVec3.Invalid;
    public IntVec3 sourceMapSize = IntVec3.Invalid;
    public int createdTick;

    public void ExposeData()
    {
        Scribe_Values.Look(ref fromTile, nameof(fromTile), -1);
        Scribe_Values.Look(ref targetTile, nameof(targetTile), -1);
        Scribe_Values.Look(ref exitDirection, nameof(exitDirection), Direction8Way.North);
        Scribe_Values.Look(ref sourceCell, nameof(sourceCell));
        Scribe_Values.Look(ref sourceMapSize, nameof(sourceMapSize));
        Scribe_Values.Look(ref createdTick, nameof(createdTick), 0);
    }
}
