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
        public override string              InjectString => "ThingDefs availability changed";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ThingDefAvailability.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

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
                isValid &= DefInjectionQualifier.TargetQualifierValid( injectionSet.targetDefs, injectionSet.qualifier, "ThingDefAvailability", ref errors );
            }

            return isValid;
        }
#endif

        public override bool                DefIsInjected( ModHelperDef def )
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

        public override bool                InjectByDef( ModHelperDef def )
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
#if DEBUG
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append( "ThingDefAvailability :: Qualifier returned: " );
#endif
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
#if DEBUG
                        stringBuilder.Append( thingDef.defName + ", " );
#endif
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
#if DEBUG
                    CCL_Log.Message( stringBuilder.ToString(), def.ModName );
#endif
                }
            }

            dictInjected.Add( def.defName, true );
            return true;
        }

    }

}
