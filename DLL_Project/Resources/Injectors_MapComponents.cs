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
#if DEBUG
                                if( ModHelperDef.Verbosity >= Verbosity.NonFatalErrors )
                                {
                                    CCL_Log.Error( "Error injecting MapComponents", ModHelperDef.ModName );
                                }
#endif
                                return false;
                            }
#if DEBUG
                            else if( ModHelperDef.Verbosity >= Verbosity.Injections )
                            {
                                CCL_Log.Message( "Injected MapComponents", ModHelperDef.ModName );
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
