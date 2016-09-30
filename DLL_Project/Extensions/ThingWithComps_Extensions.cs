using System.Reflection;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingWithComps_Extensions
    {

        private static FieldInfo            _ThingWithComps_comps;

        static                              ThingWithComps_Extensions()
        {
            _ThingWithComps_comps = typeof( ThingWithComps ).GetField( "comps", Controller.Data.UniversalBindingFlags );
            if( _ThingWithComps_comps == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'comps' in 'ThingWithComps'",
                    "ThingWithComps_Extensions" );
            }
        }

        #region Comps Getter & Setter

        public static List< ThingComp >     GetComps ( this ThingWithComps thingWithComps )
        {
            return thingWithComps.AllComps;
        }

        public static void                  SetComps ( this ThingWithComps thingWithComps, List< ThingComp > comps )
        {
            _ThingWithComps_comps.SetValue( thingWithComps, comps );
        }

        #endregion

    }

}
