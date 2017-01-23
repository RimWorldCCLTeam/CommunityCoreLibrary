using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public class MainMonoBehaviour : MonoBehaviour
    {

        #region Destructor


#if DEVELOPER
        /* Should be a static but can't */  ~MainMonoBehaviour()
        {
            CCL_Log.CloseStream();

        }

#endif

        #endregion

        #region MonoBehaviour Events

        public void                         Start()
        {
            enabled = true;
            //CCL_Log.Message( "MainMonoBehaviour.OnLevelWasLoaded() - enabled = " + enabled );
        }

        public void                         FixedUpdate()
        {
            //CCL_Log.Message( "MainMonoBehaviour.FixedUpdate() - Library.IsInGoodState = " + Controller.Library.IsInGoodState );

            Controller.Data.LibraryTicks++;

            if(
                ( !Controller.Library.IsInGoodState )||
                ( Current.ProgramState != ProgramState.Playing )||
                ( Find.Maps == null )||
                ( Find.Maps.Any( map => map.components == null ) )
            )
            {
                // Do nothing until the game has fully loaded the map and is ready to play
                return;
            }

            if( Scribe.mode != LoadSaveMode.Inactive )
            {
                // Do nothing while a save/load sequence is happening
                return;
            }

            LongEventHandler.ExecuteWhenFinished( Controller.SubControllers.Update );
        }

        #endregion

    }

}
