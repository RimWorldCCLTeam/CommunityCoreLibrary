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

	public static class FoodUtility
	{
		public static float NutritionAvailableFromFor( Thing t, Pawn p )
		{
			if( t.def.IsNutritionSource && p.RaceProps.WillAutomaticallyEat( t ) )
				return t.def.ingestible.nutrition * (float) t.stackCount;
			if( p.RaceProps.ToolUser )
			{
				// Check if it's a commnuity NPD
				CommunityCoreLibrary.Building_NutrientPasteDispenser cclNPD = t as CommunityCoreLibrary.Building_NutrientPasteDispenser;
				if( ( cclNPD != null )&&( cclNPD.CanDispenseNow ) )
					return 99999f;
				// Maybe a vanilla one?
				RimWorld.Building_NutrientPasteDispenser rwNPD = t as RimWorld.Building_NutrientPasteDispenser;
				if( ( rwNPD != null )&&( rwNPD.CanDispenseNow ) )
					return 99999f;
			}
			return 0.0f;
		}

		public static Thing BestFoodSourceFor( Pawn getter, Pawn eater, bool fullDispensersOnly, out ThingDef foodDef )
		{
			Thing thing = CommunityCoreLibrary.FoodUtility.BestFoodInWorldFor( getter, eater );

			if( ( getter.RaceProps.ToolUser )&&
				( ( thing == null )||( thing.def.ingestible.preferability < AIFoodPreferability.Awful ) ) )
			{
				// Try find community NPD
				foreach( ThingDef t in NPDDefs.AllDefs )
				{
					CommunityCoreLibrary.Building_NutrientPasteDispenser cclNPD = (CommunityCoreLibrary.Building_NutrientPasteDispenser)
						GenClosest.ClosestThingReachable( 
							getter.Position, 
							ThingRequest.ForDef( t ), 
							PathMode.InteractionCell, 
							TraverseParms.For( getter ), 9999f );
					if( cclNPD != null )
					{
						// Found a community NPD
						if(	( cclNPD.HasEnoughFoodInHoppers() )||(
							( !cclNPD.HasEnoughFoodInHoppers() )&&
							( cclNPD.AdjacentReachableHopper( getter ) != null ) ) )
						{
							// Has enough food or doesn't have enough food but has a reachable hopper

							// TODO: Update this to handle meal dispensing from a list of options
							foodDef = ThingDefOf.MealNutrientPaste;
							return ( Thing ) cclNPD;
						}
					}
				}
				// Ok, try to find a vanilla NPD
				RimWorld.Building_NutrientPasteDispenser rwNPD = (RimWorld.Building_NutrientPasteDispenser)
					GenClosest.ClosestThingReachable( 
						getter.Position, 
						ThingRequest.ForDef( ThingDefOf.NutrientPasteDispenser ), 
						PathMode.InteractionCell, 
						TraverseParms.For( getter ), 9999f );
				if( rwNPD != null )
				{
					// Found a vanilla NPD
					if(	( rwNPD.HasEnoughFoodInHoppers() )||(
						( !rwNPD.HasEnoughFoodInHoppers() )&&
						( rwNPD.AdjacentReachableHopper( getter ) != null ) ) )
					{
						// Has enough food or doesn't have enough food but has a reachable hopper
						foodDef = ThingDefOf.MealNutrientPaste;
						return (Thing) rwNPD;
					}
				}
			}
			foodDef = thing == null ? (ThingDef) null : thing.def;
			return thing;
		}

		public static Thing BestFoodInWorldFor( Pawn getter, Pawn eater )
		{
			// Default preference for race minimum
			AIFoodPreferability minPref = eater.def.race.minFoodPreferability;

			// Lower preference for starvation?
			if( ( eater.RaceProps.Humanlike )&&
				( eater.needs.food.CurCategory < HungerCategory.UrgentlyHungry ) )
			{
				minPref = AIFoodPreferability.Awful;
			}

			// What type of food do we want to eat based on preference
			ThingRequest thingRequest = eater.RaceProps.minFoodPreferability != AIFoodPreferability.Plant ? ThingRequest.ForGroup( ThingRequestGroup.FoodNotPlant ) : ThingRequest.ForGroup( ThingRequestGroup.Food );

			Thing foodWanted;
			if( eater.RaceProps.Humanlike )
			{
				// Find food for a human
				foodWanted = CommunityCoreLibrary.FoodUtility.FoodSearchInnerScan( 
					getter.Position, 
					Find.ListerThings.ThingsMatching( thingRequest ), 
					PathMode.ClosestTouch, 
					TraverseParms.For( 
						getter, 
						Danger.Deadly, 
						true ), 
					9999f );
			}
			else
			{
				// Find food for an animal
				foodWanted = GenClosest.ClosestThingReachable( 
					getter.Position, 
					thingRequest, 
					PathMode.ClosestTouch, 
					TraverseParms.For( 
						getter, 
						Danger.Deadly, 
						true ), 
					9999f, 
					searchRegionsMax: 30 );
			}
			return foodWanted;
		}

		public static Thing FoodSearchInnerScan( IntVec3 root, List<Thing> searchSet, PathMode pathMode, TraverseParms traverseParams, float maxDistance = 9999, Predicate<Thing> validator = null )
		{
			// Gotta have something to find :\
			if( searchSet == null )
				return (Thing) null;

			// No food selected and it's on another world
			Thing foodWanted = (Thing) null;
			float foodDistance = float.MinValue;

			// Itterate through list of possible food sources
			for( int index = 0; index < searchSet.Count; ++index )
			{
				Thing thisFood = searchSet[ index ];

				// Distance of pawn to food
				float dist = (float) ( root - thisFood.Position ).LengthManhattan;

				// Is it close enough to count?
				if( (double) dist <= (double) maxDistance )
				{
					// Adjust distance for quality
					float thisDistance = CommunityCoreLibrary.FoodUtility.FoodOptimality( thisFood, dist );
					// Is it closer than the currently selected food, reachable, spawned and valid?
					if( ( thisDistance > foodDistance )&&
						( Reachability.CanReach( 
							root, 
							(TargetInfo) thisFood, 
							pathMode, 
							traverseParams ) )&&
						( thisFood.SpawnedInWorld )&&
						( validator == null )||
						( validator( thisFood ) ) )
					{
						// Then select it
						foodWanted = thisFood;
						foodDistance = thisDistance;
					}
				}
			}
			return foodWanted;
		}

		public static float FoodOptimality( Thing t, float dist )
		{
			float finalDistance = dist;
			switch( t.def.ingestible.preferability )
			{
			case AIFoodPreferability.Plant:
				finalDistance += 500f;
				break;
			case AIFoodPreferability.NeverForNutrition:
				return -9999999f;
			case AIFoodPreferability.DesperateOnly:
				finalDistance += 250f;
				break;
			case AIFoodPreferability.Raw:
				finalDistance += 80f;
				break;
			case AIFoodPreferability.Awful:
				finalDistance += 40f;
				break;
			case AIFoodPreferability.Simple:
				//finalDistance = finalDistance;
				break;
			case AIFoodPreferability.Fine:
				finalDistance -= 25f;
				break;
			case AIFoodPreferability.Lavish:
				finalDistance -= 40f;
				break;
			default:
				Log.Error( "Unknown food taste." );
				break;
			}
			return -finalDistance;
		}

		public static int WillEatStackCountOf( Pawn eater, ThingDef def )
		{
			return (int) Mathf.Max( 1, 
				Mathf.Min( def.ingestible.maxNumToIngestAtOnce, 
					Mathf.RoundToInt( eater.needs.food.NutritionWanted / def.ingestible.nutrition ) ) );
		}

		public static Thing FoodInInventory( Pawn pawn )
		{
			// Pawn has an inventory?
			if( pawn.inventory != null )
			{
				// Scan through it looking for food
				List<Thing> contentsListForReading = pawn.inventory.container.ContentsListForReading;
				for( int index = 0; index < contentsListForReading.Count; ++index )
				{
					// Found some food, use it!
					if( contentsListForReading[ index ].def.IsNutritionSource )
						return contentsListForReading[ index ];
				}
			}
			// Nothing in their inventory
			return (Thing) null;
		}

	}
}

