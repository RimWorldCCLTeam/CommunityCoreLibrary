using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
	public class PlaceWorker_OnlyUnderRoof : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing( EntityDef checkingDef, IntVec3 loc, Rot4 rot )
		{
			AcceptanceReport result;
			if( Find.RoofGrid.Roofed( loc ) )
			{
				result = true;
			}
			else
			{
				result = "MessagePlacementMustBeUnderRoof".Translate();
			}
			return result;
		}
	}
}

