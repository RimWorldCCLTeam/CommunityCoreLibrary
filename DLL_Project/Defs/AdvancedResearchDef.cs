using System.Collections.Generic;

using RimWorld;
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
        public List< AdvancedResearchMod >  researchMods;

        #endregion

        [Unsaved]

        #region Instance Data

        bool                                isEnabled;

        #endregion

        #region Query State

        public bool                         IsValid
        {
            get
            {
                // Hopefully...
                var isValid = true;

                // Validate recipes
                if( IsRecipeToggle )
                {
                    // Make sure thingDefs are of the appropriate type (has ITab_Bills)
                    foreach( var buildingDef in thingDefs )
                    {
                        if( buildingDef.thingClass.GetInterface( "IBillGiver" ) == null )
                        {
                            // Invalid project
                            isValid = false;
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + buildingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + defName + " ) - Must implement \"IBillGiver\"" );
                        }
                    }

                }

                // Validate plant sowTags
                if( IsPlantToggle )
                {
                    // Make sure things are of the appropriate class (Plant)
                    foreach( var plantDef in thingDefs )
                    {
                        if( plantDef.thingClass != typeof( Plant ) )
                        {
                            // Invalid project
                            isValid = false;
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + plantDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + defName + " ) - Must be <thingClass> \"Plant\"" );
                        }
                    }

                    // Make sure sowTags are valid (!null or empty)
                    for( int i = 0; i < sowTags.Count; i++ )
                    {
                        var sowTag = sowTags[ i ];
                        if( string.IsNullOrEmpty( sowTag ) )
                        {
                            Log.Error( "Community Core Library :: Advanced Research :: sowTags( index = " + i + " ) resolved to null in AdvancedResearchDef( " + defName + " )" );
                        }
                    }
                }

                // Validate buildings
                if( IsBuildingToggle )
                {
                    // Make sure thingDefs are of the appropriate type (has proper designationCategory)
                    foreach( var buildingDef in thingDefs )
                    {
                        if( ( string.IsNullOrEmpty( buildingDef.designationCategory ) )||
                            ( buildingDef.designationCategory.ToLower() == "none" ) )
                        {
                            // Invalid project
                            isValid = false;
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + buildingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + defName + " ) - <designationCategory> must not be null or \"None\"" );
                        }
                    }
                }

                return isValid;
            }
        }

        public bool                         CanEnable
        {
            get
            {
                // God mode, allow it
                if( ( Game.GodMode )&&
                    ( !isEnabled ) )
                {
                    return true;
                }

                if( isEnabled )
                {
                    // Already on
                    return false;
                }

                // Check individual research projects
                foreach( var researchProject in researchDefs )
                {
                    if( !researchProject.IsFinished )
                    {
                        return false;
                    }
                }

                // All required research complete
                return true;
            }
        }

        public bool                         IsRecipeToggle
        {
            get
            {
                // Determine if this def toggles recipes
                return (
                    ( ( recipeDefs != null )&&( recipeDefs.Count > 0 ) )&&
                    ( ( sowTags == null )||( ( sowTags != null )&&( sowTags.Count == 0 ) ) )&&
                    ( ( thingDefs != null )&&( thingDefs.Count > 0 ) )
                );
            }
        }

        public bool                         IsPlantToggle
        {
            get
            {
                // Determine if this def toggles plant sow tags
                return (
                    ( ( recipeDefs == null )||( ( recipeDefs != null )&&( recipeDefs.Count == 0 ) ) )&&
                    ( ( sowTags != null )&&( sowTags.Count > 0 ) )&&
                    ( ( thingDefs != null )&&( thingDefs.Count > 0 ) )
                );
            }
        }

        public bool                         IsBuildingToggle
        {
            get
            {
                // Determine if this def toggles buildings
                return (
                    ( ( recipeDefs == null )||( ( recipeDefs != null )&&( recipeDefs.Count == 0 ) ) )&&
                    ( ( sowTags == null )||( ( sowTags != null )&&( sowTags.Count == 0 ) ) )&&
                    ( ( thingDefs != null )&&( thingDefs.Count > 0 ) )
                );
            }
        }

        public bool                         IsResearchToggle
        {
            get
            {
                // Determine if this def toggles research
                return (
                    ( ( effectedResearchDefs != null )&&( effectedResearchDefs.Count > 0 ) )
                );
            }
        }

        public bool                         HasCallbacks
        {
            get
            {
                // Determine if this def has callbacks
                return (
                    ( ( researchMods != null )&&( researchMods.Count > 0 ) )
                );
            }
        }

        #endregion

        #region Process State

        public void                         Disable( bool firstTimeRun = false )
        {
            // Don't unset if not set
            if( ( !isEnabled )&&
                ( firstTimeRun = false ) )
            {
                return;
            }
            if( IsRecipeToggle )
            {
                // Recipe toggle
                ToggleRecipes( true );
            }
            if( IsPlantToggle )
            {
                // Plant toggle
                TogglePlants( true );
            }
            if( IsBuildingToggle )
            {
                // Building toggle
                ToggleBuildings( true );
            }
            if( IsResearchToggle )
            {
                // Research toggle
                ToggleResearch( true );
            }
            if( ( HasCallbacks )&&
                ( !firstTimeRun ) )
            {
                // Cache callbacks
                ToggleCallbacks( true );
            }
            // Flag it as disabled
            isEnabled = false;
        }

        public void                         Enable()
        {
            // Don't set if set
            if( isEnabled )
            {
                return;
            }
            if( IsRecipeToggle )
            {
                // Recipe toggle
                ToggleRecipes();
            }
            if( IsPlantToggle )
            {
                // Plant toggle
                TogglePlants();
            }
            if( IsBuildingToggle )
            {
                // Building toggle
                ToggleBuildings();
            }
            if( IsResearchToggle )
            {
                // Research toggle
                ToggleResearch();
            }
            if( HasCallbacks )
            {
                // Cache callbacks
                ToggleCallbacks();
            }
            // Flag it as enabled
            isEnabled = true;
        }

        void                                ToggleRecipes( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {

                // Go through each recipe
                foreach( var recipeDef in recipeDefs )
                {

                    // Make sure recipe has user list
                    if( recipeDef.recipeUsers == null )
                    {
                        recipeDef.recipeUsers = new List<ThingDef>();
                    }

                    if( Hide )
                    {
                        // Hide recipe

                        // Remove building from recipe
                        if( recipeDef.recipeUsers.IndexOf( buildingDef ) >= 0 )
                        {
                            recipeDef.recipeUsers.Remove( buildingDef );
                        }

                        // Remove recipe from building
                        if( ( buildingDef.recipes != null )&&
                            ( buildingDef.recipes.IndexOf( recipeDef ) >= 0 ) )
                        {
                            buildingDef.recipes.Remove( recipeDef );
                        }

                        // Remove bill on any table of this def using this recipe
                        var buildings = Find.ListerBuildings.AllBuildingsColonistOfDef( buildingDef );
                        foreach( var building in buildings )
                        {
                            var BillGiver = building as IBillGiver;
                            for( int i = 0; i < BillGiver.BillStack.Count; ++ i )
                            {
                                var bill = BillGiver.BillStack[ i ];
                                if( bill.recipe == recipeDef )
                                {
                                    BillGiver.BillStack.Delete( bill );
                                    continue;
                                }
                            }
                        }

                    }
                    else
                    {
                        // Add building to recipe
                        recipeDef.recipeUsers.Add( buildingDef );
                    }
                }

                // Add this building to the list to recache
                ResearchController.buildingCache.Add( buildingDef );
            }
        }

        void                                TogglePlants( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

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

        void                                ToggleBuildings( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {
                buildingDef.researchPrerequisite = Hide ? Research.Locker : Research.Unlocker;
            }
        }

        void                                ToggleResearch( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

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

        void                                ToggleCallbacks( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            if( Hide )
            {
                // Cache the callbacks in reverse order when hiding
                for( int i = researchMods.Count - 1; i >= 0; i-- ){
                    var researchMod = researchMods[ i ];
                    // Add the advanced research mod to the cache
                    ResearchController.researchModCache.Add( researchMod );
                }
            }
            else
            {
            // Cache the callbacks in order
                foreach( var researchMod in researchMods )
                {

                    // Add the advanced research mod to the cache
                    ResearchController.researchModCache.Add( researchMod );
                }
            }
        }

        #endregion

    }

}
