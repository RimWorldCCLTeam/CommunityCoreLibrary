using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _CompAffectedByFacilities : CompAffectedByFacilities
    {

        internal static MethodInfo          _IsPotentiallyValidFacilityForMe_Static;

        static                              _CompAffectedByFacilities()
        {
            _IsPotentiallyValidFacilityForMe_Static = typeof( CompAffectedByFacilities ).GetMethods( Controller.Data.UniversalBindingFlags ).FirstOrDefault( methodInfo => (
                ( methodInfo.Name == "IsPotentiallyValidFacilityForMe_Static" )&&
                ( methodInfo.GetParameters().Count() == 6 )
            ) );
            if( _IsPotentiallyValidFacilityForMe_Static == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'IsPotentiallyValidFacilityForMe_Static' in 'CompAffectedByFacilities'",
                    "Detour.CompAffectedByFacilities" );
            }
        }

        #region Reflected Methods

        internal static bool                IsPotentiallyValidFacilityForMe_Static( ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot, ThingDef myDef, IntVec3 myPos, Rot4 myRot )
        {
            return (bool) _IsPotentiallyValidFacilityForMe_Static.Invoke( null, new object[]{ facilityDef, facilityPos, facilityRot, myDef, myPos, myRot } );
        }

        #endregion

        #region Detoured Methods

        [DetourClassMethod( typeof( CompAffectedByFacilities ), "IsPotentiallyValidFacilityForMe" )]
        internal bool _IsPotentiallyValidFacilityForMe( ThingDef facilityDef, IntVec3 facilityPos, Rot4 facilityRot )
        {
            if( !IsPotentiallyValidFacilityForMe_Static( facilityDef, facilityPos, facilityRot, this.parent.def, this.parent.Position, this.parent.Rotation ) )
            {
                return false;
            }
            var propFacility = facilityDef.GetCompProperties<CompProperties_Facility>();
            if( propFacility.canLinkToMedBedsOnly )
            {
                var parentBed = this.parent as Building_Bed;
                if(
                    ( parentBed == null )||
                    ( !parentBed.Medical )
                )
                {
                    var propAffected = this.props as CommunityCoreLibrary.CompProperties_AffectedByFacilities;
                    if(
                        ( propAffected == null )||
                        ( !propAffected.overrideBedOnly )
                    )
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

    }

}
