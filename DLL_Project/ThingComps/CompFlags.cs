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
	public class CompFoodSource : ThingComp
	{
		// This marks a building as a food source for
		// Alert_NeedMealSourceComplete
	}
	public class CompHopper : ThingComp
	{
		// This marks a storage building as a hopper for
		// machines which need hoppers suchs as
		// NutrientPasteDispansers
	}
}