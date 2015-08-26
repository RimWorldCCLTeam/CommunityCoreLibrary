using System.Text;
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

        public HelpCategoryDef              helpCategoryDef;
        public bool                         ConsolidateHelp;
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

        #endregion

        #region Query State

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
                    // More than one research requirement means we can't use the research data
                    if( researchDefs.Count > 1 )
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
                            
                    // Try to generate help def
                    if( HelpDef == null )
                    {
                        // Error processing data
                        isValid = false;
                        Log.Error( "Community Core Library :: Advanced Research :: Unable to create HelpDef in AdvancedResearchDef( " + defName + " )" );
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
                return ( !IsLockedOut() )&&
                    ( helpCategoryDef != null );
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
            researchDefs.Sort();
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

        public void                         ToggleHelp( bool setInitialState = false )
        {
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
                if( matchingAdvancedResearch == null )
                {
                    // Matching advanced research (by requirements) not including this one
                    matchingAdvancedResearch = ModController.AdvancedResearch.FindAll( a => (
                        ( a.defName != this.defName )&&
                        ( HasMatchingResearch( a ) )
                    ) );
                }
                return matchingAdvancedResearch;
            }
        }

        string                              baseLabel()
        {
            // Set simple stuff
            if( ( researchDefs.Count == 1 )&&
                ( string.IsNullOrEmpty( label ) ) )
            {
                return researchDefs[ 0 ].label;
            }
            return label;
        }

        string                              baseDescription()
        {
            // Set simple stuff
            if( ( researchDefs.Count == 1 )&&
                ( string.IsNullOrEmpty( description ) ) )
            {
                return researchDefs[ 0 ].description;
            }
            return description;
        }

        #endregion

        #region Help Def

        public HelpDef                      HelpDef
        {
            get
            {
                if( helpDef == null )
                {
                    // Build the helpDef only once
                    helpDef = new HelpDef();
                    helpDef.defName = defName + "_Help";

                    // Set base label
                    helpDef.label = baseLabel();

                    // Build description
                    var stringBuilder = new StringBuilder();

                    // Start with base description
                    stringBuilder.AppendLine( baseDescription() );
                    stringBuilder.AppendLine();

                    // Fill in the rest of the description from the data
                    BuildHelpDescription( stringBuilder );

                    // Set the description
                    helpDef.description = stringBuilder.ToString();

                    // Inject the help def into the database
                    DefDatabase< HelpDef >.Add( helpDef );

                }

                return helpDef;
            }
        }

        #endregion

        #region String Builders

        void BuildHelpDescription( StringBuilder s )
        {
            BuildRequiredResearchDescription( s, "Required research:" );

            BuildRecipeDescription( s, "Adds recipes:", "To buildings:", false );
            BuildPlantDescription( s, "Adds sow tags:", "To plants:", false );
            BuildBuildingDescription( s, "Enables construction of:", false );
            BuildEffectedResearchDescription( s, "Enables research projects:", false );

            BuildRecipeDescription( s, "Removes recipes:", "From buildings:", true );
            BuildPlantDescription( s, "Removes sow tags:", "From plants:", true );
            BuildBuildingDescription( s, "Disables construction of:", true );
            BuildEffectedResearchDescription( s, "Disables research projects:", true );

        }

        void BuildRequiredResearchDescription( StringBuilder s, string prependResearch )
        {
            List< ResearchProjectDef > researchList;
            if( Priority == -1 )
            {
                var prerequisite = researchDefs[ 0 ];
                if( ( prerequisite.prerequisites == null )||
                    ( prerequisite.prerequisites.Count == 0 ) )
                {
                    return;
                }
                else
                {
                    researchList = prerequisite.prerequisites;
                }
            }
            else
            {
                researchList = researchDefs;
            }

            var labels = new List< string >();

            s.AppendLine( prependResearch );

            labels.Clear();
            for( int i = 0, count = researchList.Count - 1; i <= count; i++ ){
                var d = researchList[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();
        }

        void BuildRecipeDescription( StringBuilder s, string prependRecipes, string prependBuildings, bool hidden ) 
        {
            var labels = new List< string >();

            List< RecipeDef > recipeList = new List< RecipeDef >();
            List< ThingDef > thingList = new List< ThingDef >();
            if( ( HideDefs == hidden )&&
                ( IsRecipeToggle ) )
            {
                recipeList.AddRange( recipeDefs );
                thingList.AddRange( thingDefs );
            }

            if( ConsolidateHelp )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if( ( a.HideDefs == hidden )&&
                        ( a.IsRecipeToggle ) )
                    {
                        recipeList.AddRange( a.recipeDefs );
                        thingList.AddRange( a.thingDefs );
                    }
                }

                if( ( !hidden )&&
                    ( researchDefs.Count == 1 ) )
                {
                    // Only one research prerequisite, look at core defs too
                    var prerequisite = researchDefs[ 0 ];

                    var coreRecipes = DefDatabase<RecipeDef>.AllDefsListForReading.FindAll( r => (
                        ( r.researchPrerequisite == prerequisite )
                    ) );

                    foreach( var r in coreRecipes )
                    {
                        recipeList.Add( r );
                        thingList.AddRange( r.AllRecipeUsers );
                    }
                }

            }

            if( recipeList.Count == 0 )
            {
                return;
            }

            s.AppendLine( prependRecipes );
            labels.Clear();
            for( int i = 0, count = recipeList.Count - 1; i <= count; i++ ){
                var d = recipeList[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();

            s.AppendLine( prependBuildings );
            labels.Clear();
            for( int i = 0, count = thingList.Count - 1; i <= count; i++ ){
                var d = thingList[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();
        }

        void BuildPlantDescription( StringBuilder s, string prependSowTags, string prependPlants , bool hidden ) 
        {
            var labels = new List< string >();

            List< string > sowTagList = new List< string >();
            List< ThingDef > thingList = new List< ThingDef >();
            if( ( HideDefs == hidden )&&
                ( IsPlantToggle ) )
            {
                sowTagList.AddRange( sowTags );
                thingList.AddRange( thingDefs );
            }

            if( ConsolidateHelp )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if( ( a.HideDefs == hidden )&&
                        ( a.IsPlantToggle ) )
                    {
                        sowTagList.AddRange( a.sowTags );
                        thingList.AddRange( a.thingDefs );
                    }
                }
            }

            if( sowTagList.Count == 0 )
            {
                return;
            }

            s.AppendLine( prependSowTags );
            labels.Clear();
            for( int i = 0, count = sowTagList.Count - 1; i <= count; i++ ){
                var d = sowTagList[ i ];
                if( !labels.Contains( d.ToLower() ) )
                {
                    labels.Add( d.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d );
                }
            }
            s.AppendLine();

            s.AppendLine( prependPlants );
            labels.Clear();
            for( int i = 0, count = thingList.Count - 1; i <= count; i++ ){
                var d = thingList[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();
        }

        void BuildBuildingDescription( StringBuilder s, string prependBuildings, bool hidden ) 
        {
            var labels = new List< string >();

            List< ThingDef > thingList = new List< ThingDef >();
            if( ( HideDefs == hidden )&&
                ( IsBuildingToggle ) )
            {
                thingList.AddRange( thingDefs );
            }

            if( ConsolidateHelp )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if( ( a.HideDefs == hidden )&&
                        ( a.IsBuildingToggle ) )
                    {
                        thingList.AddRange( a.thingDefs );
                    }
                }

                if( ( !hidden )&&
                    ( researchDefs.Count == 1 ) )
                {
                    // Only one research prerequisite, look at core defs too
                    var prerequisite = researchDefs[ 0 ];

                    var coreThings = DefDatabase<ThingDef>.AllDefsListForReading.FindAll( t => (
                        ( t.researchPrerequisite == prerequisite )
                    ) );

                    thingList.AddRange( coreThings );
                }

            }

            if( thingList.Count == 0 )
            {
                return;
            }

            s.AppendLine( prependBuildings );
            labels.Clear();
            for( int i = 0, count = thingList.Count - 1; i <= count; i++ ){
                var d = thingList[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();
        }

        void BuildEffectedResearchDescription( StringBuilder s, string prependResearch, bool hidden )
        {
            var labels = new List< string >();

            List< ResearchProjectDef > researchList = new List< ResearchProjectDef >();
            if( ( HideDefs == hidden )&&
                ( IsResearchToggle ) )
            {
                researchList.AddRange( effectedResearchDefs );
            }

            if( ConsolidateHelp )
            {
                foreach( var a in MatchingAdvancedResearch )
                {
                    if( ( a.HideDefs == hidden )&&
                        ( a.IsResearchToggle ) )
                    {
                        researchList.AddRange( a.effectedResearchDefs );
                    }
                }

                if( ( researchDefs.Count == 1 )&&
                    ( !hidden ) )
                {
                    var prerequisite = researchDefs[ 0 ];
                    var children = DefDatabase<ResearchProjectDef>.AllDefsListForReading.FindAll( r => (
                        ( r.prerequisites != null )&&
                        ( r.prerequisites.Contains( prerequisite ) )
                    ) );
                    researchList.AddRange( children );
                }
            }

            if( researchList.Count == 0 )
            {
                return;
            }

            s.AppendLine( prependResearch );
            labels.Clear();
            for( int i = 0, count = researchList.Count - 1; i <= count; i++ ){
                var d = researchList[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();
        }

        #endregion

    }

}
