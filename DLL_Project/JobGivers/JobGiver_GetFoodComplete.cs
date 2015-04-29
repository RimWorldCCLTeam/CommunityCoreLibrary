using System;
using System.Reflection;
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

	public class JobGiver_GetFood : JobGiver_GetFood
	{
		public override float GetPriority( Pawn pawn )
		{
			Need_Food needFood = pawn.needs.food;
			return ( needFood == null )||( (double) needFood.CurLevel >= (double) needFood.ThreshHungry + 0.0199999995529652 ? 0.0f : 9.5f );
		}

		protected override Job TryGiveTerminalJob( Pawn pawn )
		{
			Thing thing1 = FoodUtility.FoodInInventory( pawn );
			if( thing1 != null )
				return new Job(JobDefOf.Ingest, (TargetInfo) thing1)
				{
					maxNumToCarry = FoodUtility.WillEatStackCountOf(pawn, thing1.def)
				};
			ThingDef foodDef;
			Thing thing2 = FoodUtility.BestFoodSourceFor(pawn, pawn, false, out foodDef);
			if (thing2 == null)
				return (Job) null;
			Building_NutrientPasteDispenser nutrientPasteDispenser = thing2 as Building_NutrientPasteDispenser;
			if (nutrientPasteDispenser != null && !nutrientPasteDispenser.HasEnoughFoodInHoppers())
			{
				Building building = nutrientPasteDispenser.AdjacentReachableHopper(pawn);
				if (building == null)
				{
					thing2 = FoodUtility.BestFoodInWorldFor(pawn, pawn);
					if (thing2 == null)
						return (Job) null;
				}
				else
				{
					SlotGroupParent hopperSgp = building as SlotGroupParent;
					Job job = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
					if (job != null)
						return job;
					thing2 = FoodUtility.BestFoodInWorldFor(pawn, pawn);
					if (thing2 == null)
						return (Job) null;
					foodDef = thing2.def;
				}
			}
			return new Job(JobDefOf.Ingest, (TargetInfo) thing2)
			{
				maxNumToCarry = FoodUtility.WillEatStackCountOf(pawn, foodDef)
			};
		}
	}
}
