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
        
        internal const TargetIndex          GatherSpotParentInd = TargetIndex.A;
        internal const TargetIndex          ChairOrSpotInd = TargetIndex.B;
        internal const TargetIndex          OptionalIngestibleInd = TargetIndex.C;

        #region Helper Methods

        internal Thing                      GatherSpotParent
        {
            get
            {
                return this.TargetThing( GatherSpotParentInd );
            }
        }

        internal IntVec3                    ClosestGatherSpotParentCell
        {
            get
            {
                return this.GatherSpotParent.OccupiedRect().ClosestCellTo( this.pawn.Position );
            }
        }

        internal bool                       HasChair
        {
            get
            {
                return this.TargetThing( ChairOrSpotInd ) != null;
            }
        }

        internal IntVec3                    OccupySpot
        {
            get
            {
                return this.TargetCell( ChairOrSpotInd );
            }
        }

        internal Thing                      OccupyThing
        {
            get
            {
                return this.TargetThing( ChairOrSpotInd );
            }
        }

        internal bool                       HasIngestible
        {
            get
            {
                return this.TargetThing( OptionalIngestibleInd ) != null;
            }
        }

        internal bool                       IsDispenser
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

        internal Thing                      Drug
        {
            get
            {
                var thing = this.TargetThing( OptionalIngestibleInd );
                if(
                    ( thing == null )||
                    ( !thing.IngestibleNow )
                )
                {
                    return null;
                }
                return thing;
            }
        }

        internal Building_AutomatedFactory  Dispenser
        {
            get
            {
                return this.TargetThing( OptionalIngestibleInd ) as Building_AutomatedFactory;
            }
        }

        #endregion

        #region Detoured Methods

        [DetourMember]
        internal IEnumerable<Toil>          _MakeNewToils()
        {
            this.EndOnDespawnedOrNull( GatherSpotParentInd, JobCondition.Incompletable );

            if( this.HasChair )
            {
                this.EndOnDespawnedOrNull( ChairOrSpotInd, JobCondition.Incompletable );
            }
            yield return Toils_Reserve.Reserve( ChairOrSpotInd, 1 );

            if( this.HasIngestible )
            {
                this.FailOnDestroyedNullOrForbidden( OptionalIngestibleInd );
                yield return Toils_Reserve.Reserve( OptionalIngestibleInd, 1 );
                if( this.IsDispenser )
                {
                    yield return Toils_Goto.GotoThing( OptionalIngestibleInd, PathEndMode.InteractionCell );
                    // CALLER MUST USE Building_AutomatedFactory.ReserveForUseBy() BEFORE USING THIS METHOD!
                    //yield return Toils_FoodSynthesizer.TakeDrugFromSynthesizer( OptionalIngestibleInd, this.pawn );
                    yield return Toils_FoodSynthesizer.TakeFromSynthesier( OptionalIngestibleInd, this.pawn );
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
            if( this.HasIngestible )
            {
                yield return Toils_Ingest.FinalizeIngest( this.pawn, OptionalIngestibleInd );
            }
            yield break;
        }

        #endregion

    }

}
