using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Pawn_ApparelTracker_Extensions
    {

        private static FieldInfo            _wornApparel;

        static                              Pawn_ApparelTracker_Extensions()
        {
            _wornApparel = typeof( Pawn_ApparelTracker ).GetField( "wornApparel", Controller.Data.UniversalBindingFlags );
            if( _wornApparel == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'wornApparel' in 'Pawn_ApparelTracker'",
                    "Pawn_ApparelTracker_Extensions" );
            }
        }

        public static List<Apparel>         wornApparel( this Pawn_ApparelTracker obj )
        {
            return (List<Apparel>) _wornApparel.GetValue( obj );
        }

    }

}