using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class CompFacility_Extensions
    {

        private static FieldInfo            _linkedBuildings;

        public static List<Thing>           LinkedBuildings( this CompFacility compFacility )
        {
            if( _linkedBuildings == null )
            {
                _linkedBuildings = typeof( CompFacility ).GetField( "linkedBuildings", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (List<Thing>) _linkedBuildings.GetValue( compFacility );
        }

    }
}
