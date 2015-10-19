using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class ModController : MonoBehaviour
    {

        #region Instance Data

        public readonly string              GameObjectName = "Community Core Library";

        static List< AdvancedResearchDef >  advancedResearch;

        public static Version               CCLVersionMin = new Version( "0.12.0" );
        public static Version               CCLVersion;
        List< ModHelperDef >                ModHelperDefs;

        #endregion

        #region Mono Callbacks

        public void                         Start()
        {
            enabled = false;

            // Check versions of mods and throw error to the user if the
            // mod version requirement is higher than the installed version
            GetCCLVersion();

            ModHelperDefs = DefDatabase< ModHelperDef >.AllDefs.ToList();

            if( ( ModHelperDefs != null )&&
                ( ModHelperDefs.Count > 0 ) )
            {
                ValidateMods();
            }

            // Do injections
            InjectSpecials();
            InjectThingComps();
            InjectDesignators();

            // Validate advanced research defs
            if( ValidateResearch() )
            {
                ResearchController.InitComponent();
            }

            // Enablers CCL buildings for mods wanting them
            EnableCCLBuildings();

            // Auto-generate help menus
            HelpController.Initialize();

            CCL_Log.Message( "Initialized" );

            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if(
                ( Game.Mode != GameMode.MapPlaying )||
                ( Find.Map == null )||
                ( Find.Map.components != null )
            )
            {
                return;
            }
            InjectPostLoaders();
            InjectMapComponents();
        }

        public void                         OnLevelWasLoaded()
        {
        }

        #endregion

        #region Static Properties

        public static List< AdvancedResearchDef > AdvancedResearch
        {
            get
            {
                if( advancedResearch == null )
                {
                    advancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();
                }
                return advancedResearch;
            }
        }

        #endregion

        #region Versioning

        void                                GetCCLVersion ()
        {
            // TODO:  Fix issue #30 so we can use proper assembly versioning
            //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //CCLVersion = assembly.GetName().Version;
            CCLVersion = new Version( "0.12.4" );
#if DEBUG
            CCL_Log.Message( "v" + CCLVersion + " (debug)" );
#else
            CCL_Log.Message( "v" + CCLVersion );
#endif
        }

        #endregion

        #region Mod Validation

        void                                ValidateMods()
        {
            for( int i = 0; i < ModHelperDefs.Count; i++ ){
                var ModHelperDef = ModHelperDefs[ i ];
                if( !ModHelperDef.IsValid )
                {
                    // Don't do anything special with broken mods
                    ModHelperDefs.Remove( ModHelperDef );
                    continue;
                }
            }
        }

        #endregion

        #region Research Validation

        bool                                ValidateResearch()
        {
            // Make sure the hidden research exists
            if( Research.Locker == null )
            {
                CCL_Log.Error( "Missing research locker!", "Advanced Research" );
                return false;
            }

            // Validate each advanced research def
            for( int i = 0; i < AdvancedResearch.Count; i++ ){
                var Advanced = AdvancedResearch[ i ];

                if( !Advanced.IsValid )
                {
                    // Remove projects with errors from list of usable projects
                    AdvancedResearch.Remove( Advanced );
                    CCL_Log.Error( "Pruning " + Advanced.defName, "Advanced Research" );
                    i--;
                    continue;
                }

                if( Advanced.IsLockedOut() )
                {
                    // Remove locked out projects
                    AdvancedResearch.Remove( Advanced );
                    i--;
                    continue;
                }
            }

            // All research left is valid
            return true;
        }

        #endregion

        #region Map Component Injection

        void                                InjectMapComponents()
        {
            // Inject the map components into the game
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.MapComponentsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectMapComponents() )

                    ModHelperDef.InjectMapComponents();
                    if( ModHelperDef.MapComponentsInjected )
                    {
                        CCL_Log.Message( "Injected MapComponents", ModHelperDef.ModName );
                    }
                    else
                    {
                        CCL_Log.Message( "Error injecting MapComponents", ModHelperDef.ModName );
                    }
                }
            }
        }

        #endregion

        #region Designator Injection

        void                                InjectDesignators()
        {
            // Inject the designators into the categories
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.DesignatorsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectDesignators() )

                    ModHelperDef.InjectDesignators();
                    if( ModHelperDef.DesignatorsInjected )
                    {
                        CCL_Log.Message( "Injected Designators", ModHelperDef.ModName );
                    }
                    else
                    {
                        CCL_Log.Message( "Error injecting Designators", ModHelperDef.ModName );
                    }
                }
            }
        }

        #endregion

        #region ThingComp Injection

        void                                InjectThingComps()
        {
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.ThingCompsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectThingComps() )

                    ModHelperDef.InjectThingComps();
                    if( ModHelperDef.ThingCompsInjected )
                    {
                        CCL_Log.Message( "Injected ThingComps", ModHelperDef.ModName );
                    }
                    else
                    {
                        CCL_Log.Message( "Error injecting ThingComps", ModHelperDef.ModName );
                    }
                }
            }
        }

        #endregion

        #region Special Injection

        void                                InjectSpecials()
        {
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.SpecialsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectSpecials() )

                    ModHelperDef.InjectSpecials();
                    if( ModHelperDef.SpecialsInjected )
                    {
                        CCL_Log.Message( "Injected Specials", ModHelperDef.ModName );
                    }
                    else
                    {
                        CCL_Log.Message( "Error in Special Injections", ModHelperDef.ModName );
                    }
                }
            }
        }

        #endregion

        #region Post Load Injection

        void                                InjectPostLoaders()
        {
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.PostLoadersInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectPostLoaders() )

                    ModHelperDef.InjectPostLoaders();
                    if( ModHelperDef.PostLoadersInjected )
                    {
                        CCL_Log.Message( "Injected Post Loaders", ModHelperDef.ModName );
                    }
                    else
                    {
                        CCL_Log.Message( "Error in Post Load Injections", ModHelperDef.ModName );
                    }
                }
            }
        }

        #endregion

        #region Enable CCL Buildings

        public void                         EnableCCLBuildings()
        {
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( ModHelperDef.UsesGenericHoppers )
                {
                    // Mod wants generic hoppers
                    CCL_Log.Message( "Enabling hoppers", ModHelperDef.ModName );
                    EnableGenericHoppers();
                }
            }
        }

        private bool                        hoppersEnabled = false;
        private void                        EnableGenericHoppers()
        {
            if( hoppersEnabled )
            {
                // Only enable them once
                return;
            }
            var hoppers = DefDatabase<ThingDef>.AllDefs.Where( d => (
                ( d.thingClass == typeof( Building_Hopper ) )&&
                ( d.HasComp( typeof( CompHopper ) ) )
            ) ).ToList();
            foreach( var hopper in hoppers )
            {
                hopper.researchPrerequisite = null;
            }
            hoppersEnabled = true;
        }

        #endregion

    }

}
