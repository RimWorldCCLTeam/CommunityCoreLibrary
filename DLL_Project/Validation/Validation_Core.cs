using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Validation
    {
        
        public static class Core
        {
            
            public static bool              Validate()
            {

                var allMods = Controller.Data.Mods;

                bool found = false;
                int index = -1;
                for( int i = 0; i < allMods.Count; ++i )
                {
                    LoadedMod mod = allMods[ i ];
                    if( mod.name == Controller.Data.UnityObjectName )
                    {
                        found = true;
                        index = i;
                        break;
                    }
                }
                if( !found )
                {
                    CCL_Log.Error(
                        "Unable to find mod 'Community Core Library' in mod load order!",
                        "Library Validation"
                    );
                    return false;
                }
                else if( index != 1 )
                {
                    CCL_Log.Error(
                        "'Community Core Library' must be second in mod load order, immediately after 'Core'! :: Current position is #" + ( index + 1 ).ToString(),
                        "Library Validation"
                    );
                    return false;
                }

                LoadedMod CCL_Mod = Controller.Data.Mods[ index ];
                ModHelperDef CCL_HelperDef = Find_Extensions.ModHelperDefForMod( CCL_Mod );

                var vc = Version.Compare( CCL_HelperDef.version );
                if( vc != Version.VersionCompare.ExactMatch )
                {
                    CCL_Log.Error(
                        "ModHelperDef version mismatch for Community Core Library!",
                        "Library Validation"
                    );
                    return false;
                }

                // CCL rank is #2 in load order and def version matches library
                Controller.Data.cclMod = CCL_Mod;
                Controller.Data.cclHelperDef = CCL_HelperDef;

                return true;

            }

        }

    }

}
