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

    internal class _JobDriver_Ingest : JobDriver_Ingest
    {

        internal const TargetIndex IngestibleInd = TargetIndex.A;
        internal const TargetIndex TableCellInd = TargetIndex.B;
        internal const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

        #region Helper Methods

        internal Thing IngestibleSource
        {
            get
            {
                return this.TargetThing( IngestibleInd );
            }
        }

        internal bool IsUsingDrugs
        {
            get
            {
                // TODO:  Figure this out???
                return false;
            }
        }

        internal ThingDef FoodDefFromSource( Thing foodSource )
        {
            if( foodSource is Building_AutomatedFactory )
            {   // JobGivers will check for OutputToPawnsDirectly
                return ((Building_AutomatedFactory)foodSource).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
            }
            if( foodSource is Building_NutrientPasteDispenser )
            {
                return ((Building_NutrientPasteDispenser)foodSource).DispensableDef;
            }
            return foodSource.def;
        }

        #endregion

        #region Detoured Methods

        [DetourClassProperty( typeof( JobDriver_Ingest ), "UsingNutrientPasteDispenser" )]
        internal bool UsingFoodMachine
        {
            get
            {
                return IngestibleSource.def.IsFoodDispenser;
            }
        }

        [DetourClassMethod( typeof( JobDriver_Ingest ), "GetReport" )]
        public override string GetReport()
        {
            var curJob = this.pawn.jobs.curJob;
            var foodSource = IngestibleSource;
            if( foodSource == null )
			{
				return this.ReportStringProcessed(this.CurJob.def.reportString);
			}
            var foodDef = FoodUtility.GetFinalIngestibleDef( foodSource );
            if(
                ( foodDef == null )||
                ( string.IsNullOrEmpty( foodDef.ingestible.ingestReportString ) )
            )
			{
				return this.ReportStringProcessed(this.CurJob.def.reportString);
			}
            return string.Format( foodDef.ingestible.ingestReportString, foodSource.LabelShort );
        }

        [DetourClassMethod( typeof( JobDriver_Ingest ), "PrepareToIngestToils_Dispenser" )]
        internal IEnumerable<Toil> _PrepareToIngestToils_Dispenser()
        {
            var ingestibleSource = IngestibleSource;

            yield return Toils_Reserve.Reserve( IngestibleInd, 1 );
            yield return Toils_Goto.GotoThing( IngestibleInd, PathEndMode.InteractionCell )
                                   .FailOnDespawnedNullOrForbidden( IngestibleInd );

            this.AddFinishAction( () =>
            {   // Release on early exit
                if( Find.Reservations.ReservedBy( ingestibleSource, pawn ) )
                {
                    Find.Reservations.Release( ingestibleSource, pawn );
                }
            } );

            if( ingestibleSource is Building_NutrientPasteDispenser )
            {
                yield return Toils_Ingest.TakeMealFromDispenser( IngestibleInd, this.pawn );
            }
            else if( ingestibleSource is Building_AutomatedFactory )
            {
                if( !IsUsingDrugs )
                {
                    yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( IngestibleInd, this.pawn );
                }
                else
                {
                    yield return Toils_FoodSynthesizer.TakeDrugFromSynthesizer( IngestibleInd, this.pawn );
                }
            }
            yield return Toils_Ingest.CarryIngestibleToChewSpot( this.pawn, IngestibleInd )
                                     .FailOnDestroyedNullOrForbidden( IngestibleInd );
            yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, IngestibleInd );
        }

        #endregion

    }

}
