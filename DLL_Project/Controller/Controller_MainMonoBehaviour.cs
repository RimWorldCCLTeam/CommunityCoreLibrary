using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public class MainMonoBehaviour : MonoBehaviour
    {

        #region Instance Data

        private static bool                 gameValid;
        private static bool                 mapValid;

        #endregion

        #region Mono Callbacks

        // TODO:  Check if this method exists
        public void                         Stop()
        {
#if DEVELOPER
            CCL_Log.CloseStream();
#endif
        }

        public void                         Initialize()
        {
            // This is a pre-start sequence to hook some deeper level functions
            // These functions can be hooked later but it would be after the sequence of
            // operations which call them is complete

            // Detour Verse.ThingDef.PostLoad
            MethodInfo Verse_ThingDef_PostLoad = typeof( ThingDef ).GetMethod( "PostLoad", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ThingDef_PostLoad = typeof( Detour._ThingDef ).GetMethod( "_PostLoad", BindingFlags.Static | BindingFlags.NonPublic );
            Detours.TryDetourFromTo( Verse_ThingDef_PostLoad, CCL_ThingDef_PostLoad );

        }

        public void                         Start()
        {
            enabled = false;
            gameValid = false;

            // Log CCL version
            Version.Log();

            // Check versions of mods and throw errors to the user if the
            // mod version requirement is higher than the installed version
            if( !Validation.Mods.Validate() )
            {
                CCL_Log.Error( "Initialization Error!", "Mod Validation" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            // Check CCL load rank and version
            if( !Validation.Core.Validate() )
            {
                CCL_Log.Error( "Initialization Error!", "Library Validation" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;

            }

            // Validate advanced research defs
            if( !Validation.Research.Validate() )
            {
                CCL_Log.Error( "Initialization Error!", "Research Validation" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            // Do special injections
            if( !Resources.Injectors.Special.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "Special Injection" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            // Inject ThingComps into ThingsWithComps
            if( !Resources.Injectors.ThingComps.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "ThingComp Injection" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            // Enables CCL resources (hoppers, etc) for mods wanting them
            if( !Resources.Enable() )
            {
                CCL_Log.Error( "Initialization Error!", "Resource Injection" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            // Initialize research controller
            // TODO: Alpha 13 API change
            //Controller.Research.Initialize();
            if( !ResearchController.Initialize() )
            {
                CCL_Log.Error( "Initialization Error!", "Advanced Research" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            // Auto-generate help [category] defs
            if( !Controller.Help.Initialize() )
            {
                CCL_Log.Error( "Initialization Error!", "Help Controller" );
#if DEVELOPER
                CCL_Log.CloseStream();
#endif
                return;
            }

            CCL_Log.Message( "Initialized" );

            // Yay!
            gameValid = true;
            mapValid = true;
            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if(
                ( !gameValid )||
                ( !mapValid )||
                ( Game.Mode != GameMode.MapPlaying )||
                ( Find.Map == null )||
                ( Find.Map.components == null )
            )
            {
                // Do nothing until the game has fully loaded the map and is ready to play
                return;
            }

            // Do post-load injections
            if( !Resources.Injectors.PostLoad.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "Post-Load Injection" );
                mapValid = false;
                enabled = false;
                return;
            }

            // Inject map components
            if( !Resources.Injectors.MapComponents.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "Map Component Injection" );
                mapValid = false;
                enabled = false;
                return;
            }

            // Designator injections
            if( !Resources.Injectors.Designators.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "Desginator Injection" );
                mapValid = false;
                enabled = false;
                return;
            }

            // Post-load injections complete, stop calling this
            CCL_Log.Message( "Post-load injection successful" );
            enabled = false;
        }

        public void                         OnLevelWasLoaded( int level )
        {
            // Enable the frame update when the game and map are valid
            // Level 1 means we're in gameplay.
            enabled = ( ( gameValid )&&( mapValid )&&( level == 1 ) ) ? true : false;
        }

        #endregion

    }

}
