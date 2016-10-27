using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _PlayDataLoader
    {

        private static MethodInfo           _DoPlayLoad;
        internal static bool                queueRecovering = false;
        internal static bool                queueLoadAllPlayData = false;

        private static FieldInfo            PlayDataLoader_loaded;

        #region Constructor

        static                              _PlayDataLoader()
        {
            PlayDataLoader_loaded = typeof( Verse.PlayDataLoader ).GetField( "loadedInt", Controller.Data.UniversalBindingFlags );
            if( PlayDataLoader_loaded == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'loadedInt' in 'PlayDataLoader'",
                    "Detour.PlatDataLoader" );
            }
            _DoPlayLoad = typeof( Verse.PlayDataLoader ).GetMethod( "DoPlayLoad", Controller.Data.UniversalBindingFlags );
            if( _DoPlayLoad == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'DoPlayLoad' in 'PlayDataLoader'",
                    "Detour.PlatDataLoader" );
            }
        }

        #endregion

        #region Detoured Methods

        [DetourClassMethod( typeof( Verse.PlayDataLoader ), "ClearAllPlayData", InjectionSequence.DLLLoad )]
        internal static void _ClearAllPlayData()
        {
            Controller.Data.RequireRestart = true;
#if DEBUG
            if(
                ( !Controller.Data.WarnedAboutRestart )||
                ( Controller.Data.PlayWithoutRestart )
            )
            {
#endif
#if RELEASE
                if( !Controller.Data.WarnedAboutRestart )
                {
                    Window_WarnRestart.messageKey = "WarnAboutRestart";
                }
                else
                {
                    Window_WarnRestart.messageKey = "ReallyWarnAboutRestart";
                }
#else
                Window_WarnRestart.messageKey = "WarnAboutRestartDebug";
#endif
                Window_WarnRestart.callbackBeforeRestart = null;
                Find.WindowStack.Add( new Window_WarnRestart() );
#if DEBUG
            }
#endif
        }

        [DetourClassMethod( typeof( Verse.PlayDataLoader ), "LoadAllPlayData", InjectionSequence.DLLLoad )]
        internal static void _LoadAllPlayData( bool recovering = false )
        {
            if( Controller.Data.RestartWarningIsOpen )
            {
                QueueLoadAllPlayDataInt( recovering );
            }
        }

        #endregion

        #region Internal Methods

        internal static void ClearAllPlayDataInt()
        {
            LanguageDatabase.Clear();
            // A14 - ModContentPackManager was removed?
            LoadedModManager.ClearDestroy();
            foreach( Type genericParam in GenTypes.AllSubclasses( typeof( Def ) ) )
            {
                GenGeneric.InvokeStaticMethodOnGenericType( typeof( DefDatabase<> ), genericParam, "Clear" );
            }
            ThingCategoryNodeDatabase.Clear();
            BackstoryDatabase.Clear();
            SolidBioDatabase.Clear();

            PlayDataLoader_loaded.SetValue( null, false );
        }

        internal static void QueueLoadAllPlayDataInt( bool recovering = false )
        {
            queueRecovering = recovering;
            queueLoadAllPlayData = true;
        }

        private static void DoPlayLoadInt()
        {
            _DoPlayLoad.Invoke( null, null );
        }

        internal static void LoadAllPlayDataInt( bool recovering = false )
        {
            if( Verse.PlayDataLoader.Loaded )
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
                    DoPlayLoadInt();
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
                        IEnumerable<ModMetaData> activeMods = ModsConfig.ActiveModsInLoadOrder;
                        if( Enumerable.Count<ModMetaData>( activeMods ) == 1 && Enumerable.First<ModMetaData>( activeMods ).IsCoreMod )
                        {
                            throw;
                        }
                        else
                        {
                            Log.Warning( "Caught exception while loading play data but there are active mods other than Core. Resetting mods config and trying again.\nThe exception was: " + (object)ex );
                            try
                            {
                                Verse.PlayDataLoader.ClearAllPlayData();
                            }
                            catch
                            {
                                Log.Warning( "Caught exception while recovering from errors and trying to clear all play data. Ignoring it.\nThe exception was: " + (object)ex );
                            }
                            ModsConfig.Reset();
                            CrossRefLoader.Clear();
                            PostLoadInitter.Clear();
                            Verse.PlayDataLoader.LoadAllPlayData( true );
                            return;
                        }
                    }
                }
                finally
                {
                    DeepProfiler.End();
                }
                // A14 - PlayDataLoader.loaded is now private, Loaded property is getter only
                PlayDataLoader_loaded.SetValue( null, false );
                if ( !recovering )
                {
                    return;
                }
                Log.Message( "Successfully recovered from errors and loaded play data." );
                DelayedErrorWindowRequest.Add( Translator.Translate( "RecoveredFromErrorsText" ), Translator.Translate( "RecoveredFromErrorsDialogTitle" ) );
            }
        }

        #endregion

    }

}
