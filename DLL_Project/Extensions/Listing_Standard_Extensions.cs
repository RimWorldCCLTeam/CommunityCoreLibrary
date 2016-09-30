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

        static                              Listing_Standard_Extensions()
        {
            _curX = typeof( Listing_Standard ).GetField( "curX", Controller.Data.UniversalBindingFlags );
            if( _curX == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'curX' in 'Listing_Standard'",
                    "Lister_Standard_Extensions" );
            }
        }

        public static float                 Indentation( this Listing_Standard obj )
        {
            return (float)_curX.GetValue( obj );
        }

        public static void                  Indent( this Listing_Standard listing, float distance = 24f )
        {
            var curX = listing.Indentation();
            var newX = curX + distance;
            var curW = listing.ColumnWidth;
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
