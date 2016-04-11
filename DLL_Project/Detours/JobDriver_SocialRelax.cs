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
        
        internal const TargetIndex GatherSpotInd = TargetIndex.A;
        internal const TargetIndex OccupyInd = TargetIndex.B;
        internal const TargetIndex AlcoholInd = TargetIndex.C;
        internal const TargetIndex DispenserInd = TargetIndex.C;

        internal static FieldInfo _GetPawnDrawTracker;

        #region Reflected Fields

        internal static Pawn_DrawTracker GetPawnDrawTracker( this Pawn pawn )
        {
            if( _GetPawnDrawTracker == null )
            {
                _GetPawnDrawTracker = typeof( Pawn ).GetField( "drawer", BindingFlags.Instance | BindingFlags.NonPublic );
                if( _GetPawnDrawTracker == null )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Unable to get 'drawer' in class 'Pawn'",
                        "CommunityCoreLibrary.Detour.JobDriver_SocialRelax");
                    return null;
                }
            }
            return (Pawn_DrawTracker)_GetPawnDrawTracker.GetValue( pawn );
        }

        #endregion

        #region Helper Methods

        internal static bool AlcoholOrDispenser( this JobDriver_SocialRelax obj )
        {
            if( obj.Alcohol() != null )
            {
                return true;
            }
            return( obj.Dispenser() != null );
        }

        internal static Thing GatherSpot( this JobDriver_SocialRelax obj )
        {
            return obj.pawn.CurJob.GetTarget( GatherSpotInd ).Thing;
        }

        internal static IntVec3 OccupySpot( this JobDriver_SocialRelax obj )
        {
            return obj.pawn.CurJob.GetTarget( OccupyInd ).Cell;
        }

        internal static Thing OccupyThing( this JobDriver_SocialRelax obj )
        {
            return obj.pawn.CurJob.GetTarget( OccupyInd ).Thing;
        }

        internal static Thing Alcohol( this JobDriver_SocialRelax obj )
        {
            return obj.pawn.CurJob.GetTarget( AlcoholInd ).Thing;
        }

        internal static Thing Dispenser( this JobDriver_SocialRelax obj )
        {
            return obj.pawn.CurJob.GetTarget( DispenserInd ).Thing;
        }

        #endregion

        #region Detoured Methods

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_SocialRelax obj )
        {
            obj.EndOnDespawnedOrNull( GatherSpotInd, JobCondition.Incompletable );

            if( obj.OccupyThing() != null )
            {
                obj.EndOnDespawnedOrNull( OccupyInd, JobCondition.Incompletable );
            }
            yield return Toils_Reserve.Reserve( OccupyInd, 1 );

            if( obj.AlcoholOrDispenser() )
            {
                if( obj.Dispenser() is Building_AutomatedFactory )
                {
                    obj.EndOnDespawnedOrNull( DispenserInd, JobCondition.Incompletable );
                    yield return Toils_Goto.GotoThing( DispenserInd, PathEndMode.InteractionCell );
                    yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( AlcoholInd, obj.pawn );
                }
                else
                {
                    obj.FailOnDestroyedNullOrForbidden( AlcoholInd );
                    yield return Toils_Reserve.Reserve( AlcoholInd, 1 );
                    yield return Toils_Goto.GotoThing( AlcoholInd, PathEndMode.OnCell );
                    yield return Toils_Haul.StartCarryThing( AlcoholInd );
                }
            }

            yield return Toils_Goto.GotoCell( OccupyInd, PathEndMode.OnCell );

            var pawnDrawer = obj.pawn.GetPawnDrawTracker();
            var pawnFaceTarget = obj.GatherSpot().OccupiedRect().ClosestCellTo( obj.pawn.Position );
            var relax = new Toil();
            relax.defaultCompleteMode = ToilCompleteMode.Delay;
            relax.defaultDuration = obj.pawn.CurJob.def.joyDuration;
            relax.tickAction = new Action( () =>
            {
                pawnDrawer.rotator.FaceCell( pawnFaceTarget );
                obj.pawn.GainComfortFromCellIfPossible();
                JoyUtility.JoyTickCheckEnd( obj.pawn, JoyTickFullJoyAction.GoToNextToil, 1f );
            }
            );
            relax.AddFinishAction(() =>
               JoyUtility.TryGainRecRoomThought( obj.pawn )
            );
            relax.socialMode = RandomSocialMode.SuperActive;
            yield return relax;

            if( obj.Alcohol() != null )
            {
                yield return Toils_Ingest.FinalizeIngest( obj.pawn, AlcoholInd );
            }
        }

        #endregion

    }

}
