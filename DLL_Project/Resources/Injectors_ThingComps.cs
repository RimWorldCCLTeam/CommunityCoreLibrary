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
                            if( ModHelperDef.ThingCompsInjected )
                            {
                                CCL_Log.Message( "Injected ThingComps", ModHelperDef.ModName );
                            }
                            else
                            {
                                CCL_Log.Error( "Error injecting ThingComps", ModHelperDef.ModName );
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
