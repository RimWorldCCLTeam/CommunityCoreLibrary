using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Alert_NoTradingCapacity : RimWorld.Alert_NoTradingCapacity
    {

        public override AlertReport Report
        {
            get
            {
                if( GenDate.DaysPassed < 5 )
                {
                    return AlertReport.Inactive;
                }
                if( Find.ListerBuildings.allBuildingsColonist.Any( b => (
                    ( b.def.thingClass == typeof( Building_CommsConsole ) )||
                    ( b.def.thingClass.IsSubclassOf( typeof( Building_CommsConsole ) ) )
                ) ) )
                {
                    return AlertReport.Inactive;
                }
                return AlertReport.Active;
            }
        }

    }
}
