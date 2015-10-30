using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ListerThings_Extensions
    {
        
        public static List<Thing>           ListByGroup( this ThingRequestGroup group )
        {
            var listsByGroup = typeof( ListerThings ).GetField( "listsByGroup", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Find.ListerThings ) as List<Thing>[];
            return listsByGroup[ (int) group ];
        }

    }

}
