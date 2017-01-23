using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingRequestGroup_Extensions
    {

        public static List<Thing>           ListOfThingsByGroup( this ThingRequestGroup group, Map map )
        {
            if (map == null)
            {
                return Find.Maps.SelectMany( m => m.listerThings.ListsByGroup()[ (int)group] ).ToList();
            }

            return map.listerThings.ListsByGroup()[ (int) group ];
        }

    }

}
