using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Resources
    {

        public static partial class Buildings
        {
            
            public static class Hoppers
            {
                private static bool         enabled = false;

                public static bool          Enable()
                {
                    if( enabled )
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
                        hopper.researchPrerequisite = null;
                    }

                    enabled = true;
                    return true;
                }

            }

        }

    }

}
