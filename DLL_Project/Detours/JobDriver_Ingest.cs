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

        internal static Toil ReserveFoodIfWillEatWholeStack( Pawn pawn, TargetIndex index )
        {
            var resFood = new Toil();
            resFood.defaultCompleteMode = ToilCompleteMode.Instant;
            resFood.initAction = new Action( () =>
                {
                    Thing thing = pawn.CurJob.GetTarget( index ).Thing;
                    if( FoodUtility.WillEatStackCountOf( pawn, thing.def ) < thing.stackCount )
                    {
                        return;
                    }
                    if(
                        ( !thing.Spawned )||
                        ( !Find.Reservations.CanReserve( pawn, (TargetInfo) thing, 1 ) )
                    )
                    {
                        pawn.jobs.EndCurrentJob( JobCondition.Incompletable );
                    }
                    else
                    {
                        Find.Reservations.Reserve( pawn, (TargetInfo) thing, 1 );
                    }
                }
            );
            return resFood;
        }

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_Ingest obj )
        {
            var targetThingA = obj.TargetThingA();
            var targetThingB = obj.TargetThingB();

            if( targetThingA is Building )
            {
                yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.InteractionCell ).FailOnDespawnedNullOrForbidden( TargetIndex.A );

                if( targetThingB == null )
                {
                    // Meals
                    if( targetThingA is Building_NutrientPasteDispenser )
                    {
                        yield return Toils_Ingest.TakeMealFromDispenser( TargetIndex.A, obj.pawn );
                    }
                    if( targetThingA is Building_AutomatedFactory )
                    {
                        yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( TargetIndex.A, obj.pawn );
                    }
                }
                else
                {
                    // Alcohol
                    if( targetThingB is Building_AutomatedFactory )
                    {
                        yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( TargetIndex.B, obj.pawn );
                    }
                }
                yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn ).FailOnDestroyedNullOrForbidden( TargetIndex.A );
                yield return Toils_Ingest.FindAdjacentEatSurface( TargetIndex.A, TargetIndex.B );
            }
            else
            {
                var dropIfNeeded = new Toil();
                dropIfNeeded.initAction = new Action( () =>
                    {
                        Pawn pawn = obj.pawn;
                        Thing resultingThing = pawn.CurJob.GetTarget( TargetIndex.A ).Thing;
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
                            pawn.CurJob.SetTarget( TargetIndex.A, (TargetInfo) resultingThing );
                        }
                    }
                );

                yield return dropIfNeeded;
                yield return ReserveFoodIfWillEatWholeStack( obj.pawn, TargetIndex.A );

                if( obj.pawn.RaceProps.ToolUser )
                {
                    yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.ClosestTouch ).FailOnDespawnedNullOrForbidden( TargetIndex.A );
                    yield return Toils_Ingest.PickupIngestible( TargetIndex.A, obj.pawn );
                    yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn ).FailOnDestroyedNullOrForbidden( TargetIndex.A );
                    yield return Toils_Ingest.FindAdjacentEatSurface( TargetIndex.A, TargetIndex.B );
                }
                else
                {
                    yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.Touch );
                }
            }

            if( obj.pawn.Faction != null )
            {
                yield return ReserveFoodIfWillEatWholeStack( obj.pawn, TargetIndex.A );
            }

            var durationMultiplier = 1f / obj.pawn.GetStatValue( StatDefOf.EatingSpeed, true );
            yield return Toils_Ingest.ChewIngestible( obj.pawn, durationMultiplier, TargetIndex.B ).FailOnDespawnedOrNull( TargetIndex.A );
            yield return Toils_Ingest.FinalizeIngest( obj.pawn, TargetIndex.A );
        }

    }

}
