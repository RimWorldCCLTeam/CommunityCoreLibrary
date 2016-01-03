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

            public static class ThingComps
            {

                public static bool          Inject()
                {
                    var ModHelperDefs = Controller.Data.ModHelperDefs;
                    foreach( var ModHelperDef in ModHelperDefs )
                    {
                        if( !ModHelperDef.ThingCompsInjected )
                        {
                            // TODO:  Alpha 13 API change
                            //if( ModHelperDef.InjectThingComps() )

                            ModHelperDef.InjectThingComps();
                            if( !ModHelperDef.ThingCompsInjected )
                            {
#if DEBUG
                                if( ModHelperDef.Verbosity >= Verbosity.NonFatalErrors )
                                {
                                    CCL_Log.Error( "Error injecting ThingComps", ModHelperDef.ModName );
                                }
#endif
                                return false;
                            }
#if DEBUG
                            else if( ModHelperDef.Verbosity >= Verbosity.Injections )
                            {
                                CCL_Log.Message( "Injected ThingComps", ModHelperDef.ModName );
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
