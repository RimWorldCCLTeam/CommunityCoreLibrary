using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class CompFacility_Extensions
    {

        private static FieldInfo            _linkedBuildings;

        static                              CompFacility_Extensions()
        {
            _linkedBuildings = typeof( CompFacility ).GetField( "linkedBuildings", Controller.Data.UniversalBindingFlags );
            if( _linkedBuildings == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'linkedBuildings' in 'CompFacility'",
                    "CompFacility_Extensions" );
            }
        }

        public static List<Thing>           LinkedBuildings( this CompFacility compFacility )
        {
            return (List<Thing>) _linkedBuildings.GetValue( compFacility );
        }

    }
}
