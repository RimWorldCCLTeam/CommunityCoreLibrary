using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{
    
    public static class Pawn_Extensions
    {
        
        internal static FieldInfo _GetPawnDrawTracker;

        public static Pawn_DrawTracker GetPawnDrawTracker( this Pawn pawn )
        {
            if( _GetPawnDrawTracker == null )
            {
                _GetPawnDrawTracker = typeof( Pawn ).GetField( "drawer", BindingFlags.Instance | BindingFlags.NonPublic );
                if( _GetPawnDrawTracker == null )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Unable to get 'drawer' in class 'Pawn'",
                        "CommunityCoreLibrary.Detour.JobDriver_SocialRelax");
                    return null;
                }
            }
            return (Pawn_DrawTracker)_GetPawnDrawTracker.GetValue( pawn );
        }

    }

}
