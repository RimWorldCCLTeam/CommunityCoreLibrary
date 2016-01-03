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

                // Validate one ModHelperDef per mod
                var allMods = LoadedModManager.LoadedMods.ToList();
                for( int i = 0; i < allMods.Count; i++ )
                {
                    var mod = allMods[ i ];
                    var modHelperDefs = Find_Extensions.DefListOfModOfType<ModHelperDef>( mod );
                    if(
                        ( !modHelperDefs.NullOrEmpty() )&&
                        ( modHelperDefs.Count > 1 )
                    )
                    {
                        CCL_Log.Error( "Multiple ModHelperDefs detected!", mod.name );
                        rVal = false;
                    }
                }

                if( rVal == true )
                {
                    var ModHelperDefs = Controller.Data.ModHelperDefs;
                    for( int i = 0; i < ModHelperDefs.Count; i++ )
                    {
                        var modHelperDef = ModHelperDefs[ i ];
                        if( !modHelperDef.IsValid )
                        {
                            // Don't do anything special with broken mods
                            ModHelperDefs.Remove( modHelperDef );
                            rVal = false;
                            continue;
                        }
#if DEBUG
                        else if( modHelperDef.Verbosity >= Verbosity.Injections )
                        {
                            CCL_Log.Message( "ModHelperDef valid for CCL v" + modHelperDef.version, modHelperDef.ModName );
                        }
#endif
                    }
                }

                // Return true if all mods OK, false if any failed validation
                return rVal;
            }

        }

    }

}
