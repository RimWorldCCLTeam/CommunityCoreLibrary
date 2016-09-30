using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ListerThings_Extensions
    {

        private static FieldInfo            _listsByGroup;

        static                              ListerThings_Extensions()
        {
            _listsByGroup = typeof( ListerThings ).GetField( "listsByGroup", Controller.Data.UniversalBindingFlags );
            if( _listsByGroup == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'listsByGroup' in 'ListerThings'",
                    "ListerThings_Extensions" );
            }
        }

        public static List<Thing>           ListsByGroup( this ThingRequestGroup group )
        {
            var listsByGroup = _listsByGroup.GetValue( Find.ListerThings ) as List<Thing>[];
            return listsByGroup[ (int) group ];
        }

    }

}
