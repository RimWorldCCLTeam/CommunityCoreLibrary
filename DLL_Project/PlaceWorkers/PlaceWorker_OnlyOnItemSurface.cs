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
	public class PlaceWorker_OnlyOnItemSurface : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing( EntityDef checkingDef, IntVec3 loc, Rot4 rot )
		{
			bool placementOk = false;
			List<Thing> allThings = Find.ThingGrid.ThingsListAt( loc );
			foreach( Thing curThing in allThings )
			{
				if( curThing.def.itemSurface == true )
				{
					placementOk = true;
					break;
				}
			}
			AcceptanceReport result;
			if( placementOk )
			{
				result = true;
			}
			else
			{
				result = "MessagePlacementItemSurface".Translate();
			}
			return result;
		}
	}
}

