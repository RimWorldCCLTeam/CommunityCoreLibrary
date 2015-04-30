using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

	public class Alert_BuildingNeedsHopper : Alert_PasteDispenserNeedsHopper
	{
		public Alert_BuildingNeedsHopper()
		{
			baseLabel = "NeedHopperAttached".Translate();
			baseExplanation = "NeedHopperAttachedDesc".Translate();
		}

		public override AlertReport Report
		{
			get
			{
				//string derp;
				foreach( Building building in Find.ListerBuildings.allBuildingsColonist )
				{
					bool thisNeedsHopper = false;
					//derp += building.def.defName + " - wantsHopperAdjacent == " + building.def.building.wantsHopperAdjacent ) + "\n";
					if( building.def.building.wantsHopperAdjacent )
					{
						thisNeedsHopper = true;
						foreach( IntVec3 c in GenAdj.CellsAdjacentCardinal( building ) )
						{
							Building hopper = GridsUtility.GetEdifice( c );
							if( ( hopper != null )&&(
								( hopper.def == ThingDefOf.Hopper )||
								( hopper.def.GetCompProperties( typeof( CompProperties_Hopper ) ) != null ) ) )
							{
								//derp += building.def.defName + " - Has hopper\n" );
								thisNeedsHopper = false;
								break;
							}
						}
					}
					if( thisNeedsHopper ){
						//Log.Message( derp );
						return AlertReport.CulpritIs( building );
					}
				}
				return false;
			}
		}
	}
}