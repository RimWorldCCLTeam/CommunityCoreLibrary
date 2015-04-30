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
	public class Alert_NeedMealSourceCommon : Alert_NeedMealSource
	{
		public override AlertReport Report
		{
			get
			{
				// Too soon?
				if( GenDate.DaysPassed < 2 )
				{
					return false;
				}
				// Any building have CompFoodSource or a vanilla food source?
				if( Find.ListerBuildings.allBuildingsColonist.Any(
					( Building b )
					=> b.GetComp< CompFoodSource >() != null
					|| b.def == ThingDefOf.Campfire
					|| b.def == ThingDefOf.CookStove
					|| b.def == ThingDefOf.NutrientPasteDispenser
				) )
				{
					return false;
				}
				// Nothing, show it
				return true;
			}
		}
	}
}

