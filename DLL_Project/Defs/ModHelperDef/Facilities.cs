using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    [StaticConstructorOnStartup]
    public class MHD_Facilities : IInjector
    {

        // Dictionary of facilities to re-resolve
        private static Dictionary<ThingDef,CompProperties_Facility> facilityComps;

        static                              MHD_Facilities()
        {
            facilityComps = new Dictionary<ThingDef,CompProperties_Facility>();
        }

        // Link a building with a facility
        // affectedDef must have CompAffectedByFacilities
        // facilityDef must have CompFacility
        public static bool                  LinkFacility( ThingDef affectedDef, ThingDef facilityDef )
        {
            // Get comps
            var affectedComp = affectedDef.GetCompProperties<CompProperties_AffectedByFacilities>();
            var facilityComp = facilityDef.GetCompProperties<CompProperties_Facility>();
            if(
                ( affectedComp == null )||
                ( facilityComp == null )
            )
            {
                // Bad call
                return false;
            }

            // Add the facility to the building
            affectedComp.linkableFacilities.AddUnique( facilityDef );

            // Is the facility in the dictionary?
            if( !facilityComps.ContainsKey( facilityDef ) )
            {
                // Add the facility to the dictionary
                facilityComps.Add( facilityDef, facilityComp );
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
        public string                       InjectString => "Facilities injected";

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.Facilities.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( int index = 0; index < def.Facilities.Count; ++index )
            {
                var qualifierValid = true;
                var injectionSet = def.Facilities[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                if( injectionSet.facility.NullOrEmpty() )
                {
                    errors += string.Format( "\n\tfacility in Facilities {0} is null", index );
                    isValid = false;
                }
                else
                {
                    var facilityDef = DefDatabase<ThingDef>.GetNamed( injectionSet.facility, false );
                    if( facilityDef == null )
                    {
                        errors += string.Format( "Unable to resolve facility '{0}' in Facilities", injectionSet.facility );
                        isValid = false;
                    }
                    else if( facilityDef.GetCompProperties<CompProperties_Facility>() == null )
                    {
                        // Check comps
                        errors += string.Format( "'{0}' is missing CompFacility for facility injection", injectionSet.facility );
                        isValid = false;
                    }
                }
                if(
                    ( injectionSet.targetDefs.NullOrEmpty() )&&
                    ( injectionSet.qualifier == null )
                )
                {
                    errors += "targetDefs and qualifier are both null, one or the other must be supplied";
                    isValid = false;
                    qualifierValid = false;
                }
                if(
                    ( !injectionSet.targetDefs.NullOrEmpty() )&&
                    ( injectionSet.qualifier != null )
                )
                {
                    errors += "targetDefs and qualifier are both supplied, only one or the other must be supplied";
                    isValid = false;
                    qualifierValid = false;
                }
                if( qualifierValid )
                {
                    if( !injectionSet.targetDefs.NullOrEmpty() )
                    {
                        for( int index2 = 0; index2 < injectionSet.targetDefs.Count; ++index2 )
                        {
                            var thingDef = DefDatabase<ThingDef>.GetNamed( injectionSet.targetDefs[ index2 ], false );
                            if( thingDef == null )
                            {
                                errors += string.Format( "Unable to resolve targetDef '{0}' in Facilities", injectionSet.targetDefs[ index2 ] );
                                isValid = false;
                            }
                            else if( !CanInjectInto( thingDef ) )
                            {
                                errors += string.Format( "'{0}' is missing CompAffectedByFacilities for facility injection", injectionSet.targetDefs[ index2 ] );
                                isValid = false;
                            }
                        }
                    }
                    if( injectionSet.qualifier != null )
                    {
                        if( !injectionSet.qualifier.IsSubclassOf( typeof( DefInjectionQualifier ) ) )
                        {
                            errors += string.Format( "Unable to resolve qualifier '{0}'", injectionSet.qualifier );
                            isValid = false;
                        }
                        else
                        {
                            var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, null );
                            if( !thingDefs.NullOrEmpty() )
                            {
                                foreach( var thingDef in thingDefs )
                                {
                                    if( !CanInjectInto( thingDef ) )
                                    {
                                        errors += string.Format( "'{0}' is missing CompAffectedByFacilities for facility injection", thingDef.defName );
                                        isValid = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return isValid;
        }

        private bool                        CanInjectInto( ThingDef thingDef )
        {
            return( thingDef.GetCompProperties<CompProperties_AffectedByFacilities>() != null );
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.Facilities.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.Facilities.Count; index++ )
            {
                var injectionSet = def.Facilities[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var facilityDef = DefDatabase<ThingDef>.GetNamed( injectionSet.facility );
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
                    foreach( var thingDef in thingDefs )
                    {
                        var targetComp = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
                        if( !targetComp.linkableFacilities.Contains( facilityDef ) )
                        {
                            return false;
                        }
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

            for( var index = 0; index < def.Facilities.Count; index++ )
            {
                var injectionSet = def.Facilities[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var facilityDef = DefDatabase<ThingDef>.GetNamed( injectionSet.facility );
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
#if DEBUG
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append( "Facilities :: Qualifier returned: " );
#endif
                    foreach( var thingDef in thingDefs )
                    {
#if DEBUG
                        stringBuilder.Append( thingDef.defName + ", " );
#endif
                        LinkFacility( thingDef, facilityDef );
                    }
#if DEBUG
                    CCL_Log.Message( stringBuilder.ToString(), def.ModName );
#endif
                }
            }

            return true;
        }

    }

}
