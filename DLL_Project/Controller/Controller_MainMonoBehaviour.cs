using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Controller
{

    [StaticConstructorOnStartup]
    public class MainMonoBehaviour : MonoBehaviour
    {

        #region Instance Data

        private static bool                 gameValid;

        private static int                  ticks;

        private List<SubController>         UpdateControllers = null;

        #endregion

        #region Static Constructor

        static                              MainMonoBehaviour()
        {
            PreLoad();
        }

#if DEVELOPER
        /* Should be a static but can't */  ~MainMonoBehaviour()
        {
            // https://www.youtube.com/watch?v=jyaLZHiJJnE
            CCL_Log.CloseStream();

        }

#endif

        #endregion

        #region Preloader

        private static void                 PreLoad()
        {
            // This is a pre-start sequence to hook some deeper level functions.
            // These functions can be hooked later but it would be after the sequence
            // of operations which call them is complete.
            // This is done in the class constructor of a ThingDef override class so the
            // class PostLoad is not detoured while it's being executed for this object.

            // Log CCL version
            Version.Log();

            bool InjectionsOk = true;
            StringBuilder stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            // Create system controllers
            Controller.Data.SubControllers = new SubController[]
            {
                new Controller.LibrarySubController(),
                new Controller.ResearchSubController(),
                new Controller.InjectionSubController(),
                new Controller.HelpSubController()
            };

            // Detour RimWorld.MainMenuDrawer.MainMenuOnGUI
            MethodInfo RimWorld_MainMenuDrawer_MainMenuOnGUI = typeof( MainMenuDrawer ).GetMethod( "MainMenuOnGUI", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_MainMenuDrawer_MainMenuOnGUI = typeof( Detour._MainMenuDrawer ).GetMethod( "_MainMenuOnGUI", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( RimWorld_MainMenuDrawer_MainMenuOnGUI, CCL_MainMenuDrawer_MainMenuOnGUI );

            // Detour RimWorld.MainMenuDrawer.DoMainMenuButtons
            MethodInfo RimWorld_MainMenuDrawer_DoMainMenuButtons = typeof( MainMenuDrawer ).GetMethod( "DoMainMenuButtons", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_MainMenuDrawer_DoMainMenuButtons = typeof( Detour._MainMenuDrawer ).GetMethod( "_DoMainMenuButtons", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( RimWorld_MainMenuDrawer_DoMainMenuButtons, CCL_MainMenuDrawer_DoMainMenuButtons );

            Controller.Data.UnityObject = new GameObject( Controller.Data.UnityObjectName );
            Controller.Data.UnityObject.AddComponent< Controller.MainMonoBehaviour >();

            CCL_Log.Message(
                "Queueing Library Initialization",
                "PreLoader"
            );

            LongEventHandler.QueueLongEvent( Initialize, "LibraryStartup", true, null );

            CCL_Log.CaptureEnd(
                stringBuilder,
                InjectionsOk ? "Initialized" : "Errors during injection"
            );
            CCL_Log.Trace(
                Verbosity.Injections,
                stringBuilder.ToString(),
                "PreLoader" );
        }

        #endregion

        #region Mono Callbacks

        public void                         Start()
        {
        }

        public void                         FixedUpdate()
        {
            ticks++;
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
                LongEventHandler.QueueLongEvent( ReIntialize, "Initializing", true, null );
            }
            if( Scribe.mode != LoadSaveMode.Inactive )
            {
                // Do nothing while a save/load sequence is happening
                return;
            }

            LongEventHandler.ExecuteWhenFinished( UpdateSubControllers );
        }

        public void                         OnLevelWasLoaded( int level )
        {
            // Enable the frame update when the game and map are valid
            // Level 1 means we're in gameplay.
            enabled = ( ( gameValid )&&( level == 1 ) ) ? true : false;
        }

        #endregion

        #region Long Event Handlers

        public static void                         Initialize()
        {
            //enabled = false;
            gameValid = false;

            var subControllers = Controller.Data.SubControllers.ToList();
            if( subControllers.NullOrEmpty() )
            {
                CCL_Log.Error( "SubControllers array is empty!" );
                return;
            }

            LongEventHandler.SetCurrentEventText( "LibraryValidation".Translate() );

            // Validate all subs-systems
            subControllers.Sort( (x,y) => ( x.ValidationPriority > y.ValidationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( !subsys.Validate() )
                {
                    CCL_Log.Error( subsys.strReturn, subsys.Name + " :: Validation"  );
                    return;
                }
                if( subsys.strReturn != string.Empty )
                {
                    CCL_Log.Message( subsys.strReturn, subsys.Name + " :: Validations" );
                }
            }

            LongEventHandler.SetCurrentEventText( "Initializing".Translate() );

            // Initialize all sub-systems
            subControllers.Sort( (x,y) => ( x.InitializationPriority > y.InitializationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( !subsys.Initialize() )
                {
                    CCL_Log.Error( subsys.strReturn, subsys.Name + " :: Initialization" );
                    return;
                }
                if( subsys.strReturn != string.Empty )
                {
                    CCL_Log.Message( subsys.strReturn, subsys.Name + " :: Initialization" );
                }
            }

            CCL_Log.Message( "Initialized" );

            // Yay!
            gameValid = true;
            //enabled = true;
            ticks = 0;
        }

        public void                         ReIntialize()
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
                        CCL_Log.Error( subsys.strReturn, subsys.Name + " :: Reinitialization" );
                        gameValid = false;
                        enabled = false;
                        return;
                    }
                    if( subsys.strReturn != string.Empty )
                    {
                        CCL_Log.Message( subsys.strReturn, subsys.Name + " :: Reinitialization" );
                    }
                }
            }
            ticks = 0;
        }

        public void                         UpdateSubControllers()
        {
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
                    ( subsys.IsHashIntervalTick( ticks ) )
                )
                {
                    if( !subsys.Update() )
                    {
                        CCL_Log.Error( subsys.strReturn, subsys.Name + " :: Update" );
                        return;
                    }
                    if( subsys.strReturn != string.Empty )
                    {
                        CCL_Log.Message( subsys.strReturn, subsys.Name + " :: Update" );
                    }
                }
            }

        }

        #endregion

    }

}
