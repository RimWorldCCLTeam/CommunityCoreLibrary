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
#if DEBUG
                        if( ModHelperDef.Verbosity >= Verbosity.NonFatalErrors )
                        {
                            CCL_Log.Error( "Unable to enable hoppers", ModHelperDef.ModName );
                        }
#endif
                        return false;
                    }
#if DEBUG
                    else if( ModHelperDef.Verbosity >= Verbosity.Injections )
                    {
                        CCL_Log.Message( "Enabling hoppers", ModHelperDef.ModName );
                    }
#endif
                }
            }
            return true;
        }

    }

}
