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

        [Unsaved]
        private ThingDef                    facilityDef;
        [Unsaved]
        private CompProperties_Facility     facilityComp;

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

            return isValid;
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
            if( thingDef.GetCompProperties<CompProperties_AffectedByFacilities>() == null )
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

            // Get comps
            var targetComp = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
#if DEBUG
            if( targetComp == null )
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
            targetComp.linkableFacilities.AddUnique( facilityDef );

            // Building is [now] linked to the facility
#if DEBUG
            return targetComp.linkableFacilities.Contains( facilityDef );
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
                    string.Format( "An exception was thrown while trying to re-resolve ThingDef '{0}'\n{1}", facility, e.ToString() ),
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
