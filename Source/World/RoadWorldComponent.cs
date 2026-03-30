using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoadboundWorld.World;

public sealed class RoadWorldComponent : global::RimWorld.Planet.WorldComponent
{
    public RoadTransitionRequest PendingTransition;
    public List<PersistentHostileGroup> persistentHostiles = new();

    public RoadWorldComponent(global::RimWorld.Planet.World world) : base(world)
    {
    }

    public override void ExposeData()
    {
        Scribe_Deep.Look(ref PendingTransition, nameof(PendingTransition));
        Scribe_Collections.Look(ref persistentHostiles, nameof(persistentHostiles), LookMode.Deep);
    }

    public void SetPendingTransition(RoadTransitionRequest request)
    {
        PendingTransition = request;
    }

    public RoadTransitionRequest ConsumePendingTransitionForTile(int tile)
    {
        if (PendingTransition == null || PendingTransition.targetTile != tile)
        {
            return null;
        }

        var request = PendingTransition;
        PendingTransition = null;
        return request;
    }

    public void AddHostileGroup(PersistentHostileGroup group)
    {
        persistentHostiles.Add(group);
    }

    public List<PersistentHostileRecord> ConsumeHostiles(int tile, Rot4 entryDirection)
    {
        int now = Find.TickManager.TicksGame;
        persistentHostiles.RemoveAll(g => g.expiresAtTick <= now);

        var match = persistentHostiles.FirstOrDefault(g => g.tile == tile && g.entryDirection == entryDirection.AsInt);
        if (match == null)
        {
            return new List<PersistentHostileRecord>();
        }

        persistentHostiles.Remove(match);
        return match.pawns ?? new List<PersistentHostileRecord>();
    }
}
