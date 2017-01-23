using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _AddictionUtility
    {

        #region Helper Methods

        private static bool                 CanBingeOnNowInt( ThingDef thingDef, ChemicalDef chemical, DrugCategory drugCategory )
        {
            if(
                ( thingDef == null )||
                ( thingDef.ingestible == null )
            )
            {
                return false;
            }
            if(
                ( drugCategory == DrugCategory.Any )||
                ( thingDef.ingestible.drugCategory == drugCategory )
            )
            {
                var drugProps = thingDef.GetCompProperty<CompProperties_Drug>();
                if( drugProps != null )
                {
                    return( drugProps.chemical == chemical );
                    // Core empty check???
                    /*
                     * 
                    if ( 
                         ( list[ i ].Position.Roofed( list[ i ].Map )||
                         ( !list[ i ].Position.InHorDistOf( pawn.Position, 45f ) )
                    )
                    {
                    }
                    */
                }
            }
            return false;
        }

        #endregion

        #region Detoured Methods

        [DetourMember( typeof( AddictionUtility ) )]
        internal static bool                _CanBingeOnNow( Pawn pawn, ChemicalDef chemical, DrugCategory drugCategory )
        {
            if(
                ( !chemical.canBinge )||
                ( !pawn.Spawned )
            )
            {
                return false;
            }
            // Check all spawned drugs
            var listOfSpawnedDrugs = pawn.Map.listerThings.ThingsInGroup( ThingRequestGroup.Drug );
            for( int index = 0; index < listOfSpawnedDrugs.Count; ++index )
            {
                var thing = listOfSpawnedDrugs[ index ];
                if( !thing.Position.Fogged( thing.Map ) )
                {
                    var synthesizer = thing as Building_AutomatedFactory;
                    if( synthesizer != null )
                    {
                        var products = synthesizer.AllProductionReadyRecipes();
                        if( products.NullOrEmpty() )
                        {   // Nothing ready for production
                            return false;
                        }
                        foreach( var product in products )
                        {
                            if( product.IsDrug )
                            {
                                if( CanBingeOnNowInt( product, chemical, drugCategory ) )
                                {
                                    return true;
                                }
                            }
                        }
                        // No drugs or none that can be binged on can be produced
                        return false;
                    }
                    if( CanBingeOnNowInt( thing.def, chemical, drugCategory ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

    }
}

