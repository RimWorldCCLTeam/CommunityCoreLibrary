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

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_FoodDeliver obj )
        {
            yield return Toils_Reserve.Reserve( TargetIndex.B, 1 );

            var targetThingA = obj.TargetThingA();

            if( targetThingA is Building )
            {
                yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.InteractionCell );

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
                var deliveree = (Pawn) obj.pawn.CurJob.targetB.Thing;
                yield return Toils_Reserve.Reserve( TargetIndex.A, 1 );
                yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.ClosestTouch );
                yield return Toils_Ingest.PickupIngestible( TargetIndex.A, deliveree );
            }

            var pathToTarget = new Toil();
            pathToTarget.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            pathToTarget.initAction = new Action( () =>
                {
                    Pawn pawn = obj.pawn;
                    Job job = pawn.jobs.curJob;
                    pawn.pather.StartPath( job.targetC, PathEndMode.OnCell );
                }
            );
            pathToTarget.FailOnDestroyedNullOrForbidden( TargetIndex.B );
            pathToTarget.AddFailCondition( () =>
            {
                Pawn pawn = (Pawn) obj.pawn.jobs.curJob.targetB.Thing;
                return
                    ( pawn.Downed )||
                    ( !pawn.IsPrisonerOfColony )||
                    ( !pawn.guest.ShouldBeBroughtFood );
            } );
            yield return pathToTarget;

            var dropFoodAtTarget = new Toil();
            dropFoodAtTarget.initAction = new Action( () =>
                {
                    Thing resultingThing;
                    obj.pawn.carrier.TryDropCarriedThing( obj.pawn.jobs.curJob.targetC.Cell, ThingPlaceMode.Direct, out resultingThing );
                }
            );
            dropFoodAtTarget.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropFoodAtTarget;
        }

    }

}
