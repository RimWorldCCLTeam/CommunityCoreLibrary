using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class Alert_BuildingNeedsHopper : Alert
    {
        public override AlertPriority Priority
        {
            get
            {
                return AlertPriority.High;
            }
        }

        public override AlertReport GetReport()
        {
            var buildings = from map in Find.Maps
                            where map.IsPlayerHome
                            from building in map.listerBuildings.allBuildingsColonist
                            where building.def.IsHopperUser()
                            select building;

            foreach ( var building in buildings )
            {
                var userComp = building.GetComp<CompHopperUser>();
                if ( userComp.FindHoppers().NullOrEmpty() )
                {
                    this.defaultExplanation = "Alert_BuildingNeedsHopper_Description".Translate( building.def.label );
                    return AlertReport.CulpritIs( building );
                }
            }

            return AlertReport.Inactive;
        }

        public Alert_BuildingNeedsHopper()
        {
            this.defaultLabel = "Alert_BuildingNeedsHopper_Label".Translate();
        }
    }
}
