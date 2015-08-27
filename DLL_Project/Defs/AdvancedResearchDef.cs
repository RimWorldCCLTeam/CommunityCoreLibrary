using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public bool                         ConsolidateHelp;
        public HelpCategoryDef              helpCategoryDef;
        public bool                         HideUntilResearched;

        public List< RecipeDef >            recipeDefs;
        public List< string >               sowTags;
        public List< ThingDef >             thingDefs;
        public List< ResearchProjectDef >   effectedResearchDefs;
        public List< AdvancedResearchMod >  researchMods;

        #endregion

        [Unsaved]

        #region Instance Data

        bool                                isEnabled;
        bool                                researchSorted;

        HelpDef                             helpDef;

        List< AdvancedResearchDef >         matchingAdvancedResearch;
        public AdvancedResearchDef          HelpConsolidator;

        float                               totalCost = -1;

        #endregion

        #region Query State

        public bool                         IsEnabled
        {
            get
            {
                return isEnabled;
            }
        }

        public bool                         IsHelpEnabled
        {
            get
            {
                return HideUntilResearched ? isEnabled : true;
            }
        }

        public bool                         IsLockedOut()
        {
            foreach( var p in researchDefs )
            {
                if( p.IsLockedOut() )
                {
                    // Any of the research parents locked out?
                    return true;
                }
            }
            return false;
        }

        public bool                         IsValid
        {
            get
            {
                // Hopefully...
                var isValid = true;

#if DEBUG
                
                // Validate recipes
                if( IsRecipeToggle )
                {
                    // Make sure thingDefs are of the appropriate type (has ITab_Bills)
                    foreach( var thingDef in thingDefs )
                    {
                        if( thingDef.thingClass.GetInterface( "IBillGiver" ) == null )
                        {
                            // Invalid project
                            isValid = false;
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + thingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + defName + " ) - Must implement \"IBillGiver\"" );
                        }
                    }

                }

                // Validate plant sowTags
                if( IsPlantToggle )
                {
                    // Make sure things are of the appropriate class (Plant)
                    foreach( var thingDef in thingDefs )
                    {
                        if( thingDef.thingClass != typeof( Plant ) )
                        {
                            // Invalid project
                            isValid = false;
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + thingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + defName + " ) - Must be <thingClass> \"Plant\"" );
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
                    foreach( var thingDef in thingDefs )
                    {
                        if( ( string.IsNullOrEmpty( thingDef.designationCategory ) )||
                            ( thingDef.designationCategory.ToLower() == "none" ) )
                        {
                            // Invalid project
                            isValid = false;
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + thingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + defName + " ) - <designationCategory> must not be null or \"None\"" );
                        }
                    }
                }

                // Validate help
                if( HasHelp )
                {
                    if( string.IsNullOrEmpty( label ) )
                    {
                        // Error processing data
                        isValid = false;
                        Log.Error( "Community Core Library :: Advanced Research :: AdvancedResearchDef( " + defName + " ) has more than one research requirment but is missing a help label!" );
                    }
                    if( string.IsNullOrEmpty( description ) )
                    {
                        // Error processing data
                        isValid = false;
                        Log.Error( "Community Core Library :: Advanced Research :: AdvancedResearchDef( " + defName + " ) has more than one research requirment but is missing a help description!" );
                    }
                }

#endif
                
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

        public bool                         HasHelp
        {
            get
            {
                return ConsolidateHelp;
            }
        }

        bool                                HasMatchingResearch( AdvancedResearchDef other )
        {
            if( researchDefs.Count != other.researchDefs.Count )
            {
                return false;
            }

            SortResearch();
            other.SortResearch();

            for( int i = 0; i < researchDefs.Count; ++ i )
            {
                if( researchDefs[ i ] != other.researchDefs[ i ] )
                {
                    return false;
                }
            }
            return true;
        }

        public float                        TotalCost
        {
            get
            {
                if( totalCost < 0 )
                {
                    totalCost = 0;
                    foreach( var r in researchDefs )
                    {
                        totalCost += r.totalCost;
                    }
                }
                return totalCost;
            }
        }

        #endregion

        #region Process State

        public void                         Disable( bool firstTimeRun = false )
        {
            // Don't unset if not set
            if( ( !isEnabled )&&
                ( firstTimeRun == false ) )
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
            if( HasHelp )
            {
                // Build & toggle help
                ToggleHelp( true );
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
            if( HasHelp )
            {
                // Build & toggle help
                ToggleHelp();
            }
            // Flag it as enabled
            isEnabled = true;
        }

        void                                SortResearch()
        {
            if( researchSorted )
            {
                return;
            }
            researchDefs.Sort( delegate( ResearchProjectDef x, ResearchProjectDef y )
                {
                    return x.defName.CompareTo(y.defName) * -1;
                }
            );
            researchSorted = true;
        }

        #endregion

        #region Toggle States

        void                                ToggleRecipes( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {

                // Make sure the thing has a recipe list
                if( buildingDef.recipes == null )
                {
                    buildingDef.recipes = new List< RecipeDef >();
                }

                // Go through each recipe
                foreach( var recipeDef in recipeDefs )
                {

                    if( Hide )
                    {
                        // Hide recipe

                        // Remove building from recipe
                        if( recipeDef.recipeUsers!= null )
                        {
                            recipeDef.recipeUsers.Remove( buildingDef );
                        }

                        // Remove recipe from building
                        buildingDef.recipes.Remove( recipeDef );

                    }
                    else
                    {
                        // Add recipe to the building
                        buildingDef.recipes.Add( recipeDef );
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

        public void                         ToggleHelp( bool setInitialState = false )
        {
            if( HelpDef == null )
            {
                return;
            }
            bool Hide = !HideUntilResearched ? false : setInitialState;

            if( Hide )
            {
                // Hide it from the help system
                HelpDef.category = (HelpCategoryDef)null;
            }
            else
            {
                // Show it to the help system
                HelpDef.category = helpCategoryDef;
            }

            // Queue for recache
            ResearchController.helpCategoryCache.Add( helpCategoryDef );
        }

        #endregion

        #region Aggegate Data

        List< AdvancedResearchDef >         MatchingAdvancedResearch
        {
            get
            {
                if( ( matchingAdvancedResearch == null )&&
                    ( ConsolidateHelp ) )
                {
                    // Matching advanced research (by requirements) not including this one
                    matchingAdvancedResearch = ModController.AdvancedResearch.FindAll( a => (
                        ( a.defName != this.defName )&&
                        ( HasMatchingResearch( a ) )
                    ) );
                    // Set reference to help consolidator
                    foreach( var a in matchingAdvancedResearch )
                    {
                        a.HelpConsolidator = this;
                    }
                }
                return matchingAdvancedResearch;
            }
        }

        public List< Def >                  GetResearchRequirements()
        {
            // Return the list of research required
            return researchDefs.ConvertAll<Def>( def => (Def)def );
        }

        public List< ThingDef >             GetBuildsUnlocked()
        {
            // Buildings it unlocks
            var thingsOn = new List<ThingDef>();

            // Look at this def
            if( ( !HideDefs )&&
                ( IsBuildingToggle ) )
            {
                thingsOn.AddRange( thingDefs );
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if( ( !a.HideDefs )&&
                    ( a.IsBuildingToggle ) )
                {
                    thingsOn.AddRange( a.thingDefs );
                }
            }

            return thingsOn;
        }

        public List< ThingDef >             GetBuildsLocked()
        {
            // Buildings it locks
            var thingsOn = new List<ThingDef>();

            // Look at this def
            if( ( HideDefs )&&
                ( IsBuildingToggle ) )
            {
                thingsOn.AddRange( thingDefs );
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if( ( a.HideDefs )&&
                    ( a.IsBuildingToggle ) )
                {
                    thingsOn.AddRange( a.thingDefs );
                }
            }

            return thingsOn;
        }

        public void                         GetRecipesOnBuildingsUnlocked( ref List< RecipeDef > recipes, ref List< ThingDef > buildings )
        {
            // Recipes on buildings it unlocks
            recipes = new List<RecipeDef>();
            buildings = new List<ThingDef>();

            // Look at this def
            if( ( !HideDefs )&&
                ( IsRecipeToggle ) )
            {
                recipes.AddRange( recipeDefs );
                buildings.AddRange( thingDefs );
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if( ( !a.HideDefs )&&
                    ( a.IsRecipeToggle ) )
                {
                    recipes.AddRange( a.recipeDefs );
                    buildings.AddRange( a.thingDefs );
                }
            }
        }

        public void                         GetRecipesOnBuildingsLocked( ref List< RecipeDef > recipes, ref List< ThingDef > buildings )
        {
            // Recipes on buildings it unlocks
            recipes = new List<RecipeDef>();
            buildings = new List<ThingDef>();

            // Look at this def
            if( ( HideDefs )&&
                ( IsRecipeToggle ) )
            {
                recipes.AddRange( recipeDefs );
                buildings.AddRange( thingDefs );
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if( ( a.HideDefs )&&
                    ( a.IsRecipeToggle ) )
                {
                    recipes.AddRange( a.recipeDefs );
                    buildings.AddRange( a.thingDefs );
                }
            }
        }

        public void                         GetSowTagsOnPlantsUnlocked( ref List< string > SowTags, ref List< ThingDef > plants )
        {
            // Sow tags on plants it unlocks
            SowTags = new List<string>();
            plants = new List<ThingDef>();

            // Look at this def
            if( ( !HideDefs )&&
                ( IsPlantToggle ) )
            {
                SowTags.AddRange( sowTags );
                plants.AddRange( thingDefs );
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if( ( !a.HideDefs )&&
                    ( a.IsPlantToggle ) )
                {
                    SowTags.AddRange( a.sowTags );
                    plants.AddRange( a.thingDefs );
                }
            }
        }

        public void                         GetSowTagsOnPlantsLocked( ref List< string > SowTags, ref List< ThingDef > plants )
        {
            // Sow tags on plants it unlocks
            SowTags = new List<string>();
            plants = new List<ThingDef>();

            // Look at this def
            if( ( HideDefs )&&
                ( IsPlantToggle ) )
            {
                SowTags.AddRange( sowTags );
                plants.AddRange( thingDefs );
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if( ( a.HideDefs )&&
                    ( a.IsPlantToggle ) )
                {
                    SowTags.AddRange( a.sowTags );
                    plants.AddRange( a.thingDefs );
                }
            }
        }

        #endregion

        #region Help Def

        public HelpDef                      HelpDef
        {
            get
            {
                if( helpDef != null )
                {
                    return helpDef;
                }

                if( HelpConsolidator != null )
                {
                    return HelpConsolidator.HelpDef;
                }
                return null;
            }
            set
            {
                if( ConsolidateHelp )
                {
                    helpDef = value;
                }
            }
        }

        #endregion

    }

}
