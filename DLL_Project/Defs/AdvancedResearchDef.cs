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

        ResearchEnableMode                  researchState;
        bool                                researchSorted;

        HelpDef                             helpDef;

        List< AdvancedResearchDef >         matchingAdvancedResearch;
        AdvancedResearchDef                 researchConsolidator;

        float                               totalCost = -1;

        ModHelperDef                        modHelperDef;

        #endregion

        #region Query State

        public ResearchEnableMode ResearchState
        {
            get
            {
                return researchState;
            }
        }

        public bool IsHelpEnabled
        {
            get
            {
                return !HideUntilResearched || researchState != ResearchEnableMode.Incomplete;
            }
        }

        public bool IsLockedOut()
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

        public bool IsValid
        {
            get
            {
                // Hopefully...
                var isValid = true;

                var loadedMod = Find_Extensions.ModByDefOfType<AdvancedResearchDef>( defName );
                modHelperDef = Find_Extensions.ModHelperDefByMod( loadedMod );

                if( modHelperDef == null )
                {
                    // Missing ModHelperDef
                    isValid = false;
                    CCL_Log.Error( "Missing ModHelperDef for mod '" + loadedMod.name + "'", "Advanced Research" );
                    return false;
                }

#if DEBUG
                // Validate research
                if( researchDefs.NullOrEmpty() )
                {
                    // Invalid project
                    isValid = false;
                    CCL_Log.Error( "AdvancedResearchDef requires at least one research project!", defName );
                }

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
                            CCL_Log.Error( "thingDef( " + thingDef.defName + " ) is of inappropriate type, must implement \"IBillGiver\"", defName );
                        }
                    }

                }

                // Validate plant sowTags
                if( IsPlantToggle )
                {
                    // Make sure things are of the appropriate class (Plant)
                    foreach( var thingDef in thingDefs )
                    {
                        if( thingDef.thingClass != typeof( Plant )&& !thingDef.thingClass.IsSubclassOf( typeof(Plant) ) )
                        {
                            // Invalid project
                            isValid = false;
                            CCL_Log.Error( "thingDef( " + thingDef.defName + " ) is of inappropriate type, must be <thingClass> \"Plant\"", defName );
                        }
                    }

                    // Make sure sowTags are valid (!null or empty)
                    for( int i = 0; i < sowTags.Count; i++ )
                    {
                        var sowTag = sowTags[ i ];
                        if( string.IsNullOrEmpty( sowTag ) )
                        {
                            CCL_Log.Error( "sowTags( index = " + i + " ) resolved to null", defName );
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
                            CCL_Log.Error( "thingDef( " + thingDef.defName + " ) is of inappropriate type, <designationCategory> must not be null or \"None\"", defName );
                        }
                    }
                }

                // Validate help
                if( ( HasHelp )&&
                    ( ResearchConsolidator == this ) )
                {
                    if( string.IsNullOrEmpty( label ) )
                    {
                        // Error processing data
                        isValid = false;
                        CCL_Log.Error( "Def is help consolidator but missing label!", defName );
                    }
                    if( string.IsNullOrEmpty( description ) )
                    {
                        // Error processing data
                        isValid = false;
                        CCL_Log.Error( "Def is help consolidator but missing description!", defName );
                    }
                }

