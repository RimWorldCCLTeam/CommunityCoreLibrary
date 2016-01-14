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
                    if( !Resources.Buildings.Hoppers.Enable() )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Error enabling Hoppers"
                        );
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Hoppers Enabled"
                    );
#endif
                }
            }
            return true;
        }

    }

}
