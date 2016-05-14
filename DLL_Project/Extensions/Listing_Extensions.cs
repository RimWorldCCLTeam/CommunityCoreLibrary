using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Listing_Extensions
    {

        private static MethodInfo           _columnWidth;

        public static float                 ColumnWidth( this Listing obj )
        {
            if( _columnWidth == null )
            {
                PropertyInfo _columnWidthProperty = typeof( Listing ).GetProperty( "ColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic );
                _columnWidth = _columnWidthProperty.GetGetMethod( true );
            }
            return (float)_columnWidth.Invoke( obj, null );
        }

    }

}
