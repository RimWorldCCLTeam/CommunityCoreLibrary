using System;
using System.Linq;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal class _MentalStateWorker_BingingDrug : MentalStateWorker_BingingDrug
    {

        // Enable checking Building_AutomatedFactory for drug production
        [DetourClassMethod( typeof( MentalStateWorker_BingingDrug ), "StateCanOccur" ) ]
        public override bool StateCanOccur( Pawn pawn )
        {
            if( base.StateCanOccur( pawn ) )
            {
                return false;
            }
            var chemicalDefs = DefDatabase<ChemicalDef>.AllDefsListForReading;
            for( int index = 0; index < chemicalDefs.Count; ++index )
            {
                if( pawn.CanBingeOn( chemicalDefs[ index ], this.def.drugCategory ) )
                {
                    return true;
                }
            }
            var synthesizers = Find.ListerBuildings
                                   .AllBuildingsColonistOfClass<Building_AutomatedFactory>()
                                   .Where( building => (
                                       ( building.OutputToPawnsDirectly )&&
                                       ( building.BestProduct( FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol ) != null )&&
                                       ( pawn.CanReserveAndReach( building, PathEndMode.InteractionCell, pawn.NormalMaxDanger(), 1 ) )
                                    ) );
            if( synthesizers.Count() > 0 )
            {
                foreach( var synthesizer in synthesizers )
                {
                    var thingDef = synthesizer.BestProduct( FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol );
                    var drugComp = thingDef.GetCompProperty<CompProperties_Drug>();
                    if( drugComp == null )
                    {
                        continue;
                    }
                    var chemicalDef = drugComp.chemical;
                    if( chemicalDef == null )
                    {
                        continue;
                    }
                    if( pawn.CanBingeOn( chemicalDef, this.def.drugCategory ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }

}
