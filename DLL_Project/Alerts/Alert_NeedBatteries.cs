using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Alert_NeedBatteries : RimWorld.Alert_NeedBatteries
    {

        static bool                         CheckThing( Thing thing )
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

        public override AlertReport         Report
        {
            get
            {
                if( Find.ListerBuildings.ColonistsHaveBuilding( (thing) =>
                {
                    if(
                        ( thing is Building_Battery )&&
                        ( thing.def.HasComp( typeof( CompPowerBattery ) ) )
                    )
                    {   // Building is a battery
                        return true;
                    }
                    if(
                        ( thing.def.HasComp( typeof( CompPowerPlant ) ) )||
                        ( thing.def.HasComp( typeof( CompPowerPlantWind ) ) )||
                        ( thing.def.HasComp( typeof( CompPowerPlantSolar ) ) )||
                        ( thing.def.HasComp( typeof( CompPowerPlantSteam) ) )
                    )
                    {   // Building is a power plant
                        return true;
                    }
                    return false;
                } ) )
                {
                    return AlertReport.Inactive;
                }
                // Check for individual power trader which is low
                var powerTraders = Find.ListerBuildings.allBuildingsColonist.FindAll( CheckThing );
                if( ( powerTraders != null )&&
                    ( powerTraders.Count > 0 ) )
                {
                    return AlertReport.CulpritIs( powerTraders.RandomElement() );
                }

                // All power trader's good
                return AlertReport.Inactive;
            }
        }

    }

}
