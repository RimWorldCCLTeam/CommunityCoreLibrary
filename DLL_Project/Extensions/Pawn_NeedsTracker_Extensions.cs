using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Pawn_NeedsTracker_Extensions
    {

        private static FieldInfo        _pawn;

        static                          Pawn_NeedsTracker_Extensions()
        {
            _pawn = typeof( Pawn_NeedsTracker ).GetField( "pawn", Controller.Data.UniversalBindingFlags );
            if( _pawn == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'drawer' in class 'Pawn'",
                    "Pawn_NeedsTracker_Extensions");
            }
        }

        public static Pawn              pawn( this Pawn_NeedsTracker obj )
        {
            return (Pawn) _pawn.GetValue( obj );
        }

    }

}