using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Alert_NeedBatteries : RimWorld.Alert_NeedBatteries
    {

        static bool CheckThing( Thing thing )
        {
            var p = thing.TryGetComp< CompPowerTrader >();

            // Only check for power traders
            // That are connected to a power network
            // Which aren't powered on
            // But want to be powered on
            return
                ( p != null )&&
                ( p.PowerNet != null )&&
                ( !p.PowerOn )&&
                ( p.DesirePowerOn );

        }

        public override AlertReport GetReport()
        {
            //TODO: the logic here isn't quite right - it should be doing this on a map by map basis
            var lists = from map in Find.Maps
                        where map.IsPlayerHome
                        select map.listerBuildings;

            if ( lists.Any( list => list.ColonistsHaveBuilding( thing =>
                {
                    if ( thing is Building_Battery && thing.def.HasComp( typeof( CompPowerBattery ) ) )
                    {
                        return true;
                    }

                    if (
                        thing.def.HasComp( typeof( CompPowerPlant ) ) ||
                        thing.def.HasComp( typeof( CompPowerPlantWind ) ) ||
                        thing.def.HasComp( typeof( CompPowerPlantSolar ) ) ||
                        thing.def.HasComp( typeof( CompPowerPlantSteam ) ) )
                    {
                        // Building is a power plant
                        return true;
                    }

                    return false;
                } ) ) )
            {
                return AlertReport.Inactive;
            }

            // Check for individual power trader which is low
            var powerTraders = from map in Find.Maps
                               where map.IsPlayerHome
                               from building in map.listerBuildings.allBuildingsColonist
                               where CheckThing( building )
                               select building;

            if ( powerTraders.Count() > 0 )
            {
                return AlertReport.CulpritIs( powerTraders.RandomElement() );
            }

            // All power trader's good
            return AlertReport.Inactive;
        }
    }
}
