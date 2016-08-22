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

        internal const TargetIndex FoodInd = TargetIndex.A;
        internal const TargetIndex DelivereeInd = TargetIndex.B;
        internal const float FeedDurationMultiplier = 1.5f;

        internal static IEnumerable<Toil> _MakeNewToils( this JobDriver_FoodFeedPatient obj )
        {
            var foodThing = obj.TargetThing( FoodInd );
            var deliveree = (Pawn) obj.TargetThing( DelivereeInd );

            obj.FailOnDespawnedNullOrForbidden( DelivereeInd );
            obj.FailOn( () =>
            {
                return !FoodUtility.ShouldBeFedBySomeone( deliveree );
            } );

            yield return Toils_Reserve.Reserve( DelivereeInd, 1 );

            if( foodThing is Building )
            {
                yield return Toils_Reserve.Reserve( FoodInd, 1 );
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
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch ).FailOnForbidden( FoodInd );
                yield return Toils_Ingest.PickupIngestible( FoodInd, deliveree );
            }

            yield return Toils_Goto.GotoThing( DelivereeInd, PathEndMode.Touch );
            yield return Toils_Ingest.ChewIngestible( deliveree, FeedDurationMultiplier, FoodInd );
            yield return Toils_Ingest.FinalizeIngest( deliveree, FoodInd );
        }

    }

}
