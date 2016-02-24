using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Resources
    {

        public static bool                  Enable()
        {
            var ModHelperDefs = Controller.Data.ModHelperDefs;
            foreach( var ModHelperDef in ModHelperDefs )
            {
                // Generic hoppers
                if( ModHelperDef.UsesGenericHoppers )
                {
                    if( !Resources.Buildings.Hoppers.EnableGenericHoppers() )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Error enabling generic hoppers"
                        );
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Generic Hoppers Enabled"
                    );
#endif
                }

                // Vanilla hoppers
                if( ModHelperDef.HideVanillaHoppers )
                {
                    if( !Resources.Buildings.Hoppers.DisableVanillaHoppers() )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Error disabling vanilla hoppers"
                        );
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Vanilla Hoppers Disabled"
                    );
#endif
                }
            }
            return true;
        }

    }

}
