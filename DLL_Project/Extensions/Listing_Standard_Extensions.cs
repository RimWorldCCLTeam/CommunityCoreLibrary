using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Listing_Standard_Extensions
    {

        private static FieldInfo            _curX;

        public static float                 Indentation( this Listing_Standard obj )
        {
            if( _curX == null )
            {
                _curX = typeof( Listing_Standard ).GetField( "curX", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (float)_curX.GetValue( obj );
        }

        public static void                  Indent( this Listing_Standard listing, float distance = 24f )
        {
            if( _curX == null )
            {
                _curX = typeof( Listing_Standard ).GetField( "curX", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            var curX = (float)_curX.GetValue( listing );
            var newX = curX + distance;
            var curW = listing.ColumnWidth();
            var newW = curW - distance;
            _curX.SetValue( listing, newX );
            // A14 - OverrideColumnWidth => ColumnWidth?
            listing.ColumnWidth = newW;
        }

        public static void                  Undent( this Listing_Standard obj, float distance = 24f )
        {
            Indent( obj, -distance );
        }

    }

}
