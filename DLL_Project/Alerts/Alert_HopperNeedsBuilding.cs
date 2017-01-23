using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class Alert_HopperNeedsBuilding : Alert
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
                            where building.def.IsHopper()
                            select building;

            foreach ( var building in buildings )
            {
                var hopperComp = building.GetComp<CompHopper>();
                if ( hopperComp.FindHopperUser() == null )
                {
                    this.defaultExplanation = "Alert_HopperNeedsBuilding_Description".Translate( building.def.label );
                    return AlertReport.CulpritIs( building );
                }
            }

            return AlertReport.Inactive;
        }

        public Alert_HopperNeedsBuilding()
        {
            this.defaultLabel = "Alert_HopperNeedsBuilding_Label".Translate();
        }
    }
}
