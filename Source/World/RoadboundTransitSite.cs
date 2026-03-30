using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RoadboundWorld.World;

public class RoadboundTransitSite : Camp
{
    private bool affectedByPlayer;
    public bool taskRemove;

    public override void Notify_MyMapRemoved(Map map)
    {
        base.Notify_MyMapRemoved(map);
    }

    public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
    {
        if (taskRemove)
        {
            alsoRemoveWorldObject = true;
            return true;
        }

        if (!Map.mapPawns.AnyPawnBlockingMapRemoval)
        {
            if (!affectedByPlayer)
            {
                affectedByPlayer = Map.listerBuildings.allBuildingsColonist.Any()
                    || Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Any(t => t.Faction == Faction.OfPlayer)
                    || Map.zoneManager.AllZones.Any(z => z is Zone_Stockpile or Zone_Growing);
            }

            if (!affectedByPlayer)
            {
                alsoRemoveWorldObject = true;
                return true;
            }
        }

        alsoRemoveWorldObject = false;
        return false;
    }
}
