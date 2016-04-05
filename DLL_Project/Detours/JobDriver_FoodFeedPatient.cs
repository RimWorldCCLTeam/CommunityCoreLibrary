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

    internal static class _JobDriver_FoodFeedPatient
    {
        // TODO: see other todos
        /*internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_FoodFeedPatient obj )
        {
            Pawn deliveree = (Pawn) obj.pawn.CurJob.targetB.Thing;
            obj.FailOnDespawnedNullOrForbidden( TargetIndex.B );
            obj.FailOn( ( Func<bool> )( () =>
                {
                    return
                        ( deliveree.GetPosture() == PawnPosture.Standing )||
                        ( deliveree.HostFaction != null )&&
                        ( !deliveree.guest.ShouldBeBroughtFood );
                }
            ) );

            yield return Toils_Reserve.Reserve( TargetIndex.B, 1 );

            var targetThingA = obj.TargetThingA();

            if( targetThingA is Building )
            {
                yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.InteractionCell ).FailOnForbidden( TargetIndex.A );
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
                yield return Toils_Reserve.Reserve( TargetIndex.A, 1 );
                yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.ClosestTouch ).FailOnForbidden( TargetIndex.A );
                yield return Toils_Ingest.PickupIngestible( TargetIndex.A, deliveree );
            }

            yield return Toils_Goto.GotoThing( TargetIndex.B, PathEndMode.Touch );
            yield return Toils_Ingest.ChewIngestible( deliveree, 1.5f, TargetIndex.A );
            yield return Toils_Ingest.FinalizeIngest( deliveree, TargetIndex.A );
        }*/

    }

}
