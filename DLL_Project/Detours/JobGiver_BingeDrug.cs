using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal class _JobGiver_BingeDrug : JobGiver_BingeDrug
    {

        internal static MethodInfo          _GetChemical;

        static                              _JobGiver_BingeDrug()
        {
            _GetChemical = typeof( JobGiver_BingeDrug ).GetMethod( "GetChemical", Controller.Data.UniversalBindingFlags );
            if( _GetChemical == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'GetChemical' in 'JobGiver_BingeDrug'",
                    "Detour.JobGiver_BingeDrug" );
            }
        }

        #region Reflected Methods

        internal ChemicalDef                GetChemical( Pawn pawn )
        {
            return (ChemicalDef) _GetChemical.Invoke( this, new object[] { pawn } );
        }

        #endregion

        #region Helper Methods

        internal bool                       DrugIsUsableBy( Thing drugSource, ThingDef drugDef, ChemicalDef chemical, Hediff overdose, Pawn pawn )
        {
            var drugProps = drugDef.GetCompProperties<CompProperties_Drug>();
            if( drugProps == null ) // This check should never fail
            {
                return false;
            }
            return(
                ( drugProps.chemical == chemical )&&
                (
                    ( overdose == null )||
                    ( !drugProps.CanCauseOverdose )||
                    ( overdose.Severity + drugProps.overdoseSeverityOffset.max < 0.786f )
                )&&
                (
                    ( pawn.Position.InHorDistOf( drugSource.Position, 60f ) )||
                    ( drugSource.Position.Roofed( drugSource.Map ) )||
                    ( drugSource.Map.areaManager.Home[ drugSource.Position ] )||
                    ( StoreUtility.GetSlotGroup( drugSource ) != null )
                )
            );
        }

        #endregion

        #region Detoured Methods

        [DetourMember]
        internal Thing                      _BestIngestTarget( Pawn pawn )
        {
            var chemical = GetChemical( pawn );
            if( chemical == null )
            {
                Log.ErrorOnce( "Tried to binge on null chemical.", 1393746152 );
                return null;
            }
            var overdose = pawn.health.hediffSet.GetFirstHediffOfDef( HediffDefOf.DrugOverdose );
            var ingestibleThing = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup( ThingRequestGroup.Drug ),
                PathEndMode.InteractionCell,
                TraverseParms.For(
                    pawn,
                    pawn.NormalMaxDanger(),
                    TraverseMode.ByPawn,
                    false ),
                9999f,
                (thing) =>
            {
                if(
                    (
                        ( !this.IgnoreForbid( pawn ) )&&
                        ( thing.IsForbidden( pawn) )
                    )||
                    ( !pawn.CanReserve( thing, 1 ) )
                )
                {
                    return false;
                }
                var synthesizer = thing as Building_AutomatedFactory;
                if( synthesizer != null )
                {
                    var products = synthesizer.AllProductionReadyRecipes( FoodSynthesis.IsDrug, FoodSynthesis.SortJoy );
                    foreach( var product in products )
                    {
                        if( DrugIsUsableBy( synthesizer, product, chemical, overdose, pawn ) )
                        {
                            if( synthesizer.ConsiderFor( product, pawn ) )
                            {
#if DEVELOPER
                                CCL_Log.Message(
                                    string.Format( "{0} is considering {1} for {2}", pawn.LabelShort, synthesizer.ThingID, product.defName ),
                                    "Detour.JobGiver_BingDrug.BestIngestTarget"
                                );
#endif
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else if( thing.def.IsDrug )
                {
                    return DrugIsUsableBy( thing, thing.def, chemical, overdose, pawn );
                }
                return false;
            },
                null,
                -1,
                false
            );
            if(
                ( ingestibleThing != null )&&
                ( ingestibleThing is Building_AutomatedFactory )
            )
            {
                var synthesizer = ingestibleThing as Building_AutomatedFactory;
                if(
                    ( !synthesizer.IsConsidering( pawn ) )||
                    ( !synthesizer.ReserveForUseBy( pawn, synthesizer.ConsideredProduct ) )
                )
                {   // Couldn't reserve the synthesizer for production
                    return null;
                }
            }
            return ingestibleThing;
        }

        #endregion

    }

}
