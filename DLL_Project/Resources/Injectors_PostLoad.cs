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

            public static class PostLoad
            {
                
                public static bool          Inject()
                {
                    var ModHelperDefs = Controller.Data.ModHelperDefs;
                    foreach( var ModHelperDef in ModHelperDefs )
                    {
                        if( !ModHelperDef.PostLoadersInjected )
                        {
                            // TODO:  Alpha 13 API change
                            //if( ModHelperDef.InjectPostLoaders() )

                            ModHelperDef.InjectPostLoaders();
                            if( !ModHelperDef.PostLoadersInjected )
                            {
                                CCL_Log.TraceMod(
                                    ModHelperDef,
                                    Verbosity.NonFatalErrors,
                                    "Error injecting Post Loaders"
                                );
                                return false;
                            }
#if DEBUG
                            CCL_Log.TraceMod(
                                ModHelperDef,
                                Verbosity.Injections,
                                "Post Loaders injected"
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
