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
	
	public class WorkGiver_Warden : WorkGiver
	{
		// Changed const to int incase modders want to be saddistic
		// and monkey with this.
		public int				PrisonerBeatingNumAttacks = 5;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup( ThingRequestGroup.Pawn );
			}
		}

		public override PathMode PathMode
		{
			get
			{
				return PathMode.OnCell;
			}
		}

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			// Is the thing not a pawn or;
			// is it not a prisoner or;
			// is it an unsecured prisoner who is held or;
			// the prisoner has gone berserk or;
			// the pawn can't reach the prisoner
			// Do nothing
			Pawn prisoner = t as Pawn;
			if( ( prisoner == null )||
				( !prisoner.IsPrisonerOfColony )||
				(   ( !prisoner.guest.PrisonerIsSecure )||
					( prisoner.holder != null ) )||
				( prisoner.BrokenStateDef == BrokenStateDefOf.Berserk )||
				( !ReservationUtility.CanReserveAndReach( 
					pawn, 
					(TargetInfo) ((Thing) prisoner ), 
					PathMode.OnCell, 
					DangerUtility.NormalMaxDanger( pawn ), 
					1 ) ) )
				return (Job) null;
			// It's a prisoner we can interact with

			// If the prisoner isn't down and;
			// the pawn can get to the prisoner and;
			// the prisoner can't use a medical bed or;
			//    it isn't in it's bed or;
			//    it's current bed isn't a medical bed
			if( ( !prisoner.Downed )&&
				( ReservationUtility.CanReserve( 
					pawn, 
					(TargetInfo) 
					((Thing) prisoner ), 
					1 ) )&&
				(   ( !prisoner.health.CanUseMedicalBed )||
					( !RestUtility.InBed( prisoner ) )||
					( !RestUtility.CurrentBed( prisoner ).Medical) ) )
			{
				// It has it's own bed and;
				// it's not in that bed
				bool wantsBed = 
					( prisoner.ownership.ownedBed != null )&&
					( RoomQuery.RoomAt( prisoner.ownership.ownedBed.Position ) != RoomQuery.RoomAt( prisoner.Position ) );
				// Assume the other pawn can't be put to bed
				bool putToBed = false;
				if( !wantsBed )
				{
					// Is it in a room which isn't against the edge of the map
					Room room = GridsUtility.GetRoom( (Thing) prisoner );
					if( ( room != null )&&( !room.TouchesMapEdge ) )
					{
						// Find all the beds in the room
						foreach( Building_Bed buildingBed in room.ContainedBeds )
						{
							// Is the bed for prisoners and;
							// does it have no owner or;
							//    is the owner this pawn and;
							//    is this bed empty or;
							//       is the other pawn in this bed or;
							//    is bed not a medical bed
							if( ( buildingBed.ForPrisoners )&&
								(   ( buildingBed.owner == null )||
									( buildingBed.owner == prisoner )&&
									( (     ( buildingBed.CurOccupant == null )||
											( buildingBed.CurOccupant == prisoner ) )&&
										( !buildingBed.Medical ) ) ) )
							{
								// Put them in bed
								putToBed = true;
								break;
							}
						}
					}
				}
				// Does the prisoner want a bed but want to be put to bed?
				if( wantsBed || !putToBed )
				{
					// Find a bed for the prisoner and take them there
					Building_Bed bedFor = 
						RestUtility.FindBedFor( prisoner, pawn, true, false, false );
					if( bedFor != null )
						return new Job( 
							JobDefOf.EscortPrisonerToBed, 
							(TargetInfo) ((Thing) prisoner ), 
							(TargetInfo) ((Thing) bedFor ) )
						{
							maxNumToCarry = 1
						};
				}
			}
			// The prisoner has a cell and is in it

			// Is the prisoner downed or;
			// should the prisoner stay in bed,
			// and;
			// is the prisoner not in bed and;
			// can the pawn get to the prisoner
			if( (   ( prisoner.Downed )||
					( prisoner.health.PrisonerShouldStayInBedBecauseSick ) )&&
				(   ( !RestUtility.InBed( prisoner ) )&&
					( ReservationUtility.CanReserve( 
						pawn, 
						(TargetInfo) ((Thing) prisoner ), 
						1 ) ) ) )
			{
				// Find a bed for the prisoner and take them there
				// (if it's reachable)
				Thing bedFor = (Thing) RestUtility.FindBedFor( prisoner, pawn, true, true, false );
				if( ( bedFor != null )&&
					( ReservationUtility.CanReserve( 
						prisoner, 
						(TargetInfo) bedFor, 
						1 ) ) )
					return new Job( 
						JobDefOf.TakeWoundedPrisonerToBed, 
						(TargetInfo) ((Thing) prisoner ), 
						(TargetInfo) bedFor )
					{
						maxNumToCarry = 1
					};
			}
			// The prisoner isn't downed or is in bed

			// Is the prisoner hungry and;
			// are they hungry enough
			if( ( prisoner.guest.getsFood )&&
				( (double) prisoner.needs.food.CurLevel < (double) prisoner.needs.food.ThreshHungry + 0.0199999995529652 ) )
			{
				// Find them some food
				ThingDef foodDef;
				Thing ingestible = CommunityCoreLibrary.FoodUtility.BestFoodSourceFor( pawn, prisoner, true, out foodDef );
				if( ingestible != null )
				{
					// Is the pawn downed or;
					// are they in their bed and;
					// is the bed a medical bed
					if( ( prisoner.Downed )||
						( RestUtility.InBed( prisoner ) )&&
						( RestUtility.CurrentBed( prisoner ).Medical ) )
					{
						// Can the pawn get to the prisoner
						if( ReservationUtility.CanReserve( 
							pawn, 
							(TargetInfo) ((Thing) prisoner ),
							1 ))
							// Take the prisoner as much food as they need
							return new Job(
								JobDefOf.FeedPatient, 
								(TargetInfo) ingestible, 
								(TargetInfo) ((Thing) prisoner ) )
							{
								maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( prisoner, foodDef )
							};
					}
					// Otherwise
					// Is there no food in the room for the prisoner and;
					// can the pawn get to the prisoner
					else if (
						( !CommunityCoreLibrary.WorkGiver_Warden.FoodAvailableInRoomTo( prisoner ) )&&
						( ReservationUtility.CanReserve( 
							pawn, 
							(TargetInfo) ((Thing) prisoner ), 
							1 ) ) )
						// Then take food to the prisoner
						return new Job( 
							JobDefOf.DeliverFood, 
							(TargetInfo) ingestible, 
							(TargetInfo) ((Thing) prisoner ) )
						{
							maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( prisoner, foodDef ),
							targetC = (TargetInfo) RCellFinder.SpotToEatStandingNear( prisoner, ingestible )
						};
				}
			}
			// The prisoner doesn't need food or has been fed

			// Is the prisoner being released and;
			// they are not downed and;
			// they are awake
			if( ( prisoner.guest.interactionMode == PrisonerInteractionMode.Release )&&
				( !prisoner.Downed )&&
				( RestUtility.Awake( prisoner ) ) )
			{
				// Find somewhere to take them outside the colony
				IntVec3 releaseCell;
				if( !RCellFinder.TryFindPrisonerReleaseCell( prisoner, pawn, out releaseCell ) )
					return (Job) null;
				// Release the prisoner at the location
				return new Job(
					JobDefOf.ReleasePrisoner, 
					(TargetInfo) ((Thing) prisoner ), 
					(TargetInfo) releaseCell )
				{
					maxNumToCarry = 1
				};
			}
			// Prisoner is going to stay for a while

			// Are we allowed to talk to the prisoner or;
			// we are attempting to recruit them,
			// and;
			// it's time to chat with them and;
			// we can get to the prisoner and;
			// they are awake
			if( (   ( prisoner.guest.interactionMode == PrisonerInteractionMode.Chat )||
					( prisoner.guest.interactionMode == PrisonerInteractionMode.AttemptRecruit ) )&&
				(   ( prisoner.guest.PrisonerScheduledForInteraction )&&
					( ReservationUtility.CanReserve(
						pawn, 
						(TargetInfo) ((Thing) prisoner ),
						1 ) )&&
					( RestUtility.Awake( prisoner ) ) ) )
				// Then be nice to them, eh?
				return new Job(
					JobDefOf.PrisonerFriendlyChat,
					( (TargetInfo) (Thing) prisoner ) );
			// Prisoner doesn't need to be talked to

			// Should we execute the prisoner and;
			// can we get to them
			if( ( prisoner.guest.interactionMode == PrisonerInteractionMode.Execution )&&
				( ReservationUtility.CanReserve(
					pawn, 
					( (TargetInfo) (Thing) prisoner ),
					1 ) ) )
				// Let me introduce you to my little friend...
				return new Job(
					JobDefOf.PrisonerExecution, 
					( (TargetInfo) (Thing) prisoner ) );

			// Prisoner doesn't need attention
			return (Job) null;
		}

		private static bool FoodAvailableInRoomTo( Pawn prisoner )
		{
			// Does the prisoner already have food
			if( ( prisoner.carryHands.CarriedThing != null )&&
				( CommunityCoreLibrary.FoodUtility.NutritionAvailableFromFor( prisoner.carryHands.CarriedThing, prisoner ) > 0.0 ) )
				return true;
			Room room = RoomQuery.RoomAt( prisoner.Position );
			// Why isn't the prisoner in their cell???
			if (room == null)
				return false;
			
			float totalNutritionNeeded = 0.0f;
			float totalNutritionAvailable = 0.0f;
			// Find some food in the room
			for( int rIndex = 0; rIndex < room.RegionCount; ++rIndex )
			{
				Region region = room.Regions[ rIndex ];
				// Look for an NPD in the region
				List<Thing> regionThings = region.ListerThings.AllThings;
				for( int tIndex = 0; tIndex < regionThings.Count; ++tIndex )
				{
					// Look at this thing,
					// if it's an NPD of any kind
					// they have enough food available to them
					Thing possibleNPD = regionThings[ tIndex ];
					CommunityCoreLibrary.Building_NutrientPasteDispenser cclNPD = possibleNPD as CommunityCoreLibrary.Building_NutrientPasteDispenser;
					RimWorld.Building_NutrientPasteDispenser rwNPD = possibleNPD as RimWorld.Building_NutrientPasteDispenser;
					if( ( cclNPD != null )||
						( rwNPD != null ) )
						return true;
				}
				// Get the food in this region
				List<Thing> regionFood = region.ListerThings.ThingsInGroup( ThingRequestGroup.FoodNotPlant );
				if( regionFood != null )
				{
					// Look at all the food in the region
					for( int index2 = 0; index2 < regionFood.Count; ++index2 )
					{
						// Prisoners don't do drugs
						Thing food = regionFood[ index2 ];
						if( food.def.ingestible.preferability > AIFoodPreferability.NeverForNutrition )
							totalNutritionAvailable += FoodUtility.NutritionAvailableFromFor( food, prisoner );
					}
				}
				// Get the prisoners in this region
				List<Thing> regionPrisoners = region.ListerThings.ThingsInGroup( ThingRequestGroup.Pawn );
				if( regionPrisoners != null)
				{
					// Look at all the prisoners in this region
					for( int index2 = 0; index2 < regionPrisoners.Count; ++index2 )
					{
						// Get this prisoners nutritional needs
						Pawn pawn = regionPrisoners[ index2 ] as Pawn;
						if( ( pawn.IsPrisonerOfColony )&&
							( (double) pawn.needs.food.CurLevel < (double) pawn.needs.food.ThreshHungry + 0.0199999995529652 )&&
							(   ( pawn.carryHands.CarriedThing == null )||
								( !pawn.RaceProps.WillAutomaticallyEat( pawn.carryHands.CarriedThing ) ) ) )
							totalNutritionNeeded += pawn.needs.food.NutritionWanted;
					}
				}
			}
			return (double) totalNutritionAvailable + 0.5 >= (double) totalNutritionNeeded;
		}
	}
}
