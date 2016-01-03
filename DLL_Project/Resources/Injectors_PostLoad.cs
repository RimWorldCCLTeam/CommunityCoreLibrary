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
#if DEBUG
                                if( ModHelperDef.Verbosity >= Verbosity.NonFatalErrors )
                                {
                                    CCL_Log.Error( "Error in Post Load Injections", ModHelperDef.ModName );
                                }
#endif
                                return false;
                            }
#if DEBUG
                            else if( ModHelperDef.Verbosity >= Verbosity.Injections )
                            {
                                CCL_Log.Message( "Injected Post Loaders", ModHelperDef.ModName );
                            }
#endif
                        }
                    }
                    return true;
                }

            }

        }

    }

}
