using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Resources
    {

        public static partial class Injectors
        {

            public static class MapComponents
            {
                
                public static bool          Inject()
                {
                    var ModHelperDefs = Controller.Data.ModHelperDefs;
                    // Inject the map components into the game
                    foreach( var ModHelperDef in ModHelperDefs )
                    {
                        if( !ModHelperDef.MapComponentsInjected )
                        {
                            // TODO:  Alpha 13 API change
                            //if( ModHelperDef.InjectMapComponents() )

                            ModHelperDef.InjectMapComponents();
                            if( !ModHelperDef.MapComponentsInjected )
                            {
                                CCL_Log.TraceMod(
                                    ModHelperDef,
                                    Verbosity.NonFatalErrors,
                                    "Error injecting MapComponents"
                                );
                                return false;
                            }
#if DEBUG
                            CCL_Log.TraceMod(
                                ModHelperDef,
                                Verbosity.Injections,
                                "MapComponents injected"
                            );
#endif
                        }
                    }
                    return true;
                }
            }

        }

    }

}
