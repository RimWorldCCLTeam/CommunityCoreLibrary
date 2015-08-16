using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class AdvancedResearchDef : Def
    {

        #region XML Data

        // Processing priority so everything happens in the order it should
        // Lower value is higher priority
        public int                          Priority;

        // Flag this as true to hide/lock, false (default) for show/unlock
        public bool                         HideDefs;

        // Research requirement
        public List< ResearchProjectDef >   researchDefs;

        // These are optionally defined in xml
        public List< RecipeDef >            recipeDefs;
        public List< string >               sowTags;
        public List< ThingDef >             thingDefs;
        public List< ResearchProjectDef >   effectedResearchDefs;
        public List< ResearchMod >          researchMods;

        #endregion

        #region Instance Data
        [Unsaved]

        public bool                         isEnabled;

        #endregion

        #region Query State

        public bool                         ResearchComplete()
        {
            // God mode, allow it
            if( Game.GodMode )
            {
                return true;
            }

            // No god mode, check it
            foreach( var researchProject in researchDefs )
            {
                if( !researchProject.IsFinished )
                {
                    return false;
                }
            }

            // All done
            return true;
        }

        public bool                         IsRecipeToggle()
        {
            // Determine if this def toggles recipes
            return
                ( recipeDefs != null )&&( recipeDefs.Count > 0 )&&
                ( sowTags == null )||( sowTags.Count == 0 )&&
                ( thingDefs != null )&&( thingDefs.Count > 0 );
        }

        public bool                         IsPlantToggle()
        {
            // Determine if this def toggles plant sow tags
            return
                ( recipeDefs == null )||( recipeDefs.Count == 0 )&&
                ( sowTags != null )&&( sowTags.Count > 0 )&&
                ( thingDefs != null )&&( thingDefs.Count > 0 );
        }

        public bool                         IsBuildingToggle()
        {
            // Determine if this def toggles buildings
            return
                ( recipeDefs == null )||( recipeDefs.Count == 0 )&&
                ( sowTags == null )||( sowTags.Count == 0 )&&
                ( thingDefs != null )&&( thingDefs.Count > 0 );
        }

        public bool                         HasCallbacks()
        {
            // Determine if this def has callbacks
            return ( researchMods != null )&&( researchMods.Count > 0 );
        }

        public bool                         IsResearchToggle()
        {
            // Determine if this def toggles research
            return ( effectedResearchDefs != null )&&( effectedResearchDefs.Count > 0 );
        }

        #endregion

        #region Process State

        public void                         ToggleRecipes( List< ThingDef > buildingCache, bool SetInitialState = false )
        {
            bool Hide = !SetInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {

                // Go through each recipe
                foreach( var recipe in recipeDefs )
                {

                    if( Hide )
                    {
                        // Hide recipe

                        // Remove building from recipe
                        if( ( recipe.recipeUsers != null )&&
                            ( recipe.recipeUsers.IndexOf( buildingDef ) >= 0 ) )
                        {
                            recipe.recipeUsers.Remove( buildingDef );
                        }

                        // Remove recipe from building
                        if( ( buildingDef.recipes != null )&&
                            ( buildingDef.recipes.IndexOf( recipe ) >= 0 ) )
                        {
                            buildingDef.recipes.Remove( recipe );
                        }
                    }
                    else
                    {
                        // Make sure recipe has user list
                        if( recipe.recipeUsers == null )
                        {
                            recipe.recipeUsers = new List<ThingDef>();
                        }

                        // Add building to recipe
                        recipe.recipeUsers.Add( buildingDef );
                    }
                }

                // Add this building to the list to recache
                buildingCache.Add( buildingDef );
            }
        }

        public void                         TogglePlants( bool SetInitialState = false )
        {
            bool Hide = !SetInitialState ? HideDefs : !HideDefs;

            // Go through each plant
            foreach( var plantDef in thingDefs )
            {

                // Make sure it has a list to modify
                if( plantDef.plant.sowTags == null )
                {
                    plantDef.plant.sowTags = new List<string>();
                }

                // Go through each sowTag
                foreach( var sowTag in sowTags )
                {

                    if( Hide )
                    {
                        // Hide plant
                        plantDef.plant.sowTags.Remove( sowTag );
                    }
                    else
                    {
                        // Show the plant
                        plantDef.plant.sowTags.Add( sowTag );
                    }
                }
            }
        }

        public void                         ToggleBuildings( bool SetInitialState = false )
        {
            bool Hide = !SetInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {
                buildingDef.researchPrerequisite = Hide ? Research.Locker : Research.Unlocker;
            }
        }

        public void                         ToggleResearch( bool SetInitialState = false )
        {
            bool Hide = !SetInitialState ? HideDefs : !HideDefs;

            // Go through each research project to be effected
            foreach( var researchProject in effectedResearchDefs )
            {

                // Assign a new blank list
                researchProject.prerequisites = new List<ResearchProjectDef>();

                if( Hide )
                {
                    // Lock research
                    researchProject.prerequisites.Add( Research.Locker );
                }
            }
        }

        #endregion

    }

}
