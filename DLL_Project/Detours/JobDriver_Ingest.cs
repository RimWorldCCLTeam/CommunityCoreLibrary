using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _JobDriver_Ingest
    {

        internal const TargetIndex FoodOrDispenserInd = TargetIndex.A;
        internal const TargetIndex TableCellInd = TargetIndex.B;
        internal const TargetIndex AlcoholInd = TargetIndex.C;

        #region Helper Methods

        internal static bool IsDispenser( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( FoodOrDispenserInd ).Thing is Building;
        }

        internal static bool IsMeal( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( FoodOrDispenserInd ).Thing is Meal;
        }

        internal static Thing Dispenser( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( FoodOrDispenserInd ).Thing;
        }

        internal static Thing Food( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( FoodOrDispenserInd ).Thing;
        }

        internal static Thing Alcohol( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( AlcoholInd ).Thing;
        }

        internal static Thing Table( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( TableCellInd ).Thing;
        }

        internal static Toil ReserveFoodIfWillEatWholeStack( this JobDriver_Ingest obj )
        {
            var resFood = new Toil();
            resFood.defaultCompleteMode = ToilCompleteMode.Instant;
            resFood.initAction = new Action( () =>
            {
                Thing thing = obj.Food();
                if( FoodUtility.WillEatStackCountOf( obj.pawn, thing.def ) < thing.stackCount )
                {
                    return;
                }
                if(
                    ( !thing.Spawned )||
                    ( !Find.Reservations.CanReserve( obj.pawn, (TargetInfo) thing, 1 ) )
                )
                {
                    obj.pawn.jobs.EndCurrentJob( JobCondition.Incompletable );
                }
                else
                {
                    Find.Reservations.Reserve( obj.pawn, (TargetInfo) thing, 1 );
                }
            } );
            return resFood;
        }

        #endregion

        #region Detoured Methods

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_Ingest obj )
        {
            if( obj.IsDispenser() )
            {
                yield return Toils_Goto.GotoThing( FoodOrDispenserInd, PathEndMode.InteractionCell ).FailOnDespawnedNullOrForbidden( FoodOrDispenserInd );

                if( obj.Alcohol() == null )
                {
                    // Meals
                    if( obj.Dispenser() is Building_NutrientPasteDispenser )
                    {
                        yield return Toils_Ingest.TakeMealFromDispenser( FoodOrDispenserInd, obj.pawn );
                    }
                    if( obj.Dispenser() is Building_AutomatedFactory )
                    {
                        yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( FoodOrDispenserInd, obj.pawn );
                    }
                }
                else
                {
                    // Alcohol
                    if( obj.Dispenser() is Building_AutomatedFactory )
                    {
                        yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( FoodOrDispenserInd, obj.pawn );
                    }
                }
                yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn ).FailOnDestroyedNullOrForbidden( FoodOrDispenserInd );
                yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, FoodOrDispenserInd );
            }
            else if( obj.pawn.RaceProps.ToolUser )
            {
                if( obj.pawn.CurJob.eatFromInventory )
                {
                    var dropIfNeeded = new Toil();
                    dropIfNeeded.initAction = new Action( () =>
                    {
                        Pawn pawn = obj.pawn;
                        Thing resultingThing = obj.Food();
                        Thing thing = resultingThing;
                        if(
                            ( pawn.inventory == null )||
                            ( !pawn.inventory.container.Contains( resultingThing ) )
                        )
                        {
                            return;
                        }
                        int count = Mathf.Min( resultingThing.stackCount, pawn.CurJob.maxNumToCarry );
                        if( !pawn.inventory.container.TryDrop( resultingThing, pawn.Position, ThingPlaceMode.Near, count, out resultingThing ) )
                        {
                            Verse.Log.Error( pawn + " could not drop their food to eat it." );
                            obj.EndJobWith( JobCondition.Errored );
                        }
                        else
                        {
                            if( resultingThing == thing )
                            {
                                return;
                            }
                            pawn.CurJob.SetTarget( FoodOrDispenserInd, (TargetInfo) resultingThing );
                        }
                    } );

                    yield return dropIfNeeded;
                }
                yield return obj.ReserveFoodIfWillEatWholeStack();
                yield return Toils_Goto.GotoThing( FoodOrDispenserInd, PathEndMode.ClosestTouch ).FailOnDespawnedNullOrForbidden( FoodOrDispenserInd );
                yield return Toils_Ingest.PickupIngestible( FoodOrDispenserInd, obj.pawn );
                yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn ).FailOnDestroyedNullOrForbidden( FoodOrDispenserInd );
                yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, FoodOrDispenserInd );
            }
            else // Non-Tool User
            {
                yield return obj.ReserveFoodIfWillEatWholeStack();
                yield return Toils_Goto.GotoThing( FoodOrDispenserInd, PathEndMode.ClosestTouch ).FailOnDespawnedNullOrForbidden( FoodOrDispenserInd );
            }

            var durationMultiplier = 1f / obj.pawn.GetStatValue( StatDefOf.EatingSpeed, true );
            var chew = Toils_Ingest.ChewIngestible( obj.pawn, durationMultiplier, FoodOrDispenserInd, TableCellInd ).FailOn( () =>
            {
                if( !obj.Food().Spawned )
                {
                    return ( obj.pawn.carrier == null ? 0 : ( obj.pawn.carrier.CarriedThing == obj.Food() ? 1 : 0 ) ) == 0;
                }
                return false;
            } );
            yield return chew;
            yield return Toils_Ingest.FinalizeIngest( obj.pawn, FoodOrDispenserInd );
            yield return Toils_Jump.JumpIf( chew, () =>
            {
                if( obj.Food() is Corpse )
                {
                    return (double) obj.pawn.needs.food.CurLevelPercentage < JobDriver_Ingest.EatCorpseBodyPartsUntilFoodLevelPct;
                }
                return false;
            } );
        }

        #endregion

    }

}
