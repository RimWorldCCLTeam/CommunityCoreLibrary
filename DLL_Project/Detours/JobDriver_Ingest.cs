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

        internal const TargetIndex FoodInd = TargetIndex.A;
        internal const TargetIndex AlcoholInd = TargetIndex.C;
        internal const TargetIndex DispenserInd = FoodInd;
        internal const TargetIndex TableCellInd = TargetIndex.B;

        #region Helper Methods

        internal static Thing Dispenser( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( DispenserInd ).Thing;
        }

        internal static Thing Food( this JobDriver_Ingest obj )
        {
            return obj.pawn.CurJob.GetTarget( FoodInd ).Thing;
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
            //var targetThingA = obj.TargetThingA();
            //var targetThingB = obj.TargetThingB();

            if( obj.Dispenser() is Building )
            {
                yield return Toils_Goto.GotoThing( DispenserInd, PathEndMode.InteractionCell ).FailOnDespawnedNullOrForbidden( DispenserInd );

                if( obj.Alcohol() == null )
                {
                    // Meals
                    if( obj.Dispenser() is Building_NutrientPasteDispenser )
                    {
                        yield return Toils_Ingest.TakeMealFromDispenser( FoodInd, obj.pawn );
                    }
                    if( obj.Dispenser() is Building_AutomatedFactory )
                    {
                        yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( FoodInd, obj.pawn );
                    }
                }
                else
                {
                    // Alcohol
                    if( obj.Dispenser() is Building_AutomatedFactory )
                    {
                        yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( FoodInd, obj.pawn );
                    }
                }
                yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn ).FailOnDestroyedNullOrForbidden( FoodInd );
                yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, FoodInd );
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
                            pawn.CurJob.SetTarget( FoodInd, (TargetInfo) resultingThing );
                        }
                    } );

                    yield return dropIfNeeded;
                }
                yield return obj.ReserveFoodIfWillEatWholeStack();
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch ).FailOnDespawnedNullOrForbidden( FoodInd );
                yield return Toils_Ingest.PickupIngestible( FoodInd, obj.pawn );
                yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn ).FailOnDestroyedNullOrForbidden( FoodInd );
                yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, FoodInd );
            }
            else // Non-Tool User
            {
                yield return obj.ReserveFoodIfWillEatWholeStack();
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch ).FailOnDespawnedNullOrForbidden( FoodInd );
            }

            var durationMultiplier = 1f / obj.pawn.GetStatValue( StatDefOf.EatingSpeed, true );
            var chew = Toils_Ingest.ChewIngestible( obj.pawn, durationMultiplier, FoodInd, TableCellInd ).FailOn( () =>
            {
                if( !obj.Food().Spawned )
                {
                    return ( obj.pawn.carrier == null ? 0 : ( obj.pawn.carrier.CarriedThing == obj.Food() ? 1 : 0 ) ) == 0;
                }
                return false;
            } );
            yield return chew;
            yield return Toils_Ingest.FinalizeIngest( obj.pawn, FoodInd );
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
