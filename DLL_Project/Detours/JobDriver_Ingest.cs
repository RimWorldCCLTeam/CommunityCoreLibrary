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

        internal static string _GetReport( this JobDriver_Ingest obj )
        {
            var curJob = obj.pawn.jobs.curJob;
            var foodSource = obj.TargetThing( FoodInd );
            ThingDef foodDef = null;
            if( foodSource is Building_AutomatedFactory )
            {
                foodDef = ((Building_AutomatedFactory)foodSource).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
            }
            else if( foodSource is Building_NutrientPasteDispenser )
            {
                foodDef = ((Building_NutrientPasteDispenser)foodSource).DispensableDef;
            }
            if( foodDef != null )
            {
                return curJob.def.reportString.Replace( "TargetA", foodDef.label );
            }
            var str = curJob.def.reportString;
            str = !curJob.targetA.HasThing ? str.Replace( "TargetA", "AreaLower".Translate() ) : str.Replace( "TargetA", curJob.targetA.Thing.LabelShort );
            str = !curJob.targetB.HasThing ? str.Replace( "TargetB", "AreaLower".Translate() ) : str.Replace( "TargetB", curJob.targetB.Thing.LabelShort );
            str = !curJob.targetC.HasThing ? str.Replace( "TargetC", "AreaLower".Translate() ) : str.Replace( "TargetC", curJob.targetC.Thing.LabelShort );
            return str;
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
