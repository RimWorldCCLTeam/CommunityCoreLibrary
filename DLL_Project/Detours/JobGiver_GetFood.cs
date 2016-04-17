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

    internal static class _JobGiver_GetFood
    {
        
        internal static Job IngestJob( this JobGiver_GetFood obj, Pawn pawn, Thing food )
        {
            var _IngestJob = typeof( JobGiver_GetFood ).GetMethod( "IngestJob", BindingFlags.Instance | BindingFlags.NonPublic );
            return (Job)_IngestJob.Invoke( obj, new System.Object[] { pawn, food } );
        }

        internal static Job _TryGiveTerminalJob( this JobGiver_GetFood obj, Pawn pawn )
        {
            Thing foodInInventory = null;
            if( pawn.RaceProps.ToolUser )
            {
                foodInInventory = FoodUtility.FoodInInventory( pawn, (Pawn) null, FoodPreferability.Awful, FoodPreferability.Lavish, 0.0f );
                if( foodInInventory != null )
                {
                    if( pawn.Faction != Faction.OfColony )
                    {
                        return obj.IngestJob( pawn, foodInInventory );
                    }
                    CompRottable comp = foodInInventory.TryGetComp<CompRottable>();
                    if(
                        ( comp != null )&&
                        ( comp.TicksUntilRotAtCurrentTemp < 30000 )
                    )
                    {
                        return obj.IngestJob( pawn, foodInInventory );
                    }
                }
            }
            ThingDef foodDef;
            Thing bestFoodSource = FoodUtility.BestFoodSourceFor( pawn, pawn, false, out foodDef );
            if(
                ( foodInInventory != null )&&
                (
                    ( bestFoodSource == null )||
                    ( !pawn.Position.InHorDistOf( bestFoodSource.Position, 50f ) ) )
            )
            {
                return obj.IngestJob( pawn, foodInInventory );
            }
            if( bestFoodSource == null )
            {
                return (Job) null;
            }

            if( bestFoodSource is Building )
            {
                Building hopper = null;
                bool needsFilling = false;
                if(
                    ( bestFoodSource is Building_NutrientPasteDispenser )&&
                    ( !( (Building_NutrientPasteDispenser)bestFoodSource).HasEnoughFeedstockInHoppers() )
                )
                {
                    hopper = ( (Building_NutrientPasteDispenser)bestFoodSource).AdjacentReachableHopper( pawn );
                    needsFilling = true;
                }
                else if(
                    ( bestFoodSource is Building_AutomatedFactory )&&
                    ( ( (Building_AutomatedFactory)bestFoodSource ).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal ) == null )
                )
                {
                    hopper = ( (Building_AutomatedFactory)bestFoodSource ).AdjacentReachableHopper( pawn );
                    needsFilling = true;
                }
                if( needsFilling )
                {
                    if( hopper == null )
                    {
                        bestFoodSource = FoodUtility.BestFoodSpawnedFor( pawn, pawn, true, FoodPreferability.Lavish, true );
                        if( bestFoodSource == null )
                        {
                            return (Job) null;
                        }
                    }
                    else
                    {
                        ISlotGroupParent hopperSgp = hopper as ISlotGroupParent;
                        Job job = HopperFillFoodJob( pawn, hopperSgp, bestFoodSource );
                        if( job != null )
                        {
                            return job;
                        }
                        bestFoodSource = FoodUtility.BestFoodSpawnedFor( pawn, pawn, true, FoodPreferability.Lavish, true );
                        if( bestFoodSource == null )
                        {
                            return (Job) null;
                        }
                        foodDef = bestFoodSource.def;
                    }
                }
            }
            Pawn prey = bestFoodSource as Pawn;
            if( prey != null )
            {
                Job predatorHunt = new Job( JobDefOf.PredatorHunt, prey );
                predatorHunt.killIncappedTarget = true;
                return predatorHunt;
            }
            Job ingestJob = new Job( JobDefOf.Ingest, bestFoodSource );
            ingestJob.maxNumToCarry = FoodUtility.WillEatStackCountOf( pawn, foodDef );
            return ingestJob;
        }

        internal static Job HopperFillFoodJob( Pawn pawn, ISlotGroupParent hopperSgp, Thing parent )
        {
            Building building = hopperSgp as Building;
            if(
                ( !pawn.CanReserveAndReach(
                    ( TargetInfo )building.Position,
                    PathEndMode.Touch,
                    pawn.NormalMaxDanger(),
                    1 )
                )
            )
            {
                return (Job) null;
            }
            ThingDef resourceDef = (ThingDef) null;
            Thing firstItem = building.Position.GetFirstItem();
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
                : Find.Map.listerThings.ThingsInGroup( ThingRequestGroup.FoodNotPlantOrTree );
            for( int index = 0; index < list.Count; ++index )
            {
                Thing t = list[ index ];
                if(
                    ( t.def.ingestible.preferability == FoodPreferability.Raw )&&
                    ( HaulAIUtility.PawnCanAutomaticallyHaul( pawn, t ) )&&
                    (
                        ( Find.SlotGroupManager.SlotGroupAt( building.Position ).Settings.AllowedToAccept( t ) )&&
                        ( HaulAIUtility.StoragePriorityAtFor( t.Position, t ) < hopperSgp.GetSlotGroup().Settings.Priority )
                    )
                )
                {
                    Job job = HaulAIUtility.HaulMaxNumToCellJob( pawn, t, building.Position, true );
                    if( job != null )
                    {
                        return job;
                    }
                }
            }
            return (Job) null;
        }

    }

}
