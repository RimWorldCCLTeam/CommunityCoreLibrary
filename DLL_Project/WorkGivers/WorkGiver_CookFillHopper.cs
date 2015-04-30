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

	public class WorkGiver_CookFillHopper : WorkGiver
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				// Each time this is called, it will try a different
				// def to try to fill.  This will just keep cycling
				// through in a loop through the list.
				return ThingRequest.ForDef( HopperDefs.NextDef );
			}
		}

		public override PathMode PathMode
		{
			get
			{
				return PathMode.ClosestTouch;
			}
		}

		public override Job JobOnThing( Pawn pawn, Thing thing )
		{
			// Hopper has a slot slotgroup (and therefore is a hopper)?
			SlotGroupParent hopperSgp = thing as SlotGroupParent;
			if( hopperSgp == null )
				return (Job) null;

			// Is it reservable?
			if( !ReservationUtility.CanReserve( pawn, (TargetInfo) thing.Position, 1 ) )
				return (Job) null;

			// Check to see if we should haul some more
			int totalIngredients = 0;
			List<Thing> potentialIngredients = Find.ThingGrid.ThingsListAt( thing.Position );
			for( int index = 0; index < potentialIngredients.Count; ++index )
			{
				// Add potential ingredients stack count to the total count
				if( potentialIngredients[ index ].def.IsNutritionSource )
					totalIngredients += potentialIngredients[ index ].stackCount;
			}

			// Below threshold, try to fill it
			if( totalIngredients <= 25 )
				return CommunityCoreLibrary.WorkGiver_CookFillHopper.HopperFillFoodJob( pawn, hopperSgp );

			// Doesn't need anymore
			JobFailReason.Is( Translator.Translate( "AlreadyFilledLower" ) );
			return (Job) null;
		}

		public static Job HopperFillFoodJob( Pawn pawn, SlotGroupParent hopperSgp )
		{
			// Building from slotgroup
			Building building = hopperSgp as Building;

			// Can we reach it?
			if( !ReservationUtility.CanReserveAndReach( pawn, (TargetInfo) building.Position, PathMode.Touch, DangerUtility.NormalMaxDanger(pawn), 1 ) )
				return (Job) null;

			// Find first thing here
			ThingDef def = (ThingDef) null;
			Thing firstItem = GridsUtility.GetFirstItem( building.Position );
			if( firstItem != null )
			{
				// Is it a nutrition source?
				if( firstItem.def.IsNutritionSource )
				{
					// Use it!
					def = firstItem.def;
				}
				else
				{
					// Move it!
					if( GenForbid.IsForbidden( firstItem, pawn.Faction ) )
						return (Job) null;
					return HaulAIUtility.HaulAsideJobFor( pawn, firstItem );
				}
			}

			// Find the same nutrition source or any nutrition source if the hopper is empty
			List<Thing> list = def != null ? Find.Map.listerThings.ThingsOfDef( def ) : Find.Map.listerThings.ThingsInGroup( ThingRequestGroup.FoodNotPlant );

			// Now itterate the list and look for the best option
			for( int index = 0; index < list.Count; ++index )
			{
				Thing t = list[ index ];
				// Is it raw, haulable, allowed and less a priority for it's current storage?
				if( ( t.def.ingestible.preferability == AIFoodPreferability.Raw )&&
					( HaulAIUtility.PawnCanAutomaticallyHaul( pawn, t ) )&&
					( Find.SlotGroupManager.SlotGroupAt( building.Position ).Settings.AllowedToAccept( t ) )&&
					( HaulAIUtility.StoragePriorityAtFor( t.Position, t ) < hopperSgp.GetSlotGroup().Settings.Priority ) )
				{
					// Then haul it here!
					Job job = HaulAIUtility.HaulMaxNumToCellJob( pawn, t, building.Position, true );
					if( job != null )
						return job;
				}
			}
			return (Job) null;
		}
	}
}
