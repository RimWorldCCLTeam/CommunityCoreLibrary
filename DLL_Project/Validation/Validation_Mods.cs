using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Validation
    {

        public static class Mods
        {

            public static bool              Validate()
            {
                // Hopefully...
                var rVal = true;

                var ModHelperDefs = Controller.Data.ModHelperDefs;
                for( int i = 0; i < ModHelperDefs.Count; i++ ){
                    var ModHelperDef = ModHelperDefs[ i ];
                    if( !ModHelperDef.IsValid )
                    {
                        // Don't do anything special with broken mods
                        ModHelperDefs.Remove( ModHelperDef );
                        rVal = false;
                        continue;
                    }
                }
                // Return true if all mods OK, false if any failed validation
                return rVal;
            }

        }

    }

}
