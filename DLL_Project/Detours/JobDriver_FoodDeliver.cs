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

    internal static class _JobDriver_FoodDeliver
    {

        internal const TargetIndex FoodInd = TargetIndex.A;
        internal const TargetIndex DelivereeInd = TargetIndex.B;
        internal const TargetIndex DeliverToInd = TargetIndex.C;

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_FoodDeliver obj )
        {
            var foodThing = obj.TargetThing( FoodInd );
            var deliveree = (Pawn) obj.TargetThing( DelivereeInd );
            var dropCell = obj.TargetCell( DeliverToInd );

            yield return Toils_Reserve.Reserve( DelivereeInd, 1 );

            if( foodThing is Building )
            {
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.InteractionCell ).FailOnForbidden( FoodInd );

                if( foodThing is Building_NutrientPasteDispenser )
                {
                    yield return Toils_Ingest.TakeMealFromDispenser( FoodInd, obj.pawn );
                }
                else if( foodThing is Building_AutomatedFactory )
                {
                    yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( FoodInd, obj.pawn );
                }
                else // Unknown building
                {
                    throw new Exception( "Food target for JobDriver_FoodDeliver is a building but not Building_NutrientPasteDispenser or Building_AutomatedFactory!" );
                }
            }
            else if(
                ( obj.pawn.inventory != null )&&
                ( obj.pawn.inventory.Contains( foodThing ) )
            )
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier( obj.pawn, FoodInd );
            }
            else
            {
                yield return Toils_Reserve.Reserve( FoodInd, 1 );
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch );
                yield return Toils_Ingest.PickupIngestible( FoodInd, deliveree );
            }

            var pathToTarget = new Toil();
            pathToTarget.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            pathToTarget.initAction = new Action( () =>
                {
                    var pawn = obj.pawn;
                    var job = pawn.jobs.curJob;
                    pawn.pather.StartPath( job.targetC, PathEndMode.OnCell );
                }
            );
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
                    obj.pawn.carrier.TryDropCarriedThing( dropCell, ThingPlaceMode.Direct, out resultingThing );
                }
            );
            dropFoodAtTarget.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropFoodAtTarget;
        }

    }

}
