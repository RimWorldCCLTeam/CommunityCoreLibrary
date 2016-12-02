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
        private static FieldInfo            _listsByDef;

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
            _listsByDef = typeof( ListerThings ).GetField( "listsByDef", Controller.Data.UniversalBindingFlags );
            if( _listsByDef == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'listsByDef' in 'ListerThings'",
                    "ListerThings_Extensions" );
            }
        }

        public static List<Thing>[]         ListsByGroup( this ListerThings listerThings )
        {
            return _listsByGroup.GetValue( listerThings ) as List<Thing>[];
        }

        public static Dictionary<ThingDef,List<Thing>> ListsByDef( this ListerThings listerThings )
        {
            return _listsByDef.GetValue( listerThings ) as Dictionary<ThingDef,List<Thing>>;
        }

    }

}
