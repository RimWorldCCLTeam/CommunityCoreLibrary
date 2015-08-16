using System.Reflection;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingWithComps_Extensions
    {
        #region Comps Getter & Setter

        public static List< ThingComp >     GetComps ( this ThingWithComps thingWithComps )
        {
            return thingWithComps.AllComps;
        }

        public static void                  SetComps ( this ThingWithComps thingWithComps, List< ThingComp > comps )
        {
            typeof( ThingWithComps ).GetField( "comps", BindingFlags.NonPublic | BindingFlags.Instance ).SetValue( thingWithComps, comps );
        }

        #endregion

    }

}
