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

	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		public override float GetPriority( Pawn pawn )
		{
			Need_Food needFood = pawn.needs.food;
			return needFood == null || (double) needFood.CurLevel >= (double) needFood.ThreshHungry + 0.0199999995529652 ? 0.0f : 9.5f;
		}

		protected override Job TryGiveTerminalJob( Pawn pawn )
		{
			Thing myFood = CommunityCoreLibrary.FoodUtility.FoodInInventory( pawn );
			if( myFood != null )
				return new Job( JobDefOf.Ingest, (TargetInfo) myFood )
				{
					maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( pawn, myFood.def )
				};
			
			ThingDef foodDef;
			Thing bestSource = CommunityCoreLibrary.FoodUtility.BestFoodSourceFor( pawn, pawn, false, out foodDef );
			if( bestSource == null )
				return null;

			// Check if it's a common NPD
			CommunityCoreLibrary.Building_NutrientPasteDispenser cclNPD = bestSource as CommunityCoreLibrary.Building_NutrientPasteDispenser;
			if( cclNPD != null )
			{
				// It is, does it have enough food?
				if( cclNPD.HasEnoughFoodInHoppers() )
				{
					// It does!  Use it!
					return new Job( JobDefOf.Ingest, (TargetInfo) bestSource )
					{
						maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( pawn, foodDef )
					};
				}
				// Nope, add some first
				Building hopper = cclNPD.AdjacentReachableHopper( pawn );
				if( hopper != null )
				{
					// Setup a fetch job to fill a hopper
					SlotGroupParent hopperSgp = hopper as SlotGroupParent;
					Job fetchForHopper = CommunityCoreLibrary.WorkGiver_CookFillHopper.HopperFillFoodJob( pawn, hopperSgp );
					if( fetchForHopper != null )
						return fetchForHopper;
					// Something borked, try something else then...
				}
			}

			// Check if it's a vanilla NPD
			RimWorld.Building_NutrientPasteDispenser rwNPD = bestSource as RimWorld.Building_NutrientPasteDispenser;
			if( rwNPD != null )
			{
				// It is, does it have enough food?
				if( rwNPD.HasEnoughFoodInHoppers() )
				{
					// It does!  Use it!
					return new Job( JobDefOf.Ingest, (TargetInfo) bestSource )
					{
						maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( pawn, foodDef )
					};
				}
				// Nope, add some first
				Building hopper = rwNPD.AdjacentReachableHopper( pawn );
				if( hopper != null )
				{
					// Setup a fetch job to fill a hopper
					SlotGroupParent hopperSgp = hopper as SlotGroupParent;
					Job fetchForHopper = CommunityCoreLibrary.WorkGiver_CookFillHopper.HopperFillFoodJob( pawn, hopperSgp );
					if( fetchForHopper != null )
						return fetchForHopper;
					// Something borked, try something else then...
				}
			}

			// No NPDs we can use (for whatever reason), look for another food source
			bestSource = FoodUtility.BestFoodInWorldFor( pawn, pawn );
			if( bestSource == null )
				return (Job) null;

			// Found something, eat it!
			foodDef = bestSource.def;
			return new Job( JobDefOf.Ingest, (TargetInfo) bestSource )
			{
				maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( pawn, foodDef )
			};
		}
	}
}
