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
                            if( ModHelperDef.PostLoadersInjected )
                            {
                                CCL_Log.Message( "Injected Post Loaders", ModHelperDef.ModName );
                            }
                            else
                            {
                                CCL_Log.Error( "Error in Post Load Injections", ModHelperDef.ModName );
                                return false;
                            }
                        }
                    }
                    return true;
                }

            }

        }

    }

}
