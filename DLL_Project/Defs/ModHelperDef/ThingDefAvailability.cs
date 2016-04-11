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
        public string                       InjectString
        {
            get
            {
                return "ThingDefs availability changed";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ThingDefAvailability.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( int index = 0; index < def.ThingDefAvailability.Count; ++index )
            {
                var availability = def.ThingDefAvailability[ index ];
                if( !availability.menuHidden.NullOrEmpty() )
                {
                    var menuHidden = availability.menuHidden.ToLower();
                    if(
                        ( menuHidden != "true" )&&
                        ( menuHidden != "false" )
                    )
                    {
                        isValid = false;
                        errors += string.Format( "\n\tmenuHidden '{0}' is invalid in ThingDefAvailability", availability.menuHidden );
                    }
                    if( !availability.designationCategory.NullOrEmpty() )
                    {
                        if( availability.designationCategory != "None" )
                        {
                            var category = DefDatabase<DesignationCategoryDef>.GetNamed( availability.designationCategory, true );
                            if( category == null )
                            {
                                isValid = false;
                                errors += string.Format( "\n\tDesignationCategory '{0}' is invalid in ThingDefAvailability", availability.designationCategory );
                            }
                        }
                    }
                    if( availability.researchPrerequisites != null )
                    {
                        if( availability.researchPrerequisites.Count > 0 )
                        {
                            for( int index2 = 0; index2 < availability.researchPrerequisites.Count; ++index2 )
                            {
                                var projectDef = DefDatabase<ResearchProjectDef>.GetNamed( availability.researchPrerequisites[ index2 ], true );
                                if( projectDef == null )
                                {
                                    isValid = false;
                                    errors += string.Format( "\n\tresearchPrerequisite '{0}' is invalid in ThingDefAvailability", availability.researchPrerequisites[ index2 ] );
                                }
                            }
                        }
                    }
                    if( availability.targetDefs.NullOrEmpty() )
                    {
                        errors += "\n\tNull or no targetDefs in ThingDefAvailability";
                        isValid = false;
                    }
                    else
                    {
                        for( int index2 = 0; index2 < availability.targetDefs.Count; ++index2 )
                        {
                            var targetDef = DefDatabase<ThingDef>.GetNamed( availability.targetDefs[ index2 ], true );
                            if( targetDef == null )
                            {
                                isValid = false;
                                errors += string.Format( "\n\ttargetDef '{0}' is invalid in ThingDefAvailability", availability.targetDefs[ index2 ] );
                            }
                        }
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
                var availability = def.ThingDefAvailability[ index ];

                bool setMenuHidden = !availability.menuHidden.NullOrEmpty();
                bool setDesignation = !availability.designationCategory.NullOrEmpty();
                bool setResearch = availability.researchPrerequisites != null;

                bool menuHidden = false;
                DesignationCategoryDef newCategory = null;
                List<ResearchProjectDef> research = null;

                if( setMenuHidden )                                  
                {
                    menuHidden = availability.menuHidden.ToLower() == "true" ? true : false;
                }
                if( setDesignation )
                {
                    newCategory = availability.designationCategory == "None"
                                ? null
                                : DefDatabase<DesignationCategoryDef>.GetNamed( availability.designationCategory );
                }
                if(
                    ( setResearch )&&
                    ( availability.researchPrerequisites.Count > 0 )
                )
                {
                    research = DefDatabase<ResearchProjectDef>.AllDefs.Where( projectDef => availability.researchPrerequisites.Contains( projectDef.defName ) ).ToList();
                }

                var targetDefs = DefDatabase<ThingDef>.AllDefs.Where( thingDef => availability.targetDefs.Contains( thingDef.defName ) ).ToList();

                foreach( var target in targetDefs )
                {
                    if( setMenuHidden )
                    {
                        target.menuHidden = menuHidden;
                    }
                    if( setDesignation )
                    {
                        DesignationCategoryDef oldCategory = null;
                        Designator_Build oldDesignator = null;
                        if(
                            ( !target.designationCategory.NullOrEmpty() )&&
                            ( target.designationCategory != "None" )
                        )
                        {
                            oldCategory = DefDatabase<DesignationCategoryDef>.GetNamed( target.designationCategory );
                            oldDesignator = (Designator_Build) oldCategory.resolvedDesignators.FirstOrDefault( d => (
                                ( d is Designator_Build )&&
                                ( ( d as Designator_Build ).PlacingDef == (BuildableDef) target )
                            ) );
                        }
                        if( newCategory == null )
                        {
                            if( oldCategory != null )
                            {
                                oldCategory.resolvedDesignators.Remove( oldDesignator );
                            }
                        }
                        else
                        {
                            Designator_Build newDesignator = null;
                            if( oldDesignator != null )
                            {
                                oldCategory.resolvedDesignators.Remove( oldDesignator );
                                newDesignator = oldDesignator;
                            }
                            else
                            {
                                newDesignator = (Designator_Build) Activator.CreateInstance( typeof( Designator_Build ), new System.Object[] { (BuildableDef) target } );
                            }
                            newCategory.resolvedDesignators.Add( newDesignator );
                        }
                        target.designationCategory = availability.designationCategory;
                    }
                    if( setResearch )
                    {
                        target.researchPrerequisites = research;
                    }
                }
            }

            dictInjected.Add( def.defName, true );
            return true;
        }

    }

}
