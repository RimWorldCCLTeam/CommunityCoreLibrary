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

    internal class _JobDriver_SocialRelax : JobDriver_SocialRelax
    {
        
        internal const TargetIndex GatherSpotParentInd = TargetIndex.A;
        internal const TargetIndex ChairOrSpotInd = TargetIndex.B;
        internal const TargetIndex OptionalIngestibleInd = TargetIndex.C;

        #region Helper Methods

        internal Thing GatherSpotParent
        {
            get
            {
                return this.TargetThing( GatherSpotParentInd );
            }
        }

        internal IntVec3 ClosestGatherSpotParentCell
        {
            get
            {
                return this.GatherSpotParent.OccupiedRect().ClosestCellTo( this.pawn.Position );
            }
        }

        internal bool HasChair
        {
            get
            {
                return this.TargetThing( ChairOrSpotInd ) != null;
            }
        }

        internal IntVec3 OccupySpot
        {
            get
            {
                return this.TargetCell( ChairOrSpotInd );
            }
        }

        internal Thing OccupyThing
        {
            get
            {
                return this.TargetThing( ChairOrSpotInd );
            }
        }

        internal bool HasIngestible
        {
            get
            {
                var thing = this.CurJob.GetTarget(TargetIndex.C).Thing;
                if( thing == null )
                {
                    CCL_Log.Message("Thing is null");
                    return false;
                }
                CCL_Log.Message("is ingestible? thing: " + thing.def.label);
                if( thing.def.IsIngestible )
                {
                    return true;
                }
                return thing.def.IsDrug;
            }
        }

        internal bool IsDispenser
        {
            get
            {
                var thing = this.TargetThing( OptionalIngestibleInd );
                if( thing == null )
                {
                    return false;
                }
                return thing is Building_AutomatedFactory;
            }
        }

        internal bool HasIngestibleOrDispenser
        {
            get
            {
                return this.TargetThing( OptionalIngestibleInd ) != null;
            }
        }

        internal Thing Drug
        {
            get
            {
                return this.TargetThing( OptionalIngestibleInd );
            }
        }

        internal Building_AutomatedFactory Dispenser
        {
            get
            {
                return this.TargetThing( OptionalIngestibleInd ) as Building_AutomatedFactory;
            }
        }

        #endregion

        #region Detoured Methods

        [DetourClassMethod( typeof( JobDriver_SocialRelax ), "MakeNewToils" )]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull( GatherSpotParentInd, JobCondition.Incompletable );

            if( this.HasChair )
            {
                this.EndOnDespawnedOrNull( ChairOrSpotInd, JobCondition.Incompletable );
            }
            yield return Toils_Reserve.Reserve( ChairOrSpotInd, 1 );

            if( this.HasIngestibleOrDispenser )
            {
                this.FailOnDestroyedNullOrForbidden( OptionalIngestibleInd );
                yield return Toils_Reserve.Reserve( OptionalIngestibleInd, 1 );
                if( this.IsDispenser )
                {   // TODO:  Investigate and expand drug system to use factories
                    // This should never be executed as the underlying methods to return factories for drugs should not currently return factories
                    yield return Toils_Goto.GotoThing( OptionalIngestibleInd, PathEndMode.InteractionCell );
                    yield return Toils_FoodSynthesizer.TakeDrugFromSynthesizer( OptionalIngestibleInd, this.pawn );
                }
                else
                {
                    yield return Toils_Goto.GotoThing( OptionalIngestibleInd, PathEndMode.OnCell )
                                           .FailOnSomeonePhysicallyInteracting( OptionalIngestibleInd );
                    yield return Toils_Haul.StartCarryThing( OptionalIngestibleInd );
                }
            }

            yield return Toils_Goto.GotoCell( ChairOrSpotInd, PathEndMode.OnCell );

            var relax = new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = this.pawn.CurJob.def.joyDuration,
                tickAction = () =>
                {
                    var pawnDrawer = this.pawn.GetPawnDrawTracker();
                    var pawnFaceTarget = this.ClosestGatherSpotParentCell;
                    pawnDrawer.rotator.FaceCell( pawnFaceTarget );
                    this.pawn.GainComfortFromCellIfPossible();
                    JoyUtility.JoyTickCheckEnd( this.pawn, JoyTickFullJoyAction.GoToNextToil, 1f );
                }
            };
            relax.AddFinishAction( () =>
               JoyUtility.TryGainRecRoomThought( this.pawn )
            );
            relax.socialMode = RandomSocialMode.SuperActive;
            Toils_Ingest.AddIngestionEffects( relax, this.pawn, OptionalIngestibleInd, GatherSpotParentInd );
            yield return relax;
            if( this.HasIngestibleOrDispenser )
            {
                yield return Toils_Ingest.FinalizeIngest( this.pawn, OptionalIngestibleInd );
            }
        }

        #endregion

    }

}
