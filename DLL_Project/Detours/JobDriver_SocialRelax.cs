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
        internal static FieldInfo _GetPawnDrawTracker;
        internal static Pawn_DrawTracker GetPawnDrawTracker(this Pawn pawn)
        {
            if (_GetPawnDrawTracker == null)
            {
                _GetPawnDrawTracker = typeof(Pawn).GetField("drawer", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_GetPawnDrawTracker == null)
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Unable to get 'drawer' in class 'Pawn'",
                        "CommunityCoreLibrary.Detour.JobDriver_SocialRelax");
                    return null;
                }
            }
            return (Pawn_DrawTracker)_GetPawnDrawTracker.GetValue(pawn);
        }

        internal static IEnumerable<Toil> _MakeNewToils(this JobDriver_SocialRelax obj)
        {
            obj.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable);

            var targetThingB = obj.TargetThingB();
            var targetThingC = obj.TargetThingC();

            if (targetThingB != null)
            {
                obj.EndOnDespawnedOrNull(TargetIndex.B, JobCondition.Incompletable);
            }
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);

            if (targetThingC != null)
            {
                obj.EndOnDespawnedOrNull(TargetIndex.C, JobCondition.Incompletable);
                if (targetThingC is Building_AutomatedFactory)
                {
                    yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
                    yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer(TargetIndex.C, obj.pawn);
                }
                else
                {
                    obj.FailOnDestroyedNullOrForbidden(TargetIndex.C);
                    yield return Toils_Reserve.Reserve(TargetIndex.C, 1);
                    yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);
                    yield return Toils_Haul.StartCarryThing(TargetIndex.C);
                }
            }

            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

            var pawnDrawer = obj.pawn.GetPawnDrawTracker();
            var pawnFaceTarget = obj.TargetThingA().OccupiedRect().ClosestCellTo(obj.pawn.Position);
            var relax = new Toil();
            relax.defaultCompleteMode = ToilCompleteMode.Delay;
            relax.defaultDuration = obj.pawn.CurJob.def.joyDuration;
            relax.tickAction = new Action(() =>
            {
                pawnDrawer.rotator.FaceCell(pawnFaceTarget);
                obj.pawn.GainComfortFromCellIfPossible();
                JoyUtility.JoyTickCheckEnd(obj.pawn, false, 1f);
            }
            );
            relax.AddFinishAction(() =>
               JoyUtility.TryGainRecRoomThought(obj.pawn)
            );
            relax.socialMode = RandomSocialMode.SuperActive;
            yield return relax;

            if (targetThingC != null)
            {
                yield return Toils_Ingest.FinalizeIngest(obj.pawn, TargetIndex.C);
            }
        }

    }

}
