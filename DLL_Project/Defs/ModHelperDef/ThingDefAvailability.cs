using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_ThingDefAvailability : IInjector
    {

        private static Dictionary<string,bool>    dictInjected;

        static                              MHD_ThingDefAvailability()
        {
            dictInjected = new Dictionary<string,bool>();
        }

#if DEBUG
        public string                       InjectString => "ThingDefs availability changed";

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ThingDefAvailability.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( int index = 0; index < def.ThingDefAvailability.Count; ++index )
            {
                var qualifierValid = true;
                var injectionSet = def.ThingDefAvailability[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                if( !injectionSet.menuHidden.NullOrEmpty() )
                {
                    var menuHidden = injectionSet.menuHidden.ToLower();
                    if(
                        ( menuHidden != "true" )&&
                        ( menuHidden != "false" )
                    )
                    {
                        isValid = false;
                        errors += string.Format( "\n\tmenuHidden '{0}' is invalid in ThingDefAvailability", injectionSet.menuHidden );
                    }
                }
                if( !injectionSet.designationCategory.NullOrEmpty() )
                {
                    if( injectionSet.designationCategory != "None" )
                    {
                        var category = DefDatabase<DesignationCategoryDef>.GetNamed( injectionSet.designationCategory, true );
                        if( category == null )
                        {
                            isValid = false;
                            errors += string.Format( "\n\tDesignationCategory '{0}' is invalid in ThingDefAvailability", injectionSet.designationCategory );
                        }
                    }
                }
                if( injectionSet.researchPrerequisites != null )
                {
                    if( injectionSet.researchPrerequisites.Count > 0 )
                    {
                        for( int index2 = 0; index2 < injectionSet.researchPrerequisites.Count; ++index2 )
                        {
                            var projectDef = DefDatabase<ResearchProjectDef>.GetNamed( injectionSet.researchPrerequisites[ index2 ], true );
                            if( projectDef == null )
                            {
                                isValid = false;
                                errors += string.Format( "\n\tresearchPrerequisite '{0}' is invalid in ThingDefAvailability", injectionSet.researchPrerequisites[ index2 ] );
                            }
                        }
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
                            var targetDef = DefDatabase<ThingDef>.GetNamed( injectionSet.targetDefs[ index2 ], true );
                            if( targetDef == null )
                            {
                                isValid = false;
                                errors += string.Format( "\n\ttargetDef '{0}' is invalid in ThingDefAvailability", injectionSet.targetDefs[ index2 ] );
                            }
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
                }
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.ThingDefAvailability.NullOrEmpty() )
            {
                return true;
            }

            bool injected;
            if( !dictInjected.TryGetValue( def.defName, out injected ) )
            {
                return false;
            }

            return injected;
        }

        public bool                         Inject( ModHelperDef def )
        {
            if( def.ThingDefAvailability.NullOrEmpty() )
            {
                return true;
            }

            for( int index = 0; index < def.ThingDefAvailability.Count; ++index )
            {
                var injectionSet = def.ThingDefAvailability[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
                    bool setMenuHidden = !injectionSet.menuHidden.NullOrEmpty();
                    bool setDesignation = !injectionSet.designationCategory.NullOrEmpty();
                    bool setResearch = injectionSet.researchPrerequisites != null;

                    bool menuHidden = false;
                    List<ResearchProjectDef> research = null;

                    if( setMenuHidden )                                  
                    {
                        menuHidden = injectionSet.menuHidden.ToLower() == "true" ? true : false;
                    }
                    if(
                        ( setResearch )&&
                        ( injectionSet.researchPrerequisites.Count > 0 )
                    )
                    {
                        research = DefDatabase<ResearchProjectDef>.AllDefs.Where( projectDef => injectionSet.researchPrerequisites.Contains( projectDef.defName ) ).ToList();
                    }

                    foreach( var thingDef in thingDefs )
                    {
                        if( setMenuHidden )
                        {
                            thingDef.menuHidden = menuHidden;
                        }
                        if( setDesignation )
                        {
                            thingDef.ChangeDesignationCategory( injectionSet.designationCategory );
                        }
                        if( setResearch )
                        {
                            thingDef.researchPrerequisites = research;
                        }
                    }
                }
            }

            dictInjected.Add( def.defName, true );
            return true;
        }

    }

}
