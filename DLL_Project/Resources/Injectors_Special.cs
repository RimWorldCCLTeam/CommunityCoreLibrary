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

            public static class Special
            {

                public static bool          Inject()
                {
                    var ModHelperDefs = Controller.Data.ModHelperDefs;
                    foreach( var ModHelperDef in ModHelperDefs )
                    {
                        if( !ModHelperDef.SpecialsInjected )
                        {
                            // TODO:  Alpha 13 API change
                            //if( ModHelperDef.InjectSpecials() )

                            ModHelperDef.InjectSpecials();
                            if( !ModHelperDef.SpecialsInjected )
                            {
                                CCL_Log.TraceMod(
                                    ModHelperDef,
                                    Verbosity.NonFatalErrors,
                                    "Error injecting Special Injectors"
                                );
                                return false;
                            }
#if DEBUG
                            CCL_Log.TraceMod(
                                ModHelperDef,
                                Verbosity.Injections,
                                "Special Injectors injected"
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
