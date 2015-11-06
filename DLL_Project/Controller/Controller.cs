using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public static class Data
    {

        #region Instance Data

        public static readonly string       UnityObjectName = "Community Core Library";
        public static GameObject            UnityObject;

        private static List< ModHelperDef > modHelperDefs;
        private static List< AdvancedResearchDef > advancedResearchDefs;

        #endregion

        #region Static Properties

        // All controllers will use these lists when working on the global set.
        // These lists are properly maintained for the game state based on varing
        // factors.  The DefDatabase should only be used as a list of all potential
        // defs where as these lists are the working set used by CCL.

        public static List< ModHelperDef >  ModHelperDefs
        {
            get
            {
                if( modHelperDefs == null )
                {
                    // Get the initial raw set of mods
                    modHelperDefs = DefDatabase< ModHelperDef >.AllDefs.ToList();
                }
                return modHelperDefs;
            }
        }

        public static List< AdvancedResearchDef > AdvancedResearchDefs
        {
            get
            {
                if( advancedResearchDefs == null )
                {
                    // Get the initial ordered raw set of advanced research
                    advancedResearchDefs = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();
                }
                return advancedResearchDefs;
            }
        }

        #endregion

    }

}
