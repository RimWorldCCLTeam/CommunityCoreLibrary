using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class Controller : MonoBehaviour
    {
        
        public readonly string              GameObjectName = "Community Core Library";

        Version                             CCLVersion;
        List< CCLVersionDef >               CCLMods = new List< CCLVersionDef >();

        public void                         Start()
        {
            enabled = false;

            // Check versions of mods and throw error to the user if the
            // mod version requirement is higher than the installed version

            CCLMods = DefDatabase< CCLVersionDef >.AllDefs.ToList();

            GetCCLVersion();
            CheckModVersionRequirements();

            CheckAdvancedResearch();

            Log.Message( "Community Core Library :: Initialized" );

            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if( ( !ReadyForInjection() )||
                ( !NeedsComponentInjection() ) )
            {
                return;
            }

            InjectComponents();
        }

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

        void                                CheckModVersionRequirements()
        {
            if( ( CCLMods == null )||
                ( CCLMods.Count < 1 ) )
                return;

            var throwError = false;
            var errors = "Community Core Library dependency error:";

            foreach( var CCLMod in CCLMods )
            {
                var modVersion = new Version( CCLMod.version );
                if( modVersion > CCLVersion ){
                    errors += "\n\t" + CCLMod.ModName + " requires v" + modVersion;
                    throwError = true;
                }
            }

            if( throwError )
            {
                Log.Error( errors );
            }

        }

        #endregion

        #region Data Verification

        void                                CheckAdvancedResearch()
        {
            // Make sure the hidden research exists
            if( Research.Locker == null )
            {
                Log.Error( "Community Core Library :: Advanced Research :: Missing Research.Locker!" );
            }

            // Get the [advanced] research defs
            var advancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();

            if( ( advancedResearch == null )&&
                ( advancedResearch.Count == 0 ) )
            {
                return;
            }

            // Validate each advanced research def
            foreach( var Advanced in advancedResearch )
            {
                // Validate recipes
                if( Advanced.IsRecipeToggle() )
                {
                    // Make sure thingDefs are of the appropriate type (has ITab_Bills)
                    foreach( var buildingDef in Advanced.thingDefs )
                    {
                        if( buildingDef.thingClass.GetInterface( "IBillGiver" ) == null ){
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + buildingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + Advanced.defName + " ) - Must implement \"IBillGiver\"" );
                        }
                    }

                }

                // Validate plant sowTags
                if( Advanced.IsPlantToggle() )
                {
                    // Make sure things are of the appropriate class (Plant)
                    foreach( var plantDef in Advanced.thingDefs )
                    {
                        if( plantDef.thingClass != typeof( Plant ) )
                        {
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + plantDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + Advanced.defName + " ) - Must be <thingClass> \"Plant\"" );
                        }
                    }

                    // Make sure sowTags are valid (!null or empty)
                    for( int i = 0; i < Advanced.sowTags.Count; i++ )
                    {
                        var sowTag = Advanced.sowTags[ i ];
                        if( string.IsNullOrEmpty( sowTag ) )
                        {
                            Log.Error( "Community Core Library :: Advanced Research :: sowTags( index = " + i + " ) resolved to null in AdvancedResearchDef( " + Advanced.defName + " )" );
                        }
                    }
                }

                // Validate buildings
                if( Advanced.IsBuildingToggle() )
                {
                    // Make sure thingDefs are of the appropriate type (has proper designationCategory)
                    foreach( var buildingDef in Advanced.thingDefs )
                    {
                        if( ( string.IsNullOrEmpty( buildingDef.designationCategory ) )||
                            ( buildingDef.designationCategory.ToLower() == "none" ) )
                        {
                            Log.Error( "Community Core Library :: Advanced Research :: thingDef( " + buildingDef.defName + " ) is of inappropriate type in AdvancedResearchDef( " + Advanced.defName + " ) - <designationCategory> must not be null or \"None\"" );
                        }
                    }
                }
            }

            // All advanced research checked, no log errors means it's all good
        }

        #endregion

        #region Component Injection

        bool                                NeedsComponentInjection()
        {
            return ( !Find.Map.components.Exists( c => c.GetType() == typeof( ResearchControl ) ) );
        }

        bool                                ReadyForInjection()
        {
            return (
                ( Find.Map != null )&&
                ( Find.Map.components != null )
            );
        }

        void                                InjectComponents()
        {
            Find.Map.components.Add( new ResearchControl() );
            Log.Message( "Community Core Library :: Advanced Research :: Injected into existing save" );
        }

        #endregion

    }

}
