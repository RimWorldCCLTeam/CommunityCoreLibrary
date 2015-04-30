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

	public class WorkGiver_FeedIncapped : WorkGiver
	{
		
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
				return PathMode.Touch;
			}
		}

		public override bool HasJobOnThing( Pawn pawn, Thing t )
		{
			// Is the thing not a pawn or;
			// the downed is not of this colony and;
			// they are not a guest,
			// or;
			// they don't eat food or;
			// they are a quest and;
			// they get food,
			// or;
			// they are unreachable
			Pawn downed = t as Pawn;
			if( ( downed == null )||
				( downed.Faction != Faction.OfColony )&&
				( downed.guest == null )||
				(   ( !downed.RaceProps.EatsFood )||
					( downed.guest != null )&&
				    ( downed.guest.getsFood ) )||
				( !ReservationUtility.CanReserve( 
					pawn, 
					( (TargetInfo) (Thing) downed ), 
					1 ) ) )
				return false;
			// A downed pawn we care about needs food and is reachable

			// Is the downed pawn hungry enough to want food?
			if( (double) downed.needs.food.CurLevel > (double) downed.needs.food.ThreshHungry + 0.0199999995529652 )
				return false;

			// Are they downed or in bed?
			if( !downed.Downed )
			{
				// Not in bed or not in a medical bed, can't help ya
				if( ( !RestUtility.InBed( downed ) )||
					( !RestUtility.CurrentBed( downed ).Medical ) )
					return false;
			}
			// They are downed but not in bed, still can't help ya
			else if ( !RestUtility.InBed( downed ) )
				return false;

			// Return whether we can get some food for them
			ThingDef foodDef;
			return CommunityCoreLibrary.FoodUtility.BestFoodSourceFor( 
				pawn, 
				downed, 
				true, 
				out foodDef
			) != null;
		}

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			// Find the best food source for the pawn
			Pawn eater = (Pawn) t;
			ThingDef foodDef;
			Thing thing = CommunityCoreLibrary.FoodUtility.BestFoodSourceFor( 
				pawn, 
				eater, 
				true, 
				out foodDef );
			// And deliver it to them
			return new Job( JobDefOf.FeedPatient )
			{
				targetA = new TargetInfo(thing),
				targetB = new TargetInfo((Thing) eater),
				maxNumToCarry = CommunityCoreLibrary.FoodUtility.WillEatStackCountOf( eater, foodDef )
			};
		}
	}
}
