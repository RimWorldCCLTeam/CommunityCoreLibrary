using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Alert_PasteDispenserNeedsHopper : RimWorld.Alert_PasteDispenserNeedsHopper
    {
        
        public override AlertReport Report
        {
            get
            {
                var dispensers = Find.ListerBuildings.allBuildingsColonist.Where( b => (
                    ( b.def.thingClass == typeof( Building_NutrientPasteDispenser ) )||
                    ( b.def.thingClass.IsSubclassOf( typeof( Building_NutrientPasteDispenser ) ) )
                ) ).ToList();

                foreach( var dispenser in dispensers )
                {
                    var showAlert = true;
                    var CompHopperUser = dispenser.TryGetComp<CompHopperUser>();
                    if( CompHopperUser != null )
                    {
                        var hoppers = CompHopperUser.FindHoppers();
                        if( !hoppers.NullOrEmpty() )
                        {
                            showAlert = false;
                        }
                    }
                    if( showAlert )
                    {
                        foreach( IntVec3 c in GenAdj.CellsAdjacentCardinal( dispenser ) )
                        {
                            Thing thing = (Thing) GridsUtility.GetEdifice( c );
                            if(
                                ( thing != null )&&
                                ( thing.def == ThingDefOf.Hopper )
                            )
                            {
                                showAlert = false;
                                break;
                            }
                        }
                    }
                    if( showAlert )
                    {
                        return AlertReport.CulpritIs( dispenser );
                    }
                }
                return AlertReport.Inactive;
            }
        }

    }

}
