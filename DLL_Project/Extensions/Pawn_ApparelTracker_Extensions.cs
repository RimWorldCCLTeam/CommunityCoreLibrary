using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Pawn_ApparelTracker_Extensions
    {

        private static FieldInfo _wornApparel;

        public static List<Apparel> wornApparel( this Pawn_ApparelTracker obj )
        {
            if( _wornApparel == null )
            {
                _wornApparel = typeof( Pawn_ApparelTracker ).GetField( "wornApparel", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (List<Apparel>) _wornApparel.GetValue( obj );
        }

    }

}