#endif

                return isValid;
            }
        }

        public ResearchEnableMode EnableMode
        {
            get
            {
                if( researchState == ResearchEnableMode.Complete )
                {
                    // Already on
                    return researchState;
                }

                // Check if research projects are done
                if( researchDefs.All( rd => rd.IsFinished ) )
                {
                    return ResearchEnableMode.Complete;
                }

                // God mode, allow it anyway
                if( Game.GodMode )
                {
                    return ResearchEnableMode.GodMode;
                }

                // One or more required research is incomplete
                return ResearchEnableMode.Incomplete;
            }
        }

        public bool IsRecipeToggle
        {
            get
            {
                // Determine if this def toggles recipes
                return (
                    ( !recipeDefs.NullOrEmpty() )&&
                    ( sowTags.NullOrEmpty() )&&
                    ( !thingDefs.NullOrEmpty() )
                );
            }
        }

        public bool IsPlantToggle
        {
            get
            {
                // Determine if this def toggles plant sow tags
                return (
                    ( recipeDefs.NullOrEmpty() )&&
                    ( !sowTags.NullOrEmpty() )&&
                    ( !thingDefs.NullOrEmpty() )
                );
            }
        }

        public bool IsBuildingToggle
        {
            get
            {
                // Determine if this def toggles buildings
                return (
                    ( recipeDefs.NullOrEmpty() )&&
                    ( sowTags.NullOrEmpty() )&&
                    ( !thingDefs.NullOrEmpty() )
                );
            }
        }

        public bool IsResearchToggle
        {
            get
            {
                // Determine if this def toggles research
                return (
                    ( !effectedResearchDefs.NullOrEmpty() )
                );
            }
        }

        public bool HasCallbacks
        {
            get
            {
                // Determine if this def has callbacks
                return (
                    ( !researchMods.NullOrEmpty() )
                );
            }
        }

        public bool HasHelp
        {
            get
            {
                return
                    ( ConsolidateHelp )||
                    (
                        ( ResearchConsolidator != null )&&
                        ( ResearchConsolidator.ConsolidateHelp )
                    );
            }
        }

        bool HasMatchingResearch( AdvancedResearchDef other )
        {
            if( researchDefs.Count != other.researchDefs.Count )
            {
                return false;
            }

            SortResearch();
            other.SortResearch();

            for( int i = 0; i < researchDefs.Count; ++i )
            {
                if( researchDefs[i] != other.researchDefs[i] )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Total research cost of all required researches
        /// </summary>
        public float TotalCost
        {
            get
            {
                if( totalCost < 0 )
                {
                    totalCost = researchDefs.Sum( rd => rd.totalCost );
                }
                return totalCost;
            }
        }

        

        public AdvancedResearchDef ResearchConsolidator
        {
            get
            {
                if( ConsolidateHelp )
                {
                    return this;
                }
                if( researchConsolidator == null )
                {
                    var matching = Controller.Data.AdvancedResearchDefs.Where( a => (
                        ( HasMatchingResearch( a ) )
                    ) ).ToList();
                    researchConsolidator = matching.FirstOrDefault( a => ( a.ConsolidateHelp ) );
                    if( researchConsolidator == null )
                    {
                        researchConsolidator = matching.FirstOrDefault();
                    }
                }
                return researchConsolidator;
            }
        }

        #endregion

        #region Process State

        public void Disable( bool firstTimeRun = false )
        {
            // Don't disable if it's not the first run or not yet enabled
            if(
                ( researchState == ResearchEnableMode.Incomplete )&&
                ( firstTimeRun == false )
            )
            {
                return;
            }
#if DEBUG
            if( modHelperDef.Verbosity >= Verbosity.StateChanges )
            {
                CCL_Log.Message( "Disabling AdvancedResearchDef '" + defName + "'", modHelperDef.ModName );
            }
#endif
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
            if(
                ( IsResearchToggle )&&
                ( researchState != ResearchEnableMode.GodMode )
            )
            {
                // Research toggle
                ToggleResearch( true );
            }
            if(
                ( HasCallbacks )&&
                ( !firstTimeRun )
            )
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
            researchState = ResearchEnableMode.Incomplete;
        }

        public void Enable( ResearchEnableMode mode )
        {
            // Don't enable if enabled
            if( researchState != ResearchEnableMode.Incomplete )
            {
                if(
                    ( researchState == ResearchEnableMode.GodMode )&&
                    ( mode != ResearchEnableMode.GodMode )&&
                    ( IsResearchToggle )
                )
                {
                    // Player completed research with god-mode on
                    // Research toggle
                    ToggleResearch();
                    // Flag it as enabled by mode
                    researchState = mode;
                }
                return;
            }
#if DEBUG
            if( modHelperDef.Verbosity >= Verbosity.StateChanges )
            {
                CCL_Log.Message( "Enabling AdvancedResearchDef '" + defName + "'", modHelperDef.ModName );
            }
#endif
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
            if(
                ( IsResearchToggle )&&
                ( mode != ResearchEnableMode.GodMode )
            )
            {
                // Research toggle, if it's not god mode
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
            // Flag it as enabled by mode
            researchState = mode;
        }

        void SortResearch()
        {
            if( researchSorted )
            {
                return;
            }
            researchDefs.Sort( delegate ( ResearchProjectDef x, ResearchProjectDef y )
                {
                    return x.defName.CompareTo( y.defName ) * -1;
                }
            );
            researchSorted = true;
        }

        #endregion

        #region Toggle States

        void ToggleRecipes( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {

                // Make sure the thing has a recipe list
                if( buildingDef.recipes == null )
                {
                    buildingDef.recipes = new List<RecipeDef>();
                }

                // Go through each recipe
                foreach( var recipeDef in recipeDefs )
                {

                    if( Hide )
                    {
                        // Hide recipe

                        // Remove building from recipe
                        if( recipeDef.recipeUsers != null )
                        {
                            recipeDef.recipeUsers.Remove( buildingDef );
                        }

                        // Remove recipe from building
                        buildingDef.recipes.Remove( recipeDef );

                    }
                    else if( !buildingDef.recipes.Contains( recipeDef ) )
                    {
                        // Add recipe to the building
                        buildingDef.recipes.Add( recipeDef );
                    }
                }

                // Add this building to the list to recache
                ResearchController.buildingCache.Add( buildingDef );
            }
        }

        void TogglePlants( bool setInitialState = false )
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
                    else if( !plantDef.plant.sowTags.Contains( sowTag ) )
                    {
                        // Show the plant
                        plantDef.plant.sowTags.Add( sowTag );
                    }
                }
            }
        }

        void ToggleBuildings( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            // Go through each building
            foreach( var buildingDef in thingDefs )
            {
                buildingDef.researchPrerequisite = Hide ? Research.Locker : Research.Unlocker;
            }
        }

        void ToggleResearch( bool setInitialState = false )
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

        void ToggleCallbacks( bool setInitialState = false )
        {
            bool Hide = !setInitialState ? HideDefs : !HideDefs;

            if( Hide )
            {
                // Cache the callbacks in reverse order when hiding
                for( int i = researchMods.Count - 1; i >= 0; i-- )
                {
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

        public void ToggleHelp( bool setInitialState = false )
        {
            if( ( !ConsolidateHelp )||
                ( HelpDef == null ) )
            {
                return;
            }
            bool Hide = HideUntilResearched && setInitialState;

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

        #region Aggregate Data

        List<AdvancedResearchDef> MatchingAdvancedResearch
        {
            get
            {
                if( ( matchingAdvancedResearch == null )&&
                    ( ResearchConsolidator == this ) )
                {
                    // Matching advanced research (by requirements)
                    var matching  = Controller.Data.AdvancedResearchDefs.FindAll( a => (
                        ( HasMatchingResearch( a ) )
                    ) );
                    // Find this research as the consolidator
                    researchConsolidator = matching.First( a => ( a.ConsolidateHelp ) );
                    if( researchConsolidator == null )
                    {
                        // Find the highest priority one instead
                        researchConsolidator = matching.First();
                    }
                    if( researchConsolidator == this )
                    {
                        // This is the final consolidator
                        matching.Remove( this );
                        // Set reference to help consolidator
                        foreach( var a in matching )
                        {
                            a.researchConsolidator = this;
                        }
                        matchingAdvancedResearch = matching;
                    }
                }
                // return the matching research or null
                return matchingAdvancedResearch;
            }
        }

        public List<Def> GetResearchRequirements()
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetResearchRequirements();
            }

            // Return the list of research required
            return researchDefs.ConvertAll<Def>( def => (Def)def );
        }

        public List<ThingDef> GetThingsUnlocked()
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetThingsUnlocked();
            }

            // Buildings it unlocks
            var thingdefs = new List<ThingDef>();

            // Look at this def
            if(
                ( !HideDefs )&&
                ( IsBuildingToggle )
            )
            {
                thingdefs.AddRange( thingDefs );
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( !a.HideDefs )&&
                        ( a.IsBuildingToggle )
                    )
                    {
                        thingdefs.AddRange( a.thingDefs );
                    }
                }
            }

            return thingdefs;
        }

        public List<ThingDef> GetThingsLocked()
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetThingsLocked();
            }

            // Buildings it locks
            var thingdefs = new List<ThingDef>();

            // Look at this def
            if(
                ( HideDefs )&&
                ( IsBuildingToggle )
            )
            {
                thingdefs.AddRange( thingDefs );
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( a.HideDefs )&&
                        ( a.IsBuildingToggle )
                    )
                    {
                        thingdefs.AddRange( a.thingDefs );
                    }
                }
            }

            return thingdefs;
        }

        public List<RecipeDef> GetRecipesUnlocked( ref List<ThingDef> thingdefs )
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetRecipesUnlocked( ref thingdefs );
            }

            // Recipes on buildings it unlocks
            var recipedefs = new List<RecipeDef>();
            if( thingdefs != null )
            {
                thingdefs.Clear();
            }

            // Look at this def
            if(
                ( !HideDefs )&&
                ( IsRecipeToggle )
            )
            {
                recipedefs.AddRange( recipeDefs );
                if( thingdefs != null )
                {
                    thingdefs.AddRange( thingDefs );
                }
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( !a.HideDefs )&&
                        ( a.IsRecipeToggle )
                    )
                    {
                        recipedefs.AddRange( a.recipeDefs );
                        if( thingdefs != null )
                        {
                            thingdefs.AddRange( a.thingDefs );
                        }
                    }
                }
            }

            return recipedefs;
        }

        public List<RecipeDef> GetRecipesLocked( ref List<ThingDef> thingdefs )
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetRecipesLocked( ref thingdefs );
            }

            // Recipes on buildings it locks
            var recipedefs = new List<RecipeDef>();
            if( thingdefs != null )
            {
                thingdefs.Clear();
            }

            // Look at this def
            if(
                ( HideDefs )&&
                ( IsRecipeToggle )
            )
            {
                recipedefs.AddRange( recipeDefs );
                if( thingdefs != null )
                {
                    thingdefs.AddRange( thingDefs );
                }
            }

            // Look in matching research
            foreach( var a in MatchingAdvancedResearch )
            {
                if(
                    ( a.HideDefs )&&
                    ( a.IsRecipeToggle )
                )
                {
                    recipedefs.AddRange( a.recipeDefs );
                    if( thingdefs != null )
                    {
                        thingdefs.AddRange( a.thingDefs );
                    }
                }
            }

            return recipedefs;
        }

        public List<string> GetSowTagsUnlocked( ref List<ThingDef> thingdefs )
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetSowTagsUnlocked( ref thingdefs );
            }

            // Sow tags on plants it unlocks
            var sowtags = new List<string>();
            if( thingdefs != null )
            {
                thingdefs.Clear();
            }

            // Look at this def
            if(
                ( !HideDefs )&&
                ( IsPlantToggle )
            )
            {
                sowtags.AddRange( sowTags );
                if( thingdefs != null )
                {
                    thingdefs.AddRange( thingDefs );
                }
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( !a.HideDefs )&&
                        ( a.IsPlantToggle )
                    )
                    {
                        sowtags.AddRange( a.sowTags );
                        if( thingdefs != null )
                        {
                            thingdefs.AddRange( a.thingDefs );
                        }
                    }
                }
            }

            return sowtags;
        }

        public List<string> GetSowTagsLocked( ref List<ThingDef> thingdefs )
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetSowTagsLocked( ref thingdefs );
            }

            // Sow tags on plants it unlocks
            var sowtags = new List<string>();
            if( thingdefs != null )
            {
                thingdefs.Clear();
            }

            // Look at this def
            if(
                ( HideDefs )&&
                ( IsPlantToggle )
            )
            {
                sowtags.AddRange( sowTags );
                if( thingdefs != null )
                {
                    thingdefs.AddRange( thingDefs );
                }
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( a.HideDefs )&&
                        ( a.IsPlantToggle )
                    )
                    {
                        sowtags.AddRange( a.sowTags );
                        if( thingdefs != null )
                        {
                            thingdefs.AddRange( a.thingDefs );
                        }
                    }
                }
            }

            return sowtags;
        }

        public List<Def> GetResearchUnlocked()
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetResearchUnlocked();
            }

            // Research it unlocks
            var researchdefs = new List<Def>();

            // Look at this def
            if(
                ( !HideDefs )&&
                ( IsResearchToggle )
            )
            {
                researchdefs.AddRange( effectedResearchDefs.ConvertAll<Def>( def => (Def)def ) );
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( !a.HideDefs )&&
                        ( a.IsResearchToggle )
                    )
                    {
                        researchdefs.AddRange( a.effectedResearchDefs.ConvertAll<Def>( def => (Def)def ) );
                    }
                }
            }

            return researchdefs;
        }

        public List<Def> GetResearchLocked()
        {
            if( ResearchConsolidator != this )
            {
                return ResearchConsolidator.GetResearchLocked();
            }

            // Research it locks
            var researchdefs = new List<Def>();

            // Look at this def
            if(
                ( HideDefs )&&
                ( IsResearchToggle )
            )
            {
                researchdefs.AddRange( effectedResearchDefs.ConvertAll<Def>( def => (Def)def ) );
            }

            // Look in matching research
            if( !MatchingAdvancedResearch.NullOrEmpty() )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if(
                        ( a.HideDefs )&&
                        ( a.IsResearchToggle )
                    )
                    {
                        researchdefs.AddRange( a.effectedResearchDefs.ConvertAll<Def>( def => (Def)def ) );
                    }
                }
            }

            return researchdefs;
        }

        #endregion

        #region Help Def

        public HelpDef HelpDef
        {
            get
            {
                if( helpDef != null )
                {
                    return helpDef;
                }

                if( ResearchConsolidator != null )
                {
                    return ResearchConsolidator.helpDef;
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

        #region ResearchProjectDef 'interface'

        public bool IsFinished
        {
            get
            {
                return ( ResearchState == ResearchEnableMode.Complete );
            }
        }
        
        #endregion

    }

}
