using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal class _JoyGiver_Ingest : JoyGiver_Ingest
    {

        internal static bool                        ThisDefIsValid( Pawn pawn, ThingDef thingDef )
        {
            if(
                ( thingDef.ingestible == null )||
                ( thingDef.ingestible.joyKind == null )||
                ( thingDef.ingestible.joy <= 0f )
            )
            {
                return false;
            }
            if(
                ( thingDef.IsDrug )&&
                ( pawn.drugs != null )&&
                ( !pawn.drugs.CurrentPolicy[ thingDef ].allowedForJoy )&&
                ( pawn.story != null )
            )
            {
                var drugDesire = pawn.story.traits.DegreeOfTrait( TraitDefOf.DrugDesire );
                if(
                    ( drugDesire <= 0 )&&
                    ( !pawn.InMentalState )
                )
                {
                    return false;
                }
            }
            return true;
        }

        #region Detoured Methods

        [DetourClassMethod( typeof( JoyGiver_Ingest ), "BestIngestItem" )]
        internal Thing                              _BestIngestItem( Pawn pawn, Predicate<Thing> extraValidator )
        {
            Predicate<Thing> validator = (Thing t) =>
            (
                ( this.CanUseIngestItemForJoy( pawn, t ) )&&
                (
                    ( extraValidator == null )||
                    ( extraValidator( t ) )
                )
            );
            
            var container = pawn.inventory.container;
            for( int index = 0; index < container.Count; index++ )
            {
                var containerItem = container[ index ];
                if(
                    ( this.SearchSetWouldInclude( containerItem ) )&&
                    ( validator( containerItem ) )
                )
                {
                    return containerItem;
                }
            }
            var searchSet = this.SearchSet;
            if( searchSet.Count < 1 )
            {
                return null;
            }
            return GenClosest.ClosestThing_Global_Reachable(
                pawn.Position,
                searchSet,
                PathEndMode.InteractionCell,
                TraverseParms.For(
                    pawn,
                    Danger.Deadly,
                    TraverseMode.ByPawn,
                    false ),
                9999f,
                validator,
                null );
        }

        [DetourClassMethod( typeof( JoyGiver_Ingest ), "CanUseIngestItemForJoy" )]
        internal bool                               _CanUseIngestItemForJoy( Pawn pawn, Thing t )
        {
            if(
                ( t.Spawned )&&
                (
                    ( !pawn.CanReserve( t, 1 ) )||
                    ( t.IsForbidden( pawn ) )||
                    ( !t.IsSociallyProper( pawn ) )
                )
            )
            {
                return false;
            }
            var synthesizer = t as Building_AutomatedFactory;
            if( synthesizer == null )
            {
                return ThisDefIsValid( pawn, t.def );
            }
            var products = synthesizer.AllProductionReadyRecipes( FoodSynthesis.IsJoy, FoodSynthesis.SortJoy );
            if( products.NullOrEmpty() )
            {
                return false;
            }
            foreach( var product in products )
            {
                if( ThisDefIsValid( pawn, product ) )
                {
                    if( synthesizer.ConsiderFor( product, pawn ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [DetourClassMethod( typeof( JoyGiver_Ingest ), "CreateIngestJob" )]
        internal Job                                _CreateIngestJob( Thing ingestible, Pawn pawn )
        {
            var synthesizer = ingestible as Building_AutomatedFactory;
            if( synthesizer != null )
            {
                if( !synthesizer.IsConsidering( pawn ) )
                {
                    CCL_Log.Error( string.Format( "{0} has not considered using {1}", pawn.LabelShort, synthesizer.ThingID ) );
                    return null;
                }
                if( !synthesizer.ReserveForUseBy( pawn, synthesizer.ConsideredProduct ) )
                {
                    CCL_Log.Error( string.Format( "{0} tried to use {1} but could not reserve to produce {2}!", pawn.LabelShort, synthesizer.ThingID, synthesizer.ConsideredProduct.defName ) );
                    return null;
                }
            }
            var job = new Job( JobDefOf.Ingest, ingestible );
            if( synthesizer == null )
            {
                job.maxNumToCarry = Mathf.Min( ingestible.stackCount, ingestible.def.ingestible.maxNumToIngestAtOnce );
            }
            else
            {
                job.maxNumToCarry = 1;
            }
            return job;
        }
        #endregion

    }

}
