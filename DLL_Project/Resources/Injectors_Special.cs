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
#if DEBUG
                                if( ModHelperDef.Verbosity >= Verbosity.NonFatalErrors )
                                {
                                    CCL_Log.Error( "Error in Special Injections", ModHelperDef.ModName );
                                }
#endif
                                return false;
                            }
#if DEBUG
                            else if( ModHelperDef.Verbosity >= Verbosity.Injections )
                            {
                                CCL_Log.Message( "Injected Specials", ModHelperDef.ModName );
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
