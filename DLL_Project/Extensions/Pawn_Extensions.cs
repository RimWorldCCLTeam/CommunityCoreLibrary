using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class Pawn_Extensions
    {
        
        internal static FieldInfo       _GetPawnDrawTracker;

        static                          Pawn_Extensions()
        {
            _GetPawnDrawTracker = typeof( Pawn ).GetField( "drawer", Controller.Data.UniversalBindingFlags );
            if( _GetPawnDrawTracker == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'drawer' in class 'Pawn'",
                    "Pawn_Extensions");
            }
        }

        public static Pawn_DrawTracker  GetPawnDrawTracker( this Pawn pawn )
        {
            return (Pawn_DrawTracker)_GetPawnDrawTracker.GetValue( pawn );
        }

    }

}
