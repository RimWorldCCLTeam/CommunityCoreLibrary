using System.Reflection;
using System.Collections.Generic;

using RimWorld;
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

        internal static bool                HasConnectedFacilityFast( CompAffectedByFacilities compAffected, ThingDef facility )
        {
            return( compAffected.LinkedFacilitiesListForReading.Any( thing => thing.def == facility ) );
        }

        public static bool                  HasConnectedFacility( this ThingWithComps thingWithComps, ThingDef facility )
        {
            var compAffected = thingWithComps.GetComp<CompAffectedByFacilities>();
            if(
                ( facility == null )||
                ( compAffected == null )||
                ( compAffected.LinkedFacilitiesListForReading.NullOrEmpty() )
            )
            {
                return false;
            }
            return HasConnectedFacilityFast( compAffected, facility );
        }

        public static bool                  HasConnectedFacilities( this ThingWithComps thingWithComps, List<ThingDef> facilities )
        {
            var compAffected = thingWithComps.GetComp<CompAffectedByFacilities>();
            if(
                ( facilities.NullOrEmpty() )||
                ( compAffected == null )||
                ( compAffected.LinkedFacilitiesListForReading.NullOrEmpty() )
            )
            {
                return false;
            }
            foreach( var facility in facilities )
            {
                if( !HasConnectedFacilityFast( compAffected, facility ) )
                {
                    return false;
                }
            }
            return true;
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
