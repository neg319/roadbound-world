using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace RoadboundWorld.Systems;

public static class PersonalInventoryStockpileSystem
{
    public static void TryAutoStashNearbyHaulables(Pawn pawn)
    {
        if (pawn?.MapHeld == null || pawn.inventory?.innerContainer == null)
        {
            return;
        }

        if (!RoadboundWorldMod.Settings.personalStockpileMode || !pawn.IsColonistPlayerControlled || pawn.Dead || pawn.Downed)
        {
            return;
        }

        float remainingMass = Math.Max(0f, MassUtility.Capacity(pawn) - MassUtility.GearAndInventoryMass(pawn));
        if (remainingMass <= 0.5f)
        {
            return;
        }

        foreach (IntVec3 cell in GenRadial.RadialCellsAround(pawn.PositionHeld, RoadboundWorldMod.Settings.personalStockpileSearchRadius, true))
        {
            if (!cell.InBounds(pawn.MapHeld))
            {
                continue;
            }

            var things = cell.GetThingList(pawn.MapHeld);
            for (int i = things.Count - 1; i >= 0; i--)
            {
                Thing thing = things[i];
                if (!ShouldStash(pawn, thing))
                {
                    continue;
                }

                float massPerItem = Math.Max(0.01f, thing.GetStatValue(StatDefOf.Mass, true));
                int takeCount = thing.stackCount;
                if (massPerItem > 0f)
                {
                    takeCount = Math.Min(takeCount, Math.Max(1, (int)Math.Floor(remainingMass / massPerItem)));
                }

                if (takeCount <= 0)
                {
                    return;
                }

                Thing taken = thing.stackCount > takeCount ? thing.SplitOff(takeCount) : thing;
                if (!pawn.inventory.innerContainer.TryAdd(taken))
                {
                    if (!taken.Spawned)
                    {
                        GenPlace.TryPlaceThing(taken, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near);
                    }
                    continue;
                }

                remainingMass -= massPerItem * takeCount;
                if (remainingMass <= 0.5f)
                {
                    return;
                }
            }
        }
    }

    private static bool ShouldStash(Pawn pawn, Thing thing)
    {
        if (thing == null || !thing.Spawned || thing.Destroyed || thing.def == null)
        {
            return false;
        }

        if (!thing.def.EverHaulable || thing is Pawn || thing.def.category == ThingCategory.Building)
        {
            return false;
        }

        if (thing.IsForbidden(Faction.OfPlayer) || thing.PositionHeld.Fogged(pawn.MapHeld))
        {
            return false;
        }

        if (!pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None))
        {
            return false;
        }

        if (thing.def.Minifiable || thing is Corpse)
        {
            return false;
        }

        return true;
    }
}
