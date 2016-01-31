using System;
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
                return;
            }

            // Validate advanced research defs
            if( !Validation.Research.Validate() )
            {
                CCL_Log.Error( "Initialization Error!", "Research Validation" );
                return;
            }

            // Do special injections
            if( !Resources.Injectors.Special.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "Special Injection" );
                return;
            }

            // Inject ThingComps into ThingsWithComps
            if( !Resources.Injectors.ThingComps.Inject() )
            {
                CCL_Log.Error( "Initialization Error!", "ThingComp Injection" );
                return;
            }

            // Enables CCL resources (hoppers, etc) for mods wanting them
            if( !Resources.Enable() )
            {
                CCL_Log.Error( "Initialization Error!", "Resource Injection" );
                return;
            }

            // Initialize research controller
            // TODO: Alpha 13 API change
            //Controller.Research.Initialize();
            if( !ResearchController.Initialize() )
            {
                CCL_Log.Error( "Initialization Error!", "Advanced Research" );
                return;
            }

            // Auto-generate help [category] defs
            if( !Controller.Help.Initialize() )
            {
                CCL_Log.Error( "Initialization Error!", "Help Controller" );
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
            // Level 1 means we're in gameplay.
            enabled = ( ( gameValid )&&( level == 1 ) ) ? true : false;
        }

        #endregion

    }

}
