using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class Pawn_Extensions
    {
        
        internal static FieldInfo       _GetPawnDrawTracker;

        internal static bool            CanBingeOn( this Pawn pawn, ChemicalDef chemicalDef, DrugCategory drugCategory )
        {
            return(
                ( AddictionUtility.CanBingeOnNow( pawn, chemicalDef, drugCategory ) )||
                (
                    ( drugCategory == DrugCategory.Hard )&&
                    ( AddictionUtility.CanBingeOnNow( pawn, chemicalDef, DrugCategory.Social ) )
                )
            );
        }

        public static Pawn_DrawTracker  GetPawnDrawTracker( this Pawn pawn )
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
