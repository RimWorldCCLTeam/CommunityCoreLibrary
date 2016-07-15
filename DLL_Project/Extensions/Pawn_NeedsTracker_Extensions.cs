using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Pawn_NeedsTracker_Extensions
    {

        private static FieldInfo       _pawn;

        public static Pawn            pawn( this Pawn_NeedsTracker obj )
        {
            if( _pawn == null )
            {
                _pawn = typeof( Pawn_NeedsTracker ).GetField( "pawn", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (Pawn) _pawn.GetValue( obj );
        }

    }

}