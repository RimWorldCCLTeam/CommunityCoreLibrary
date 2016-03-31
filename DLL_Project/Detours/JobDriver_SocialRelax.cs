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

    internal static class _JobDriver_SocialRelax
    {

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_SocialRelax obj )
        {
            obj.EndOnDespawned( TargetIndex.A, JobCondition.Incompletable );

            var targetThingB = obj.TargetThingB();
            var targetThingC = obj.TargetThingC();

            if( targetThingB != null )
            {
                obj.EndOnDespawned( TargetIndex.B, JobCondition.Incompletable );
            }
            yield return Toils_Reserve.Reserve( TargetIndex.B, 1 );

            if( targetThingC != null )
            {
                if( targetThingC is Building_AutomatedFactory )
                {
                    yield return Toils_Goto.GotoThing( TargetIndex.C, PathEndMode.InteractionCell );
                    yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( TargetIndex.C, obj.pawn );
                }
                else
                {
                    obj.FailOnDestroyedOrForbidden( TargetIndex.C );
                    yield return Toils_Reserve.Reserve( TargetIndex.C, 1 );
                    yield return Toils_Goto.GotoThing( TargetIndex.C, PathEndMode.OnCell );
                    yield return Toils_Haul.StartCarryThing( TargetIndex.C );
                }
            }

            yield return Toils_Goto.GotoCell( TargetIndex.B, PathEndMode.OnCell );

            var relax = new Toil();
            relax.defaultCompleteMode = ToilCompleteMode.Delay;
            relax.defaultDuration = obj.pawn.CurJob.def.joyDuration;
            relax.tickAction = new Action( () =>
                {
                    obj.pawn.drawer.rotator.FaceCell( obj.TargetThingA().OccupiedRect().ClosestCellTo( obj.pawn.Position ) );
                    obj.pawn.GainComfortFromCellIfPossible();
                    obj.pawn.talker.BeChatty();
                    JoyUtility.JoyTickCheckEnd( obj.pawn, false, 1f );
                }
            );
            relax.AddFinishAction( () =>
                JoyUtility.TryGainRecRoomThought( obj.pawn )
            );
            yield return relax;

            if( targetThingC != null )
            {
                yield return Toils_Ingest.FinalizeIngest( obj.pawn, TargetIndex.C );
            }
        }

    }

}
