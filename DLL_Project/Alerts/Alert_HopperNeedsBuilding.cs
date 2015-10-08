using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace CommunityCoreLibrary
{
    
	public class Alert_HopperNeedsBuilding : Alert_High
    {

        public override AlertReport Report
        {
            get
            {
				var buildings =
					Find.ListerBuildings.allBuildingsColonist
						.Where( b => (
							( b.def.IsHopper() )
						) ).ToList();

                foreach( var building in buildings )
                {
					var hopperComp = building.GetComp<CompHopper>();
					if( hopperComp.FindHopperUser() == null )
                    {
                        this.baseExplanation = "Alert_HopperNeedsBuilding_Description".Translate( building.def.label );
						return AlertReport.CulpritIs( building );
                    }
                }
                return AlertReport.Inactive;
            }
        }

		public Alert_HopperNeedsBuilding()
        {
            this.baseLabel = "Alert_HopperNeedsBuilding_Label".Translate();
        }
    }
}
