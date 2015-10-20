using System.Collections.Generic;

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
