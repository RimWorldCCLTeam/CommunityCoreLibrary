using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public static class DrugUtility
    {
        
        internal static bool                        TryFindJoyDrug( IntVec3 center, Pawn ingester, float maxDistance, bool nurseable, List<ThingDef> drugDefs, out Thing ingestible, out ThingDef ingestibleDef )
        {
            ingestible = null;
            ingestibleDef = null;
            if(
                (
                    ( ingester.story != null )&&
                    ( ingester.story.traits.DegreeOfTrait( TraitDefOf.DrugDesire ) < 0 )
                )||
                ( ingester.drugs == null )
            )
            {
                return false;
            }
            drugDefs.Clear();
            var currentPolicy = ingester.drugs.CurrentPolicy;
            for( int index = 0; index < currentPolicy.Count; index++ )
            {
                if( currentPolicy[ index ].allowedForJoy )
                {
                    if(
                        ( !nurseable )||
                        (
                            ( nurseable )&&
                            ( currentPolicy[ index ].drug.ingestible.nurseable )
                        )
                    )
                    {
                        drugDefs.Add( currentPolicy[ index ].drug );
                    }
                }
            }
            drugDefs.Shuffle();

            // Get a list of all synthesizers which can produce any of the drugs
            var synthesizers = ingester.Map.listerBuildings.AllBuildingsColonistOfClass<Building_AutomatedFactory>().Where( factory => (
                ( factory.OutputToPawnsDirectly )&&
                ( !factory.IsReserved )&&
                ( factory.AllProductionReadyRecipes().Any( product => drugDefs.Contains( product ) ) )
            ) ).ToList().ConvertAll( synthesizer => (Thing)synthesizer );

            for( int index = 0; index < drugDefs.Count; ++index )
            {
                var currentDrug = drugDefs[ index ];

                // Get all things that are this drug
                var listOfDrugs = ingester.Map.listerThings.ThingsOfDef( currentDrug );

                // Add all synthesizers that can produce this drug
                listOfDrugs.AddRange( synthesizers.Where( synthesizer => ((Building_AutomatedFactory)synthesizer).CanDispenseNow( currentDrug ) ).ToList() );

                // Now find the closest drug/synthesizer
                if( listOfDrugs.Count > 0 )
                {
                    ingestible = GenClosest.ClosestThing_Global_Reachable(
                        center,
                        ingester.Map,
                        listOfDrugs,
                        PathEndMode.InteractionCell,
                        TraverseParms.For(
                            ingester,
                            Danger.Deadly,
                            TraverseMode.ByPawn,
                            false ),
                        maxDistance,
                        (drug) =>
                    {
                        return(
                            ( !drug.IsForbidden( ingester ) )&&
                            ( ingester.CanReserve( drug, 1 ) )
                        );
                    },
                        null );
                    if( ingestible != null )
                    {
                        var synthesizer = ingestible as Building_AutomatedFactory;
                        if( synthesizer != null )
                        {
                            if( !synthesizer.ConsiderFor( currentDrug, ingester ) )
                            {   // Could not consider for pawn
                                return false;
                            }
                        }
                        ingestibleDef = currentDrug;
                        return true;
                    }
                }
            }
            return false;
        }

    }

}
