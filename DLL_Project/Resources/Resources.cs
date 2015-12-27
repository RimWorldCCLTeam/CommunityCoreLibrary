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
                        CCL_Log.Error( "Unable to enable hoppers", ModHelperDef.ModName );
                        return false;
                    }
                    CCL_Log.Message( "Enabling hoppers", ModHelperDef.ModName );
                }
            }
            return true;
        }

    }

}
