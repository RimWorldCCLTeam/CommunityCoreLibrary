using System.Reflection;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingWithComps_Extensions
    {

        private static FieldInfo            _ThingWithComps_comps;

        #region Comps Getter & Setter

        public static List< ThingComp >     GetComps ( this ThingWithComps thingWithComps )
        {
            return thingWithComps.AllComps;
        }

        public static void                  SetComps ( this ThingWithComps thingWithComps, List< ThingComp > comps )
        {
            if( _ThingWithComps_comps == null )
            {
                _ThingWithComps_comps = typeof( ThingWithComps ).GetField( "comps", BindingFlags.NonPublic | BindingFlags.Instance );
            }
            _ThingWithComps_comps.SetValue( thingWithComps, comps );
        }

        #endregion

    }

}
