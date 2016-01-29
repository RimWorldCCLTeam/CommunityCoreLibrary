using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Validation
    {

        public static class Research
        {

            public static bool              Validate()
            {
                // Hopefully...
                var rVal = true;

                var AdvancedResearchDefs = Controller.Data.AdvancedResearchDefs;

                // Make sure the hidden research exists
                if( CommunityCoreLibrary.Research.Locker == null )
                {
                    CCL_Log.Error( "Missing research locker!", "Advanced Research" );
                    rVal = false;
                }

                // Validate each advanced research def
                for( int i = 0; i < AdvancedResearchDefs.Count; i++ ){
                    var advancedResearchDef = AdvancedResearchDefs[ i ];

                    if( !advancedResearchDef.IsValid )
                    {
                        // Remove projects with errors from list of usable projects
                        AdvancedResearchDefs.Remove( advancedResearchDef );
                        i--;
                        rVal = false;
                        continue;
                    }

                    if( advancedResearchDef.IsLockedOut() )
                    {
                        // Remove locked out projects
                        AdvancedResearchDefs.Remove( advancedResearchDef );
                        i--;
                        continue;
                    }

                }

#if DEBUG
                if( rVal == true )
                {
                    var allMods = Controller.Data.Mods;
                    foreach( var mod in allMods )
                    {
                        if( !Find_Extensions.DefListOfTypeForMod<AdvancedResearchDef>( mod ).NullOrEmpty() )
                        {
                            CCL_Log.TraceMod(
                                mod,
                                Verbosity.Validation,
                                "Passed validation",
                                "Advanced Research"
                            );
                        }
                    }
                }
#endif
                
                // Return true if no errors, false otherwise
                return rVal;
            }

        }

    }

}
