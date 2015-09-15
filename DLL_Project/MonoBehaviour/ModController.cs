using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
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

            // Do special injectors
            InjectSpecials();

            // Validate advanced research defs
            if( ValidateResearch() )
            {
                ResearchController.InitComponent();
            }

            // Auto-generate help menus
            HelpController.Initialize();

            CCL_Log.Message( "Initialized" );

            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if( ReadyForDesignatorInjection() )
            {
                InjectDesignators();
            }
            if( ReadyForMapComponentInjection() )
            {
                InjectMapComponents();
            }
            if (ReadyForThingCompsInjection())
            {
                InjectThingComps();
            }
        }

        public void OnLevelWasLoaded()
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
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            CCLVersion = assembly.GetName().Version;
#if DEBUG
            Log.Message( "Community Core Library v" + CCLVersion + " (debug)" );
#else
            Log.Message( "Community Core Library v" + CCLVersion );
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

        #region Research Verification

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

        bool                                ReadyForMapComponentInjection()
        {
            return (
                ( Find.Map != null )&&
                ( Find.Map.components != null )
            );
        }

        void                                InjectMapComponents()
        {
            // Inject the map components into the game
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.MapComponentsInjected )
                {
                    CCL_Log.Message( "Injecting MapComponents for " + ModHelperDef.ModName );
                    ModHelperDef.InjectMapComponents();
                }
            }
        }

        #endregion

        #region Designator Injection

        bool                                ReadyForDesignatorInjection()
        {
            var DesignatorCategoryDefs = DefDatabase<DesignationCategoryDef>.AllDefs;

            foreach( var DesignatorCategoryDef in DesignatorCategoryDefs )
            {
                if( DesignatorCategoryDef.resolvedDesignators == null )
                {
                    return false;
                }
            }

            return true;
        }

        void                                InjectDesignators()
        {
            // Inject the designators into the categories
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.DesignatorsInjected )
                {
                    CCL_Log.Message( "Injecting Designators for " + ModHelperDef.ModName );
                    ModHelperDef.InjectDesignators();
                }
            }
        }
        
        #endregion

        #region ThingComp Injection
        
        bool                                ReadyForThingCompsInjection()
        {
            foreach ( var ModHelperDef in ModHelperDefs )
            {
                if ( !ModHelperDef.ThingCompsInjected )
                {
                    return true;
                }
            }
            return false;
        }

        void                                InjectThingComps()
        {
            foreach (var ModHelperDef in ModHelperDefs)
            {
                if ( !ModHelperDef.ThingCompsInjected )
                {
                    CCL_Log.Message( "Injecting ThingComps for " + ModHelperDef.ModName );
                    ModHelperDef.InjectThingComps();
                }
            }
        }

        #endregion

        #region Special Injection

        void                                InjectSpecials()
        {
            foreach (var ModHelperDef in ModHelperDefs)
            {
                if ( !ModHelperDef.SpecialsInjected )
                {
                    CCL_Log.Message( "Special Injections for " + ModHelperDef.ModName );
                    ModHelperDef.InjectSpecials();
                }
            }
        }

        #endregion

    }

}
