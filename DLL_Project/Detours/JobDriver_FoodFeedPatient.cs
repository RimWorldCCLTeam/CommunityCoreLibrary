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

    internal class _JobDriver_FoodFeedPatient : JobDriver_FoodFeedPatient
    {

        internal const TargetIndex FoodInd = TargetIndex.A;
        internal const TargetIndex DelivereeInd = TargetIndex.B;
        internal const float FeedDurationMultiplier = 1.5f;

        [DetourClassMethod( typeof( JobDriver_FoodFeedPatient ), "MakeNewToils" )]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var foodThing = this.TargetThing( FoodInd );
            var deliveree = (Pawn) this.TargetThing( DelivereeInd );

            this.FailOnDespawnedNullOrForbidden( DelivereeInd );
            this.FailOn( () =>
            {
                return !FoodUtility.ShouldBeFedBySomeone( deliveree );
            } );

            yield return Toils_Reserve.Reserve( DelivereeInd, 1 );

            if(
                ( this.pawn.inventory != null )&&
                ( this.pawn.inventory.Contains( foodThing ) )
            )
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier( this.pawn, DelivereeInd );
            }
            else if( foodThing is Building )
            {
                yield return Toils_Reserve.Reserve( FoodInd, 1 );
                this.AddFinishAction( () =>
                {
                    if( Find.Reservations.ReservedBy( foodThing, pawn ) )
                    {   // Release reservation if aborted early
                        Find.Reservations.Release( foodThing, pawn );
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
                this.AddFinishAction( () =>
                {
                    if( Find.Reservations.ReservedBy( foodThing, pawn ) )
                    {   // Release reservation if aborted early
                        Find.Reservations.Release( foodThing, pawn );
                    }
                } );
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch ).FailOnForbidden( FoodInd );
                yield return Toils_Ingest.PickupIngestible( FoodInd, deliveree );
            }

            yield return Toils_Goto.GotoThing( DelivereeInd, PathEndMode.Touch );
            yield return Toils_Ingest.ChewIngestible( deliveree, FeedDurationMultiplier, FoodInd );
            yield return Toils_Ingest.FinalizeIngest( deliveree, FoodInd );
            yield break;
        }

    }

}
