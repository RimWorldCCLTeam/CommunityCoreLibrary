using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingRequestGroup_Extensions
    {

        public static List<Thing>           ListOfThingsByGroup( this ThingRequestGroup group )
        {
            var listsByGroup = Find.ListerThings.ListsByGroup();
            return listsByGroup[ (int) group ];
        }

    }

}
