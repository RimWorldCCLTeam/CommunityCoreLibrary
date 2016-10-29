// A14 - Signficant changes - needs a thorough check
// - Fluffy.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using CommunityCoreLibrary;

namespace CommunityCoreLibrary.Detour
{

    internal class _JobGiver_GetFood : JobGiver_GetFood
    {

        [DetourClassMethod( typeof( JobGiver_GetFood ), "TryGiveJob" )]
        protected override Job TryGiveJob( Pawn pawn )
        {
            bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
            Thing foodSource;
            ThingDef foodDef;

            //CCL_Log.Message( "JobGiver_GetFood for " + pawn.LabelShort );

            // Find an appropriate food source for the pawn
            if( !FoodUtility.TryFindBestFoodSourceFor(
                pawn,
                pawn,
                desperate,
                out foodSource,
                out foodDef,
                true,
                true,
                false,
                true )
            )
            {
                return null;
            }

            //CCL_Log.Message( string.Format( "Found {0} ({1}) for {2}", foodSource.ThingID, foodDef.defName, pawn.LabelShort ) );

            // Predator-Prey
            var prey = foodSource as Pawn;
            if( prey != null )
            {
                var hunterJob = new Job( JobDefOf.PredatorHunt, prey );
                hunterJob.killIncappedTarget = true;
                return hunterJob;
            }

            // Nutrient Paste Dispensers and Food Synthesizers (Building_AutomatedFactory)
            if( foodSource is Building )
            {
                var hopperNeedsFilling = false;
                var hopper = (Building) null;
                if( foodSource is Building_NutrientPasteDispenser )
                {
                    var NPD = foodSource as Building_NutrientPasteDispenser;
                    if( !NPD.HasEnoughFeedstockInHoppers() )
                    {
                        //CCL_Log.Message( string.Format( "Hopper for {0} needs filling", foodSource.ThingID ) );
                        hopperNeedsFilling = true;
                        hopper = NPD.AdjacentReachableHopper( pawn );
                    }
                }
                if( foodSource is Building_AutomatedFactory )
                {
                    //Fixed the exepction from passing a Factory into the WillIngestStackCountOf function.
                    //However, truely fixing this will require detouring FoodUtilities to get proper functionality.
                    var FS = foodSource as Building_AutomatedFactory;
                    foodDef = FS.BestProduct(FoodSynthesis.IsMeal, FoodSynthesis.SortMeal);
                    if( !FS.HasEnoughResourcesInHoppersFor(foodDef) )
                    {
                        //CCL_Log.Message( string.Format( "Hopper for {0} needs filling", foodSource.ThingID ) );
                        hopperNeedsFilling = true;
                        hopper = FS.AdjacentReachableHopper( pawn );
                    }
                }
                if( hopperNeedsFilling )
                {
                    if( hopper != null )
                    {
                        //CCL_Log.Message( string.Format( "Found hopper {0} for {1} that needs filling", hopper.ThingID, foodSource.ThingID ) );
                        return HopperFillFoodJob( pawn, hopper, foodSource );
                    }
                    else
                    {   // Find an alternate source that isn't an NPD or FS
                        //CCL_Log.Message( "Searching for non-machine food for " + pawn.LabelShort );
                        foodSource = FoodUtility.BestFoodSourceOnMap(
                            pawn,
                            pawn,
                            desperate,
                            FoodPreferability.MealLavish,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false );
                        if( foodSource == null )
                        {
                            return null;
                        }
                        foodDef = foodSource.def;
                    }
                }
            }

            //CCL_Log.Message( string.Format( "Giving JobDriver_Ingest to {0} using {1}", pawn.LabelShort, foodSource.ThingID ) );
            // Ingest job for found food source
            var ingestJob = new Job( JobDefOf.Ingest, foodSource );
            ingestJob.maxNumToCarry = FoodUtility.WillIngestStackCountOf( pawn, foodDef );
            return ingestJob;
        }


        internal static Job HopperFillFoodJob( Pawn pawn, Building hopper, Thing parent )
        {
            var hopperSgp = hopper as ISlotGroupParent;
            if(
                ( !pawn.CanReserveAndReach(
                    hopper.Position,
                    PathEndMode.Touch,
                    pawn.NormalMaxDanger(),
                    1 )
            )
            )
            {
                return null;
            }
            ThingDef resourceDef = null;
            var firstItem = hopper.Position.GetFirstItem();
            if( firstItem != null )
            {
                if(
                    (
                        ( parent is Building_NutrientPasteDispenser )&&
                        ( Building_NutrientPasteDispenser.IsAcceptableFeedstock( firstItem.def ) )
                    )||
                    (
                        ( parent is Building_AutomatedFactory )&&
                        ( ( (Building_AutomatedFactory)parent ).CompHopperUser.ResourceSettings.AllowedToAccept( firstItem ) )
                    )
                )
                {
                    resourceDef = firstItem.def;
                }
                else
                {
                    if( firstItem.IsForbidden( pawn ) )
                    {
                        return ( Job )null;
                    }
                    return HaulAIUtility.HaulAsideJobFor( pawn, firstItem );
                }
            }
            List<Thing> list =
                resourceDef != null
                ? Find.Map.listerThings.ThingsOfDef( resourceDef )
                : Find.Map.listerThings.ThingsInGroup( ThingRequestGroup.FoodSourceNotPlantOrTree );
            for( int index = 0; index < list.Count; ++index )
            {
                Thing t = list[ index ];
                if(
                    ( t.def.IsNutritionGivingIngestible )&&
                    (
                        ( t.def.ingestible.preferability == FoodPreferability.RawBad )||
                        ( t.def.ingestible.preferability == FoodPreferability.RawTasty )
                    )&&
                    ( HaulAIUtility.PawnCanAutomaticallyHaul( pawn, t ) )&&
                    (
                        ( Find.SlotGroupManager.SlotGroupAt( hopper.Position ).Settings.AllowedToAccept( t ) )&&
                        ( HaulAIUtility.StoragePriorityAtFor( t.Position, t ) < hopperSgp.GetSlotGroup().Settings.Priority )
                    )
                )
                {
                    Job job = HaulAIUtility.HaulMaxNumToCellJob( pawn, t, hopper.Position, true );
                    if( job != null )
                    {
                        return job;
                    }
                }
            }
            return null;
        }

    }

}
