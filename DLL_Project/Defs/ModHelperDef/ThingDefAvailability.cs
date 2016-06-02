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
                var availabilitySet = def.ThingDefAvailability[ index ];
                bool processThis = true;
                if( !availabilitySet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( availabilitySet.requiredMod ) != null;
                }
                if( processThis )
                {
                    if( !availabilitySet.menuHidden.NullOrEmpty() )
                    {
                        var menuHidden = availabilitySet.menuHidden.ToLower();
                        if(
                            ( menuHidden != "true" )&&
                            ( menuHidden != "false" )
                        )
                        {
                            isValid = false;
                            errors += string.Format( "\n\tmenuHidden '{0}' is invalid in ThingDefAvailability", availabilitySet.menuHidden );
                        }
                        if( !availabilitySet.designationCategory.NullOrEmpty() )
                        {
                            if( availabilitySet.designationCategory != "None" )
                            {
                                var category = DefDatabase<DesignationCategoryDef>.GetNamed( availabilitySet.designationCategory, true );
                                if( category == null )
                                {
                                    isValid = false;
                                    errors += string.Format( "\n\tDesignationCategory '{0}' is invalid in ThingDefAvailability", availabilitySet.designationCategory );
                                }
                            }
                        }
                        if( availabilitySet.researchPrerequisites != null )
                        {
                            if( availabilitySet.researchPrerequisites.Count > 0 )
                            {
                                for( int index2 = 0; index2 < availabilitySet.researchPrerequisites.Count; ++index2 )
                                {
                                    var projectDef = DefDatabase<ResearchProjectDef>.GetNamed( availabilitySet.researchPrerequisites[ index2 ], true );
                                    if( projectDef == null )
                                    {
                                        isValid = false;
                                        errors += string.Format( "\n\tresearchPrerequisite '{0}' is invalid in ThingDefAvailability", availabilitySet.researchPrerequisites[ index2 ] );
                                    }
                                }
                            }
                        }
                        if( availabilitySet.targetDefs.NullOrEmpty() )
                        {
                            errors += "\n\tNull or no targetDefs in ThingDefAvailability";
                            isValid = false;
                        }
                        else
                        {
                            for( int index2 = 0; index2 < availabilitySet.targetDefs.Count; ++index2 )
                            {
                                var targetDef = DefDatabase<ThingDef>.GetNamed( availabilitySet.targetDefs[ index2 ], true );
                                if( targetDef == null )
                                {
                                    isValid = false;
                                    errors += string.Format( "\n\ttargetDef '{0}' is invalid in ThingDefAvailability", availabilitySet.targetDefs[ index2 ] );
                                }
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
                var availabilitySet = def.ThingDefAvailability[ index ];

                bool processThis = true;
                if( !availabilitySet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( availabilitySet.requiredMod ) != null;
                }
                if( processThis )
                {
                    bool setMenuHidden = !availabilitySet.menuHidden.NullOrEmpty();
                    bool setDesignation = !availabilitySet.designationCategory.NullOrEmpty();
                    bool setResearch = availabilitySet.researchPrerequisites != null;

                    bool menuHidden = false;
                    List<ResearchProjectDef> research = null;

                    if( setMenuHidden )                                  
                    {
                        menuHidden = availabilitySet.menuHidden.ToLower() == "true" ? true : false;
                    }
                    if(
                        ( setResearch )&&
                        ( availabilitySet.researchPrerequisites.Count > 0 )
                    )
                    {
                        research = DefDatabase<ResearchProjectDef>.AllDefs.Where( projectDef => availabilitySet.researchPrerequisites.Contains( projectDef.defName ) ).ToList();
                    }

                    var targetDefs = DefDatabase<ThingDef>.AllDefs.Where( thingDef => availabilitySet.targetDefs.Contains( thingDef.defName ) ).ToList();

                    foreach( var target in targetDefs )
                    {
                        if( setMenuHidden )
                        {
                            target.menuHidden = menuHidden;
                        }
                        if( setDesignation )
                        {
                            target.ChangeDesignationCategory( availabilitySet.designationCategory );
                        }
                        if( setResearch )
                        {
                            target.researchPrerequisites = research;
                        }
                    }
                }
            }

            dictInjected.Add( def.defName, true );
            return true;
        }

    }

}
