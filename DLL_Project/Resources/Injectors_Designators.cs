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

            public static class Designators
            {
                
                public static bool          Inject()
                {
                    var ModHelperDefs = Controller.Data.ModHelperDefs;
                    // Inject the designators into the categories
                    foreach( var ModHelperDef in ModHelperDefs )
                    {
                        if( !ModHelperDef.DesignatorsInjected )
                        {
                            // TODO:  Alpha 13 API change
                            //if( ModHelperDef.InjectDesignators() )

                            ModHelperDef.InjectDesignators();
                            if( ModHelperDef.DesignatorsInjected )
                            {
                                CCL_Log.Message( "Injected Designators", ModHelperDef.ModName );
                            }
                            else
                            {
                                CCL_Log.Error( "Error injecting Designators", ModHelperDef.ModName );
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
