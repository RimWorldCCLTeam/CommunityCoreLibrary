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

namespace CommunityCoreLibrary.Detour
{

    internal class _JobDriver_FoodDeliver : JobDriver_FoodDeliver
    {

        internal const TargetIndex          FoodInd = TargetIndex.A;
        internal const TargetIndex          DelivereeInd = TargetIndex.B;
        internal const TargetIndex          DeliverToInd = TargetIndex.C;

        // TODO: check for changes in A16; I can't figure out how to decompile an iterator (NuOfBelthasar)
        [DetourMember]
        internal IEnumerable<Toil>          _MakeNewToils()
        {
            var foodThing = this.TargetThing( FoodInd );
            var deliveree = (Pawn) this.TargetThing( DelivereeInd );
            var dropCell = this.TargetCell( DeliverToInd );

            yield return Toils_Reserve.Reserve( DelivereeInd, 1 );

            if(
                ( this.pawn.inventory != null )&&
                ( this.pawn.inventory.Contains( foodThing ) )
            )
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier( this.pawn, FoodInd );
            }
            else if( foodThing is Building )
            {
                yield return Toils_Reserve.Reserve( FoodInd, 1 );
                this.AddFinishAction( () =>
                {
                    if( pawn.Map.reservationManager.ReservedBy( foodThing, pawn ) )
                    {   // Release reservation if aborted early
                        pawn.Map.reservationManager.Release( foodThing, pawn );
                    }
                } );
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.InteractionCell ).FailOnForbidden( FoodInd );

                if( foodThing is Building_NutrientPasteDispenser )
                {
                    yield return Toils_Ingest.TakeMealFromDispenser( FoodInd, this.pawn );
                }
                else if( foodThing is Building_AutomatedFactory )
                {
                    // CALLER MUST USE Building_AutomatedFactory.ReserveForUseBy() BEFORE USING THIS METHOD!
                    //yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( FoodInd, this.pawn );
                    yield return Toils_FoodSynthesizer.TakeFromSynthesier( FoodInd, this.pawn );
                }
                else // Unknown building
                {
                    throw new Exception( "Food target for JobDriver_FoodDeliver is a building but not Building_NutrientPasteDispenser or Building_AutomatedFactory!" );
                }
            }
            else
            {
                yield return Toils_Reserve.Reserve( FoodInd, 1 );
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch ).FailOnForbidden( FoodInd );
                this.AddFinishAction( () =>
                {
                    if( pawn.Map.reservationManager.ReservedBy( foodThing, pawn ) )
                    {   // Release reservation if aborted early
                        pawn.Map.reservationManager.Release( foodThing, pawn );
                    }
                } );
                yield return Toils_Ingest.PickupIngestible( FoodInd, deliveree );
            }

            var pathToTarget = new Toil();
            pathToTarget.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            pathToTarget.initAction = new Action( () =>
            {
                var pawn = this.pawn;
                var job = pawn.jobs.curJob;
                pawn.pather.StartPath( this.TargetC, PathEndMode.OnCell );
            } );
            pathToTarget.FailOnDestroyedNullOrForbidden( DelivereeInd );
            pathToTarget.AddFailCondition( () =>
            {
                return
                    ( deliveree.Downed )||
                    ( !deliveree.IsPrisonerOfColony )||
                    ( !deliveree.guest.ShouldBeBroughtFood );
            } );
            yield return pathToTarget;

            var dropFoodAtTarget = new Toil();
            dropFoodAtTarget.initAction = new Action( () =>
            {
                Thing resultingThing;
                this.pawn.carryTracker.TryDropCarriedThing( dropCell, ThingPlaceMode.Direct, out resultingThing );
            } );
            dropFoodAtTarget.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropFoodAtTarget;
            yield break;
        }

    }

}
