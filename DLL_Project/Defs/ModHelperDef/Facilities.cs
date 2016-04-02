using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_Facilities : IInjector
    {

        // Dictionary of facilities to re-resolve
        private static Dictionary<ThingDef,CompProperties> facilityComps;

        static                              MHD_Facilities()
        {
            facilityComps = new Dictionary<ThingDef,CompProperties>();
        }

        // Link a building with a facility
        // affectedDef must have CompAffectedByFacilities
        // facilityDef must have CompFacility
        public static bool                  LinkFacility( ThingDef affectedDef, ThingDef facilityDef )
        {
            // Get comps
            var affectedComp = affectedDef.GetCompProperties( typeof( CompAffectedByFacilities ) );
            var facilityComp = facilityDef.GetCompProperties( typeof( CompFacility ) );
            if(
                ( affectedComp == null )||
                ( facilityComp == null )
            )
            {
                // Bad call
                return false;
            }

            // Is this building already linked?
            if( !affectedComp.linkableFacilities.Contains( facilityDef ) )
            {
                // Add the facility to the building
                affectedComp.linkableFacilities.Add( facilityDef );

                // Is the facility in the dictionary?
                if( !facilityComps.ContainsKey( facilityDef ) )
                {
                    // Add the facility to the dictionary
                    facilityComps.Add( facilityDef, facilityComp );
                }
            }

            // Building is [now] linked to the facility
            return true;
        }

        // Re-resolve all the facilities which have been updated
        public static void                  ReResolveDefs()
        {
            // Any facilities to re-resolve?
            if( facilityComps.Count > 0 )
            {
                // Get the facility and comp
                foreach( var keyValue in facilityComps )
                {
                    // comp.ResolveReferences( def )
                    keyValue.Value.ResolveReferences( keyValue.Key );
                }
            }
        }

#if DEBUG
        public string                       InjectString
        {
            get
            {
                return "Facilities injected";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.Facilities.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( int index = 0; index < def.Facilities.Count; ++index )
            {
                var facilitySet = def.Facilities[ index ];
                if( facilitySet.facility.NullOrEmpty() )
                {
                    errors += string.Format( "\n\tfacility in Facilities {0} is null", index );
                    isValid = false;
                }
                else
                {
                    var facilityDef = DefDatabase<ThingDef>.GetNamed( facilitySet.facility, false );
                    if( facilityDef == null )
                    {
                        errors += string.Format( "Unable to resolve facility '{0}' in Facilities", facilitySet.facility );
                        isValid = false;
                    }
                    else if( facilityDef.GetCompProperties( typeof( CompFacility ) ) == null )
                    {
                        // Check comps
                        errors += string.Format( "'{0}' is missing CompFacility for facility injection", facilitySet.facility );
                        isValid = false;
                    }
                }
                for( int index2 = 0; index2 < facilitySet.targetDefs.Count; ++index2 )
                {
                    var target = facilitySet.targetDefs[ index2 ];
                    var targetDef = DefDatabase<ThingDef>.GetNamed( target, false );
                    if( targetDef == null )
                    {
                        errors += string.Format( "Unable to resolve targetDef '{0}' in Facilities", target );
                        isValid = false;
                    }
                    else if( targetDef.GetCompProperties( typeof( CompAffectedByFacilities ) ) == null )
                    {
                        errors += string.Format( "'{0}' is missing CompAffectedByFacilities for facility injection", target );
                        isValid = false;
                    }
                }
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.Facilities.NullOrEmpty() )
            {
                return true;
            }

            foreach( var facility in def.Facilities )
            {
                var facilityDef = DefDatabase<ThingDef>.GetNamed( facility.facility );

                foreach( var target in facility.targetDefs )
                {
                    var targetDef = DefDatabase<ThingDef>.GetNamed( target );
                    var targetComp = targetDef.GetCompProperties( typeof( CompAffectedByFacilities ) );
                    if( !targetComp.linkableFacilities.Contains( facilityDef ) )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool                         Inject( ModHelperDef def )
        {
            if( def.Facilities.NullOrEmpty() )
            {
                return true;
            }

            foreach( var facility in def.Facilities )
            {
                var facilityDef = DefDatabase<ThingDef>.GetNamed( facility.facility );
                foreach( var target in facility.targetDefs )
                {
                    var targetDef = DefDatabase<ThingDef>.GetNamed( target );
                    LinkFacility( targetDef, facilityDef );
                }
            }

            return true;
        }

    }

}
