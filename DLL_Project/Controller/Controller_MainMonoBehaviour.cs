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

        private List<SubController>         UpdateControllers = null;

        #endregion

        #region Mono Callbacks

        public void                         Start()
        {
            enabled = false;
            gameValid = false;

            var subControllers = Controller.Data.SubControllers.ToList();
            if( subControllers.NullOrEmpty() )
            {
                CCL_Log.Error( "SubControllers array is empty!" );
                return;
            }

            // Validate all subs-systems
            subControllers.Sort( (x,y) => ( x.ValidationPriority > y.ValidationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( !subsys.Validate() )
                {
                    CCL_Log.Error( subsys.strReturn, subsys.Name );
                    return;
                }
                if( subsys.strReturn != string.Empty )
                {
                    CCL_Log.Message( subsys.strReturn, subsys.Name );
                }
            }

            // Initialize all sub-systems
            subControllers.Sort( (x,y) => ( x.InitializationPriority > y.InitializationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( !subsys.Initialize() )
                {
                    CCL_Log.Error( subsys.strReturn, subsys.Name );
                    return;
                }
                if( subsys.strReturn != string.Empty )
                {
                    CCL_Log.Message( subsys.strReturn, subsys.Name );
                }
            }

            CCL_Log.Message( "Initialized" );

            // Yay!
            gameValid = true;
            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if(
                ( !gameValid )||
                ( Game.Mode != GameMode.MapPlaying )||
                ( Find.Map == null )||
                ( Find.Map.components == null )
            )
            {
                // Do nothing until the game has fully loaded the map and is ready to play
                return;
            }

            if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                // Call controller Initialize() on game load
                var subControllers = Controller.Data.SubControllers.ToList();
                subControllers.Sort( (x,y) => ( x.InitializationPriority > y.InitializationPriority ) ? -1 : 1 );

                foreach( var subsys in subControllers )
                {
                    if(
                        ( subsys.State >= SubControllerState._BaseOk )&&
                        ( subsys.ReinitializeOnGameLoad )
                    )
                    {
                        if( !subsys.Initialize() )
                        {
                            CCL_Log.Error( subsys.strReturn, subsys.Name );
                            gameValid = false;
                            enabled = false;
                            return;
                        }
                        if( subsys.strReturn != string.Empty )
                        {
                            CCL_Log.Message( subsys.strReturn, subsys.Name );
                        }
                    }
                }

            }
            if( Scribe.mode != LoadSaveMode.Inactive )
            {
                // Do nothing while a save/load sequence is happening
                return;
            }

            if( UpdateControllers == null )
            {
                // Create a list of sub controllers in update order
                UpdateControllers = Controller.Data.SubControllers.ToList();
                UpdateControllers.Sort( (x,y) => ( x.UpdatePriority > y.UpdatePriority ) ? -1 : 1 );
            }

            foreach( var subsys in UpdateControllers )
            {
                if(
                    ( subsys.State == SubControllerState.Ok )&&
                    ( subsys.IsHashIntervalTick( Find.TickManager.TicksGame ) )
                )
                {
                    if( !subsys.Update() )
                    {
                        CCL_Log.Error( subsys.strReturn, subsys.Name );
                        return;
                    }
                    if( subsys.strReturn != string.Empty )
                    {
                        CCL_Log.Message( subsys.strReturn, subsys.Name );
                    }
                }
            }

        }

        public void                         OnLevelWasLoaded( int level )
        {
            // Enable the frame update when the game and map are valid
            // Level 1 means we're in gameplay.
            enabled = ( ( gameValid )&&( level == 1 ) ) ? true : false;
        }

        #endregion

    }

}
