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
        
        internal const TargetIndex GatherSpotParentInd = TargetIndex.A;
        internal const TargetIndex ChairOrSpotInd = TargetIndex.B;
        internal const TargetIndex OptionalDrinkInd = TargetIndex.C;

        #region Helper Methods

        internal static Thing GatherSpotParent( this JobDriver_SocialRelax obj )
        {
            return obj.TargetThing( GatherSpotParentInd );
        }

        internal static IntVec3 ClosestGatherSpotParentCell( this JobDriver_SocialRelax obj )
        {
            return obj.GatherSpotParent().OccupiedRect().ClosestCellTo( obj.pawn.Position );
        }

        internal static bool HasChair( this JobDriver_SocialRelax obj )
        {
            return obj.TargetThing( ChairOrSpotInd ) != null;
        }

        internal static IntVec3 OccupySpot( this JobDriver_SocialRelax obj )
        {
            return obj.TargetCell( ChairOrSpotInd );
        }

        internal static Thing OccupyThing( this JobDriver_SocialRelax obj )
        {
            return obj.TargetThing( ChairOrSpotInd );
        }

        internal static bool IsDrink( this JobDriver_SocialRelax obj )
        {
            var thing = obj.TargetThing( OptionalDrinkInd );
            if( thing == null )
            {
                return false;
            }
            return thing.def.IsAlcohol();
        }

        internal static bool IsDispenser( this JobDriver_SocialRelax obj )
        {
            var thing = obj.TargetThing( OptionalDrinkInd );
            if( thing == null )
            {
                return false;
            }
            return thing is Building_AutomatedFactory;
        }

        internal static bool HasDrinkOrDispenser( this JobDriver_SocialRelax obj )
        {
            return obj.TargetThing( OptionalDrinkInd ) != null;
        }

        internal static Thing Alcohol( this JobDriver_SocialRelax obj )
        {
            return obj.TargetThing( OptionalDrinkInd );
        }

        internal static Building_AutomatedFactory Dispenser( this JobDriver_SocialRelax obj )
        {
            return obj.TargetThing( OptionalDrinkInd ) as Building_AutomatedFactory;
        }

        #endregion

        #region Detoured Methods

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_SocialRelax obj )
        {
            obj.EndOnDespawnedOrNull( GatherSpotParentInd, JobCondition.Incompletable );

            if( obj.HasChair() )
            {
                obj.EndOnDespawnedOrNull( ChairOrSpotInd, JobCondition.Incompletable );
            }
            yield return Toils_Reserve.Reserve( ChairOrSpotInd, 1 );

            if( obj.HasDrinkOrDispenser() )
            {
                obj.FailOnDestroyedNullOrForbidden( OptionalDrinkInd );
                yield return Toils_Reserve.Reserve( OptionalDrinkInd, 1 );
                if( obj.IsDispenser() )
                {
                    yield return Toils_Goto.GotoThing( OptionalDrinkInd, PathEndMode.InteractionCell );
                    yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( OptionalDrinkInd, obj.pawn );
                }
                else
                {
                    yield return Toils_Goto.GotoThing( OptionalDrinkInd, PathEndMode.OnCell );
                    yield return Toils_Haul.StartCarryThing( OptionalDrinkInd );
                }
            }

            yield return Toils_Goto.GotoCell( ChairOrSpotInd, PathEndMode.OnCell );

            var relax = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = obj.pawn.CurJob.def.joyDuration,
                tickAction = () =>
                {
                    var pawnDrawer = obj.pawn.GetPawnDrawTracker();
                    var pawnFaceTarget = obj.ClosestGatherSpotParentCell();
                    pawnDrawer.rotator.FaceCell( pawnFaceTarget );
                    obj.pawn.GainComfortFromCellIfPossible();
                    JoyUtility.JoyTickCheckEnd( obj.pawn, JoyTickFullJoyAction.GoToNextToil, 1f );
                }
            };
            relax.AddFinishAction( () =>
               JoyUtility.TryGainRecRoomThought( obj.pawn )
            );
            relax.socialMode = RandomSocialMode.SuperActive;
            yield return relax;

            if( obj.IsDrink() )
            {
                yield return Toils_Ingest.FinalizeIngest( obj.pawn, OptionalDrinkInd );
            }
        }

        #endregion

    }

}
