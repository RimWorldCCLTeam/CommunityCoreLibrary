using System;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _DrugAIUtility
    {
        
        #region Detoured Methods

        [DetourMember( typeof( DrugAIUtility ) )]
        internal static Job                 _IngestAndTakeToInventoryJob( Thing drug, Pawn pawn, int maxNumToCarry = 9999 )
        {
            var synthesizer = drug as Building_AutomatedFactory;
            if( synthesizer != null )
            {
                if( !synthesizer.IsConsidering( pawn ) )
                {
                    CCL_Log.Error( string.Format( "{0} has not considered using {1}", pawn.LabelShort, synthesizer.ThingID ) );
                    return null;
                }
                if( !synthesizer.ReserveForUseBy( pawn, synthesizer.ConsideredProduct) )
                {
                    CCL_Log.Error( string.Format( "{0} tried to use {1} but could not reserve to produce {2}!", pawn.LabelShort, synthesizer.ThingID, synthesizer.ConsideredProduct.defName ) );
                    return null;
                }
            }
            var job = new Job( JobDefOf.Ingest, drug );
            if( synthesizer != null )
            {
                job.maxNumToCarry = 1;
            }
            else
            {
                job.maxNumToCarry = Mathf.Min( new int[]
                {
                    drug.stackCount,
                    drug.def.ingestible.maxNumToIngestAtOnce,
                    maxNumToCarry
                } );
                if(
                    ( drug.Spawned )&&
                    ( pawn.drugs != null )&&
                    ( !pawn.inventory.container.Contains( drug.def ) )
                )
                {
                    var drugPolicy = pawn.drugs.CurrentPolicy[ drug.def ];
                    if( drugPolicy.allowScheduled )
                    {
                        job.takeExtraIngestibles = drugPolicy.takeToInventory;
                    }
                }
            }
            return job;
        }

        #endregion

    }

}
