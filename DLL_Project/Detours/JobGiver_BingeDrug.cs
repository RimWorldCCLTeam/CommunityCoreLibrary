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

        /* TODO:  Investigate and expand drug system to use factories
        
        internal static bool                        IsValidDrugFor( Thing drugSource, ThingDef drugDef, ChemicalDef chemical, Hediff overdose, Pawn pawn )
        {
            var props = drugDef.GetCompProperty<CompProperties_Drug>();
            if( props == null )
            {
                return false;
            }
            return(
                ( props.chemical == chemical )&&
                (
                    ( drugSource is Building_AutomatedFactory )||
                    ( drugSource.def == drugDef )
                )&&
                (
                    ( overdose == null )||
                    ( !props.CanCauseOverdose )||
                    ( (double) overdose.Severity + (double) props.overdoseSeverityOffset.max < 0.78600001335144 )
                )&&
                (
                    ( pawn.Position.InHorDistOf( drugSource.Position, 60f ) )||
                    ( drugSource.Position.Roofed() )||
                    (
                        ( Find.AreaHome[ drugSource.Position ] )||
                        ( drugSource.Position.GetSlotGroup() != null )
                    )
                )
            );
        }

        [DetourClassMethod( typeof( JobGiver_BingeDrug ), "BestIngestTarget" )]
        protected override Thing                    BestIngestTarget( Pawn pawn )
        {
            var chemical = ((MentalState_BingingDrug) pawn.MentalState).chemical;
            var overdose = pawn.health.hediffSet.GetFirstHediffOfDef( HediffDefOf.DrugOverdose );
            var ingestibleThing = GenClosest.ClosestThingReachable(
                pawn.Position,
                ThingRequest.ForGroup( ThingRequestGroup.Drug ),
                PathEndMode.OnCell,
                TraverseParms.For(
                    pawn,
                    pawn.NormalMaxDanger(),
                    TraverseMode.ByPawn,
                    false ),
                9999f,
                (thing) =>
            {
                if(
                    ( !this.IgnoreForbid( pawn ) )&&
                    ( thing.IsForbidden( pawn) )||
                    ( !pawn.CanReserve( thing, 1 ) )
                )
                {
                    return false;
                }
                var factory = thing as Building_AutomatedFactory;
                if( factory != null )
                {
                    var products = factory.AllProducts();
                    foreach( var product in products )
                    {
                        if(
                            ( product.IsDrug )&&
                            ( factory.CanProduce( product ) )
                        )
                        {
                            if( IsValidDrugFor( factory, product, chemical, overdose, pawn ) )
                            {
                                // Set flag to let FoodUtility.GetFinalIngestibleDef() know we are looking for drugs, not food
                                _FoodUtility._GetSynthesizedDrug = true;
                                _FoodUtility._GetSpecificSynthesizedProduct = product;
                                return true;
                            }
                        }
                    }

                }
                else if( thing.def.IsDrug )
                {
                    return IsValidDrugFor( thing, thing.def, chemical, overdose, pawn );
                }
                return false;
            },
                null,
                -1,
                false
            );
            return ingestibleThing;
        }

        */

    }

}
