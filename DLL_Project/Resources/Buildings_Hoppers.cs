using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public static partial class Resources
    {

        public static partial class Buildings
        {
            
            public static class Hoppers
            {
                
                public static bool          EnableGenericHoppers()
                {
                    if( Controller.Data.GenericHoppersEnabled )
                    {
                        // Only enable them once
                        return true;
                    }

                    var hoppers = DefDatabase<ThingDef>.AllDefs.Where( d => (
                        ( d.thingClass == typeof( Building_Hopper ) )&&
                        ( d.HasComp( typeof( CompHopper ) ) )
                    ) ).ToList();

                    foreach( var hopper in hoppers )
                    {
                        if( hopper.researchPrerequisite == Research.Locker )
                        {
                            // Only change the prerequisite if it's using the default locker
                            hopper.researchPrerequisite = null;
                        }
                    }

                    // Flag it as done
                    Controller.Data.GenericHoppersEnabled = true;
                    return true;
                }

                public static bool          DisableVanillaHoppers()
                {
                    if( Controller.Data.VanillaHoppersDisabled )
                    {
                        // Only disable them once
                        return true;
                    }

                    // This will hide it "normally"
                    ThingDefOf.Hopper.researchPrerequisite = Research.Locker;

                    // This will hide it in god mode
                    var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( ThingDefOf.Hopper.designationCategory, true );
                    var designator = designationCategory.resolvedDesignators.Find( d => (
                        ( d is Designator_Build )&&
                        ( ((Designator_Build)d).PlacingDef == ThingDefOf.Hopper )
                    ) );
                    if( designator != null )
                    {
                        designationCategory.resolvedDesignators.Remove( designator );
                    }
                    ThingDefOf.Hopper.menuHidden = true;
                    ThingDefOf.Hopper.designationCategory = "None";

                    // Flag it as done
                    Controller.Data.VanillaHoppersDisabled = true;
                    return true;
                }

            }

        }

    }

}
