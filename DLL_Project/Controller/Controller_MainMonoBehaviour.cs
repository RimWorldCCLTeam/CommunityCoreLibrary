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

    [StaticConstructorOnStartup]
    public class MainMonoBehaviour : MonoBehaviour
    {

        #region Instance Data

        private static bool                 gameValid;

        private static int                  ticks;

        private List<SubController>         UpdateControllers = null;

        private static MethodInfo           _DoPlayLoad;
        private static bool                 queueRecovering = false;
        private static bool                 queueLoadAllPlayData = false;

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

            // Detour Verse.PlayDataLoader.LoadAllPlayData
            MethodInfo Verse_PlayDataLoader_LoadAllPlayData = typeof( PlayDataLoader ).GetMethod( "LoadAllPlayData", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_PlayDataLoader_LoadAllPlayData = typeof( Detour._PlayDataLoader ).GetMethod( "_LoadAllPlayData", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( Verse_PlayDataLoader_LoadAllPlayData, CCL_PlayDataLoader_LoadAllPlayData );

            // Detour Verse.PlayDataLoader.ClearAllPlayData
            MethodInfo Verse_PlayDataLoader_ClearAllPlayData = typeof( PlayDataLoader ).GetMethod( "ClearAllPlayData", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_PlayDataLoader_ClearAllPlayData = typeof( Detour._PlayDataLoader ).GetMethod( "_ClearAllPlayData", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( Verse_PlayDataLoader_ClearAllPlayData, CCL_PlayDataLoader_ClearAllPlayData );

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
            UnityEngine.Object.DontDestroyOnLoad( Controller.Data.UnityObject );

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

        #region Reload RimWorld

        public static void ReloadRimWorld()
        {
            Controller.Data.ReloadingPlayData = true;
            LongEventHandler.QueueLongEvent(
                ReloadQueue,
                "",
                true,
                null
            );
        }

        private static void ReloadQueue()
        {
            ClearAllPlayData();
            if( queueLoadAllPlayData )
            {
                LoadAllPlayData( queueRecovering );
            }
            Controller.Data.ReloadingPlayData = false;
        }

        #endregion

        #region Restart RimWorld

        internal static void RestartRimWorld()
        {
            var args = Environment.GetCommandLineArgs();
            var commandLine = "\"" + args[ 0 ] + "\"";
            var arguements = string.Empty;
            for( int index = 1; index < args.GetLength( 0 ); ++index )
            {
                if( index > 1 )
                {
                    arguements += " ";
                }
                arguements += args[ index ];
            }
#if DEVELOPER
            Log.Message( "Restarting RimWorld:\n" + commandLine + " " + arguements );
#endif
            Process.Start( commandLine, arguements );
            Root.Shutdown();
        }

        #endregion

        #region Play Data

        private static void ClearAllPlayData()
        {
            LanguageDatabase.Clear();
            LoadedModManager.ClearDestroy();
            foreach( Type genericParam in GenTypes.AllSubclasses( typeof( Def ) ) )
            {
                GenGeneric.InvokeStaticMethodOnGenericType( typeof( DefDatabase<> ), genericParam, "Clear" );
            }
            ThingCategoryNodeDatabase.Clear();
            BackstoryDatabase.Clear();
            SolidBioDatabase.Clear();
            PlayDataLoader.loaded = false;
        }

        internal static void QueueLoadAllPlayData( bool recovering = false )
        {
            queueRecovering = recovering;
            queueLoadAllPlayData = true;
        }

        private static void DoPlayLoad()
        {
            if( _DoPlayLoad == null )
            {
                _DoPlayLoad = typeof( PlayDataLoader ).GetMethod( "DoPlayLoad", BindingFlags.Static | BindingFlags.NonPublic );
            }
            _DoPlayLoad.Invoke( null, null );
        }

        internal static void LoadAllPlayData( bool recovering = false )
        {
            if( PlayDataLoader.loaded )
            {
                Log.Error( "Loading play data when already loaded. Call ClearAllPlayData first." );
            }
            else
            {
                queueRecovering = false;
                queueLoadAllPlayData = false;

                DeepProfiler.Start( "LoadAllPlayData" );
                try
                {
                    DoPlayLoad();
                }
                catch( Exception ex )
                {
                    if( !Prefs.ResetModsConfigOnCrash )
                        throw;
                    else if( recovering )
                    {
                        Log.Warning( "Could not recover from errors loading play data. Giving up." );
                        throw;
                    }
                    else
                    {
                        IEnumerable<InstalledMod> activeMods = ModsConfig.ActiveMods;
                        if( Enumerable.Count<InstalledMod>( activeMods ) == 1 && Enumerable.First<InstalledMod>( activeMods ).IsCoreMod )
                        {
                            throw;
                        }
                        else
                        {
                            Log.Warning( "Caught exception while loading play data but there are active mods other than Core. Resetting mods config and trying again.\nThe exception was: " + (object)ex );
                            try
                            {
                                PlayDataLoader.ClearAllPlayData();
                            }
                            catch
                            {
                                Log.Warning( "Caught exception while recovering from errors and trying to clear all play data. Ignoring it.\nThe exception was: " + (object)ex );
                            }
                            ModsConfig.Reset();
                            CrossRefLoader.Clear();
                            PostLoadInitter.Clear();
                            PlayDataLoader.LoadAllPlayData( true );
                            return;
                        }
                    }
                }
                finally
                {
                    DeepProfiler.End();
                }
                PlayDataLoader.loaded = true;
                if( !recovering )
                    return;
                Log.Message( "Successfully recovered from errors and loaded play data." );
                DelayedErrorWindowRequest.Add( Translator.Translate( "RecoveredFromErrorsText" ), Translator.Translate( "RecoveredFromErrorsDialogTitle" ) );
            }
        }

        #endregion

        #region Mono Callbacks

        public void                         Start()
        {
            enabled = true;
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
            // enabled = ( ( gameValid )&&( level == 1 ) ) ? true : false;
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
                if( subsys.ValidationPriority != SubController.DontProcessThisPhase )
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
                else
                {
                    subsys.State = SubControllerState.Validated;
                }
            }

            LongEventHandler.SetCurrentEventText( "Initializing".Translate() );

            // Initialize all sub-systems
            subControllers.Sort( (x,y) => ( x.InitializationPriority > y.InitializationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( subsys.InitializationPriority != SubController.DontProcessThisPhase )
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
                else
                {
                    subsys.State = SubControllerState.Ok;
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
                if( subsys.InitializationPriority != SubController.DontProcessThisPhase )
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
                if( subsys.UpdatePriority != SubController.DontProcessThisPhase )
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

        }

        #endregion

    }

}
