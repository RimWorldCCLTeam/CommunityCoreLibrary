using System;
using System.Collections.Generic;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjectionSet_Facility : SequencedInjectionSet
    {

        public string                       facility;
        public string                       overrideBedOnly = string.Empty;
        public string                       overrideMaxSimultaneous = string.Empty;

        [Unsaved]
        private ThingDef                    facilityDef;
        [Unsaved]
        private CompProperties_Facility     facilityComp;

        [Unsaved]
        private bool                        setoverrideBedOnly = false;
        [Unsaved]
        private bool                        setoverrideBedOnlyValue;

        [Unsaved]
        private bool                        setoverrideMaxSimultaneous = false;
        [Unsaved]
        private int                         setoverrideMaxSimultaneousValue;

        public                              SequencedInjectionSet_Facility()
        {
            injectionSequence               = InjectionSequence.MainLoad;
            injectionTiming                 = InjectionTiming.Facilities;
        }

        public override Type                defType => typeof( ThingDef );
        public override InjectorTargetting  Targetting => InjectorTargetting.Multi;

        public override bool                IsValid()
        {
            bool isValid = true;

            if( string.IsNullOrEmpty( facility ) )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    "facility is null",
                    Name
                );
                isValid = false;
            }
            else
            {
                facilityDef = DefDatabase<ThingDef>.GetNamed( facility, false );
                if( facilityDef == null )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        string.Format( "Unable to resolve facility '{0}'", facility ),
                        Name
                    );
                    isValid = false;
                }
                else
                {
                    facilityComp = facilityDef.GetCompProperties<CompProperties_Facility>();
                    if( facilityComp == null )
                    {
                        // Check comps
                        CCL_Log.Trace(
                            Verbosity.Validation,
                            string.Format( "ThingDef '{0}' requires CompFacility", facility ),
                            Name
                        );
                        isValid = false;
                    }
                }
            }

            if( !string.IsNullOrEmpty( overrideBedOnly ) )
            {
                var overrideBedOnlyValid = overrideBedOnly.ToLower();
                if(
                    ( overrideBedOnlyValid != "true" )&&
                    ( overrideBedOnlyValid != "false" )
                )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        string.Format( "overrideBedOnly value of '{0}' is invalid", overrideBedOnly ),
                        Name
                    );
                    isValid = false;
                }
                else
                {
                    setoverrideBedOnly = true;
                    setoverrideBedOnlyValue = overrideBedOnly.ToLower() == "true";
                }
            }

            if( !string.IsNullOrEmpty( overrideMaxSimultaneous ) )
            {
                var overrideMaxSimultaneousValid = Convert.ToInt32( overrideMaxSimultaneous );
                if( overrideMaxSimultaneousValid < 1 )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        string.Format( "overrideMaxSimultaneous value of '{0}' is invalid", overrideMaxSimultaneous ),
                        Name
                    );
                    isValid = false;
                }
                else
                {
                    setoverrideMaxSimultaneous = true;
                    setoverrideMaxSimultaneousValue = overrideMaxSimultaneousValid;
                }
            }

            return isValid;
        }

        private static RimWorld.CompProperties_AffectedByFacilities GetAffectedPropeties( ThingDef thingDef )
        {
            var compProps = thingDef.GetCompProperties<RimWorld.CompProperties_AffectedByFacilities>();
            if( compProps == null )
            {   // Try get CCL's properties
                compProps = (RimWorld.CompProperties_AffectedByFacilities) thingDef.GetCompProperties<CommunityCoreLibrary.CompProperties_AffectedByFacilities>();
            }
            return compProps;
        }

        public override bool                TargetIsValid( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            if( GetAffectedPropeties( thingDef ) == null )
            {   // Applied to an invald def
#if DEBUG
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "ThingDef '{0}' requires CompAffectedByFacilities", target.defName ),
                    Name
                );
#endif
                return false;
            }
            return true;
        }

        public override bool                Inject( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif

            // Get affected properties
            var propsAffected = GetAffectedPropeties( thingDef );
#if DEBUG
            if( propsAffected == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "ThingDef '{0}' requires CompAffectedByFacilities", target.defName ),
                    Name
                );
                return false;
            }
#endif

            // Add the facility to the building
            propsAffected.linkableFacilities.AddUnique( facilityDef );

            // Set new max simultaneous if present and larger
            if( setoverrideMaxSimultaneous )
            {
                var propsFacility = facilityDef.GetCompProperties<CompProperties_Facility>();
#if DEBUG
                if( propsFacility == null )
                {   // Should never happen
                    CCL_Log.Trace(
                        Verbosity.Injections,
                        string.Format( "ThingDef '{0}' requires CompFacility", facility ),
                        Name
                    );
                    return false;
                }
#endif
                propsFacility.maxSimultaneous = Math.Max( propsFacility.maxSimultaneous, setoverrideMaxSimultaneousValue );
            }

            // Update overrides on extended comp
            var cclProps = propsAffected as CommunityCoreLibrary.CompProperties_AffectedByFacilities;
            if( cclProps != null )
            {
                if( setoverrideBedOnly )
                {
                    cclProps.overrideBedOnly = setoverrideBedOnlyValue;
                }
            }

            // Building is [now] linked to the facility
#if DEBUG
            return propsAffected.linkableFacilities.Contains( facilityDef );
#else
            return true;
#endif
        }

        // Re-resolve the facility
        public override bool                ReResolveDefs()
        {
#if DEBUG
            var reresolved = true;
            try
            {
#endif
                // comp.ResolveReferences( def )
                facilityComp.ResolveReferences( facilityDef );
#if DEBUG
            }
            catch( Exception e )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "An exception was thrown while trying to re-resolve CompProperties_Facility on ThingDef '{0}'\n{1}", facility, e.ToString() ),
                    Name
                );
                reresolved = false;
            }
            return reresolved;
#else
            return true;
#endif
        }

    }

}
