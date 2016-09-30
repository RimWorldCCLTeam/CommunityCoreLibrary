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
        public override string              InjectString => "Facilities injected";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.Facilities.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( int index = 0; index < def.Facilities.Count; ++index )
            {
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
                var qualifierValid = DefInjectionQualifier.TargetQualifierValid( injectionSet.targetDefs, injectionSet.qualifier, "Facilities", ref errors );
                isValid &= qualifierValid;
                if( qualifierValid )
                {
                    var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
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

            return isValid;
        }

        private bool                        CanInjectInto( ThingDef thingDef )
        {
            return( thingDef.GetCompProperties<CompProperties_AffectedByFacilities>() != null );
        }
#endif

        public override bool                DefIsInjected( ModHelperDef def )
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

        public override bool                InjectByDef( ModHelperDef def )
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
                    stringBuilder.Append( string.Format( "Facilities ({0}):: Qualifier returned: ", facilityDef.defName ) );
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
