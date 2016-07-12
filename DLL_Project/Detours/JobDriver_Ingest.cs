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

    internal static class _JobDriver_Ingest
    {

        internal const TargetIndex FoodInd = TargetIndex.A;
        internal const TargetIndex TableCellInd = TargetIndex.B;
        internal const TargetIndex AlcoholInd = TargetIndex.C;

        #region Detoured Methods

        internal static bool _UsingNutrientPasteDispenser( this JobDriver_Ingest obj )
        {
            var foodSource = obj.TargetThing( FoodInd );
            if( foodSource is Building_AutomatedFactory )
            {   // JobGivers will check for OutputToPawnsDirectly
                return true;
            }
            return foodSource is Building_NutrientPasteDispenser;
        }

        internal static IEnumerable<Toil> _PrepareToEatToils_Dispenser( this JobDriver_Ingest obj )
        {
            var foodSource = obj.TargetThing( FoodInd );
            var alcohol = obj.TargetThing( AlcoholInd );

            yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.InteractionCell )
                                   .FailOnDespawnedNullOrForbidden( FoodInd );
            if( foodSource is Building_NutrientPasteDispenser )
            {
                yield return Toils_Ingest.TakeMealFromDispenser( FoodInd, obj.pawn );
            }
            else if( foodSource is Building_AutomatedFactory )
            {
                if( alcohol == null )
                {
                    yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( FoodInd, obj.pawn );
                }
                else
                {
                    yield return Toils_FoodSynthesizer.TakeAlcoholFromSynthesizer( AlcoholInd, obj.pawn );
                }
            }
            yield return Toils_Ingest.CarryIngestibleToChewSpot( obj.pawn )
                                     .FailOnDestroyedNullOrForbidden( FoodInd );
            yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, FoodInd );
        }

        #endregion

    }

}
