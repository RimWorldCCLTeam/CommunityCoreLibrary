using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class SequencedInjectionSet_ThingDefAvailability : SequencedInjectionSet
    {

        public string                       menuHidden;
        public string                       designationCategory;
        public List< string >               researchPrerequisites;

        [Unsaved]
        private bool                        setMenuHidden = false;
        [Unsaved]
        private bool                        setMenuHiddenValue;

        [Unsaved]
        private bool                        setDesignation = false;

        [Unsaved]
        private bool                        setResearch = false;
        [Unsaved]
        private List<ResearchProjectDef>    setResearchValue;

        public                              SequencedInjectionSet_ThingDefAvailability()
        {
            injectionSequence               = InjectionSequence.MainLoad;
            injectionTiming                 = InjectionTiming.ThingDefAvailability;
        }

        public override Type                defType => typeof( ThingDef );
        public override InjectorTargetting  Targetting => InjectorTargetting.Multi;

        public override bool                IsValid()
        {
            var valid = true;
            if( !menuHidden.NullOrEmpty() )
            {
                var menuHiddenValid = menuHidden.ToLower();
                if(
                    ( menuHiddenValid != "true" )&&
                    ( menuHiddenValid != "false" )
                )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        string.Format( "menuHidden value of '{0}' is invalid", menuHidden ),
                        Name
                    );
                    valid = false;
                }
                else
                {
                    setMenuHidden = true;
                    setMenuHiddenValue = menuHidden.ToLower() == "true";
                }
            }
            if( !designationCategory.NullOrEmpty() )
            {
                if( designationCategory == "None" )
                {
                    setDesignation = true;
                }
                else
                {
                    var category = DefDatabase<DesignationCategoryDef>.GetNamed( designationCategory, true );
                    if( category == null )
                    {
                        CCL_Log.Trace(
                            Verbosity.Validation,
                            string.Format( "Cannot resolve DesignationCategory '{0}'", designationCategory ),
                            Name
                        );
                        valid = false;
                    }
                    else
                    {
                        setDesignation = true;
                    }
                }
            }
            if( researchPrerequisites != null )
            {
                var allResearchValid = true;
                if( researchPrerequisites.Count > 0 )
                {
                    for( int index = 0; index < researchPrerequisites.Count; ++index )
                    {
                        var projectDef = DefDatabase<ResearchProjectDef>.GetNamed( researchPrerequisites[ index ], true );
                        if( projectDef == null )
                        {
                            CCL_Log.Trace(
                                Verbosity.Validation,
                                string.Format( "Cannot resolve ResearchProjectDef '{0}'", researchPrerequisites[ index ] ),
                                Name
                            );
                            valid = false;
                            allResearchValid = false;
                        }
                    }
                }
                if( allResearchValid )
                {
                    setResearch = true;
                    if( researchPrerequisites.Count > 0 )
                    {
                        setResearchValue = DefDatabase<ResearchProjectDef>.AllDefs.Where( def => researchPrerequisites.Contains( def.defName ) ).ToList();
                    }
                    else
                    {
                        setResearchValue = null;
                    }
                }
            }
            return valid;
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
            if( setMenuHidden )
            {
                //Log.Message( string.Format( "Setting menuHidden to '{0}' on '{1}'", setMenuHiddenValue, thingDef.defName ) );
                thingDef.menuHidden = setMenuHiddenValue;
            }
            if( setDesignation )
            {
                //Log.Message( string.Format( "Setting designationCategory to '{0}' on '{1}'", designationCategory, thingDef.defName ) );
                thingDef.ChangeDesignationCategory( designationCategory );
            }
            if( setResearch )
            {
                //Log.Message( string.Format( "Setting researchPrerequisites to '{0}' on '{1}'", setResearchValue, thingDef.defName ) );
                thingDef.researchPrerequisites = setResearchValue;
            }
            return true;
        }

    }

}
