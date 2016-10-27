using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _AddictionUtility
    {

        #region Detoured Methods

        [DetourClassMethod( typeof( AddictionUtility ), "CanBingeOnNow" )]
        internal static bool                        _CanBingeOnNow( Pawn pawn, ChemicalDef chemical, DrugCategory drugCategory )
        {
            if( !chemical.canBinge )
            {
                return false;
            }
            // Check all spawned drugs
            var listOfSpawnedDrugs = Find.ListerThings.ThingsInGroup( ThingRequestGroup.Drug );
            for( int index = 0; index < listOfSpawnedDrugs.Count; ++index )
            {
                if(
                    ( !listOfSpawnedDrugs[ index ].Position.Fogged() )&&
                    (
                        ( drugCategory == DrugCategory.Any )||
                        ( listOfSpawnedDrugs[ index ].def.ingestible.drugCategory == drugCategory )
                    )
                )
                {
                    if( listOfSpawnedDrugs[ index ].TryGetComp<CompDrug>().Props.chemical == chemical )
                    {
                        return true;
                    }
                    // Core empty check???
                    /*
                    if(
                        ( list[ index ].Position.Roofed() )||
                        ( list[ index ].Position.InHorDistOf( pawn.Position, 45f ) )
                    )
                    {
                    }
                    */
                }
            }
            // Check synthesizers for drug production
            /* TODO:  Investigate and expand drug system to use factories
            var listOfDrugSynthesizers = Find.ListerBuildings.AllBuildingsColonistOfClass<Building_AutomatedFactory>().Where( building => (
                ( building.BestProduct( FoodSynthesis.IsDrug, FoodSynthesis.SortDrug ) != null )
            ) ).ToList();
            if( !listOfDrugSynthesizers.NullOrEmpty() )
            {
                foreach( var synthesizer in listOfDrugSynthesizers )
                {
                    var products = synthesizer.AllProducts();
                    foreach( var product in products )
                    {
                        if(
                            ( product.IsDrug )&&
                            ( synthesizer.CanProduce( product ) )&&
                            (
                                ( drugCategory == DrugCategory.Any )||
                                ( product.ingestible.drugCategory == drugCategory )
                            )
                        )
                        {
                            return true;
                        }
                    }
                }
            }
            */
            return false;
        }

        #endregion

    }
}

