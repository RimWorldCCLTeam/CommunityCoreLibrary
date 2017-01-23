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

        internal const TargetIndex          IngestibleInd = TargetIndex.A;
        internal const TargetIndex          TableCellInd = TargetIndex.B;
        internal const TargetIndex          ExtraIngestiblesToCollectInd = TargetIndex.C;

        #region Helper Methods

        internal Thing                      IngestibleSource
        {
            get
            {
                return this.TargetThing( IngestibleInd );
            }
        }

        internal bool                       IsUsingDrugs
        {
            get
            {
                // TODO:  Resolve factories
                return false;
            }
        }

        internal ThingDef                   IngestibleDefFromSource( Thing foodSource )
        {
            if( foodSource is Building_AutomatedFactory )
            {   // JobGivers will check for OutputToPawnsDirectly
                return ((Building_AutomatedFactory)foodSource).ReservedThingDef;
            }
            if( foodSource is Building_NutrientPasteDispenser )
            {
                return ((Building_NutrientPasteDispenser)foodSource).DispensableDef;
            }
            return foodSource.def;
        }

        #endregion

        #region Detoured Methods

        // TODO: we probably don't need this anymore; it matches the existing binary (NuOfBelthasar)
        [DetourMember]
        internal bool                       _UsingNutrientPasteDispenser
        {
            get
            {
                return IngestibleSource.def.IsFoodDispenser;
            }
        }

        [DetourMember]
        internal string                     _GetReport()
        {
            var curJob = this.pawn.jobs.curJob;
            var foodSource = IngestibleSource;
            if( foodSource == null )
            {
                return this.ReportStringProcessed( this.CurJob.def.reportString );
            }
            var foodDef = FoodUtility.GetFinalIngestibleDef( foodSource );
            if(
                ( foodDef == null )||
                ( string.IsNullOrEmpty( foodDef.ingestible.ingestReportString ) )
            )
            {
                return this.ReportStringProcessed( this.CurJob.def.reportString );
            }
            return string.Format( foodDef.ingestible.ingestReportString, foodDef.label );
        }

        // TODO: check for changes in A16; I can't figure out how to decompile an iterator (NuOfBelthasar)
        [DetourMember]
        internal IEnumerable<Toil>          _PrepareToIngestToils_Dispenser()
        {
            var ingestibleSource = IngestibleSource;

            yield return Toils_Reserve.Reserve( IngestibleInd, 1 );
            this.AddFinishAction( () =>
            {   // Release on early exit
                if( pawn.Map.reservationManager.ReservedBy( ingestibleSource, pawn ) )
                {
                    pawn.Map.reservationManager.Release( ingestibleSource, pawn );
                }
            } );

            yield return Toils_Goto.GotoThing( IngestibleInd, PathEndMode.InteractionCell )
                                   .FailOnDespawnedNullOrForbidden( IngestibleInd );

            if( ingestibleSource is Building_NutrientPasteDispenser )
            {
                yield return Toils_Ingest.TakeMealFromDispenser( IngestibleInd, this.pawn );
            }
            else if( ingestibleSource is Building_AutomatedFactory )
            {
                // CALLER MUST USE Building_AutomatedFactory.ReserveForUseBy() BEFORE USING THIS METHOD!
                yield return Toils_FoodSynthesizer.TakeFromSynthesier( IngestibleInd, this.pawn );
                /*
                if( !IsUsingDrugs )
                {
                    yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( IngestibleInd, this.pawn );
                }
                else
                {
                    yield return Toils_FoodSynthesizer.TakeDrugFromSynthesizer( IngestibleInd, this.pawn );
                }
                */
            }
            yield return Toils_Ingest.CarryIngestibleToChewSpot( this.pawn, IngestibleInd )
                                     .FailOnDestroyedNullOrForbidden( IngestibleInd );
            yield return Toils_Ingest.FindAdjacentEatSurface( TableCellInd, IngestibleInd );
            yield break;
        }

        #endregion

    }

}
