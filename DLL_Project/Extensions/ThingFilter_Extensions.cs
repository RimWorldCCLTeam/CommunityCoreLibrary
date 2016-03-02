using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingFilter_Extensions
    {

        public static bool                  Matches( this ThingFilter a, ThingFilter b )
        {
            foreach( var thingDefA in a.AllowedThingDefs )
            {
                if( !b.Allows( thingDefA ) )
                {
                    return false;
                }
            }
            foreach( var thingDefB in b.AllowedThingDefs )
            {
                if( !a.Allows( thingDefB ) )
                {
                    return false;
                }
            }
            return true;
        }

    }

}
