using System;
using System.Linq;
using System.Reflection;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{
    
    internal static class Library
    {
        
        internal static void                PreLoad()
        {
            // This is a pre-start sequence to hook some deeper level functions.
            // These functions can be hooked later but it would be after the sequence
            // of operations which call them is complete.

#if DEVELOPER
            // Open a log file for CCL specific output
            // https://www.youtube.com/watch?v=jyaLZHiJJnE
            if( CCL_Log.OpenStream() == null )
            {
                Log.Error(
                    string.Format(
                        "Unable to open file stream for {0}!",
                        Controller.Data.UnityObjectName
                    )
                );
            }
#endif

            // Log CCL version
            Version.Log();

            bool libraryValid = true;
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            if( Controller.Data.Assembly_CSharp == null )
            {
                libraryValid = false;
                CCL_Log.Error(
                    "Unable to load 'Assembly-CSharp.dll'",
                    "Library Validation"
                );
            }
            if( Controller.Data.Assembly_CCL == null )
            {
                libraryValid = false;
                CCL_Log.Error(
                    "Unable to load 'Community Core Library.dll'",
                    "Library Validation"
                );
            }

            // Find and create sub-controllers
            if( libraryValid )
            {
                libraryValid &= Controller.SubControllers.Create();
            }

            // Inject all CCL detours that are ImmediatelyOnDLLLoad
            if( libraryValid )
            {
                libraryValid &= InjectionSubController.TrySequencedInjectorsOnLoad( Controller.Data.Assembly_CCL );
            }

            if( libraryValid )
            {
                LongEventHandler.ExecuteWhenFinished( Initialize );
            }

            CCL_Log.CaptureEnd(
                stringBuilder,
                libraryValid ? "Validated" : "Errors during validation"
            );
            CCL_Log.Trace(
                Verbosity.Injections,
                stringBuilder.ToString(),
                "Library :: Validation" );

            Controller.Data.LibraryValid = libraryValid;
        }

        internal static void                Initialize()
        {
            if( !Controller.Data.LibraryValid )
            {
                CCL_Log.Error( "Library did not validate properly but 'Library.Initialize' was called!" );
                return;
            }
            var gameObject = new GameObject( Controller.Data.UnityObjectName );
            if( gameObject == null )
            {
                CCL_Log.Error(
                    "Unable to create GameObject",
                    "Library.Initialize"
                );
                return;
            }
            else
            {
                if( gameObject.AddComponent< Controller.MainMonoBehaviour >() == null )
                {
                    CCL_Log.Error(
                        "Unable to create MonoBehaviour",
                        "Library.Initialize"
                    );
                    return;
                }
                else
                {
                    UnityEngine.Object.DontDestroyOnLoad( gameObject );
                    Controller.Data.UnityObject = gameObject;
                }
            }

            CCL_Log.Message( "Queueing Library Startup" );
            LongEventHandler.QueueLongEvent( Startup, "LibraryStartup", true, null );
        }

        internal static void                Startup()
        {
            if( !Controller.Data.LibraryValid )
            {
                CCL_Log.Error(
                    "Library did not validate properly but 'Library.Startup' was called!",
                    "Library.Startup"
                );
                return;
            }

            LongEventHandler.SetCurrentEventText( "LibraryValidation".Translate() );
            if( !Controller.SubControllers.Valid() )
            {
                return;
            }

            LongEventHandler.SetCurrentEventText( "Initializing".Translate() );
            if( !Controller.SubControllers.Initialize() )
            {
                return;
            }

            CCL_Log.Message( "Initialized" );

            // Yay!
            Controller.Data.LibraryInitialized = true;
            Controller.Data.LibraryOk = true;
            Controller.Data.LibraryTicks = 0;
        }

        internal static bool                IsInGoodState
        {
            get
            {
                return(
                    ( Controller.Data.LibraryValid )&&
                    ( Controller.Data.LibraryInitialized )&&
                    ( Controller.Data.LibraryOk )
                );
            }
        }

        internal static void                Restart()
        {
            if( !IsInGoodState )
            {
                CCL_Log.Error( "Library is not in a valid state but 'Library.Restart' was called!" );
                return;
            }

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
                            Controller.Data.LibraryOk = false;
                            return;
                        }
                        if( subsys.strReturn != string.Empty )
                        {
                            CCL_Log.Message( subsys.strReturn, subsys.Name + " :: Reinitialization" );
                        }
                    }
                }
            }
            Controller.Data.LibraryTicks = 0;
        }

    }

}
