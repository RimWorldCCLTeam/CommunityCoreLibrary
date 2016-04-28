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

        internal static MethodInfo __DoPlayLoad;
        internal static bool CallLoadAllPlayerDataWhenFinished;
        internal static bool queueRecovering;

        internal static void _DoPlayLoad()
        {
            if( __DoPlayLoad == null )
            {
                __DoPlayLoad = typeof( PlayDataLoader ).GetMethod( "DoPlayLoad", BindingFlags.Static | BindingFlags.NonPublic );
            }
            __DoPlayLoad.Invoke( null, null );
        }

        internal static void _ClearAllPlayData()
        {
            Controller.Data.RequireRestart = true;
            if( !Controller.Data.ContinueWithoutRestart )
            {
                queueRecovering = false;
                CallLoadAllPlayerDataWhenFinished = false;
                Find.WindowStack.Add( new Window_WarnRestart() );
            }
        }

        internal static void _LoadAllPlayData( bool recovering = false )
        {
            if( Controller.Data.RestartWarningIsOpen )
            {
                queueRecovering = recovering;
                CallLoadAllPlayerDataWhenFinished = true;
                return;
            }
            if( PlayDataLoader.loaded )
            {
                Log.Error( "Loading play data when already loaded. Call ClearAllPlayData first." );
            }
            else
            {
                DeepProfiler.Start( "LoadAllPlayData" );
                try
                {
                    _DoPlayLoad();
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

    }

}
