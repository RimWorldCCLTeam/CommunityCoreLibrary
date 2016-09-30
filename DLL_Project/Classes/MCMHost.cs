using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class MCMHost : IExposable
    {

        #region Control Constants

        public const string ConfigFilePrefix = "MCM_Data_";
        public const string ConfigFileSuffix = ".xml";

        #endregion

        #region Instance Data

        public string                   Label;
        public ModConfigurationMenu     worker;

        public bool                     OpenedThisSession;

        private string                  _key;

        #endregion

        #region Properties

        public string                   key
        {
            get
            {
                if( string.IsNullOrEmpty( _key ) )
                {
                    if( string.IsNullOrEmpty( worker.InjectionSet.saveKey ) )
                    {
                        _key = Label.Replace( " ", "_" );
                    }
                    else
                    {
                        _key = worker.InjectionSet.saveKey;
                    }
                }
                return _key;
            }
        }

        #endregion

        #region Constructors

        public                          MCMHost()
        {
            this.Label = "?";
            this.worker = null;
            this._key = "";
            this.OpenedThisSession = false;
        }

        public                          MCMHost( string Label, ModConfigurationMenu worker )
        {
            this.Label = Label;
            this.worker = worker;
            this._key = "";
            this.OpenedThisSession = false;
        }

        #endregion

        #region IExposable

        public void                     ExposeData()
        {
            if( worker == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    string.Format( "worker is null in MCMHost for {0}", Label ),
                    "Mod Configuration Menu" );
                return;
            }
            // Call the worker expose data
            worker.ExposeData();
        }

        #endregion

        #region Intialize MCMs

        public static bool InitializeHosts( bool preload = false )
        {
            if( preload )
            {
                Controller.Data.MCMHosts.Clear();
            }

            // Get the mods with config menus
            foreach( var mhd in Controller.Data.ModHelperDefs )
            {
                // Create all the menus for it
                if( !mhd.ModConfigurationMenus.NullOrEmpty() )
                {
                    foreach( var mcm in mhd.ModConfigurationMenus )
                    {
                        if( // Filter out preload during non-preload and non-preload during preload
                           (
                               ( preload )&&
                               ( mcm.preload )
                              )||
                           (
                               ( !preload )&&
                               ( !mcm.preload )
                              )
                          )
                        {
                            var host = Controller.Data.MCMHosts.Find( m => m.worker.InjectionSet == mcm );
                            if( host != null )
                            {   // MCM already created....?
                                CCL_Log.TraceMod(
                                    mhd,
                                    Verbosity.Warnings,
                                    string.Format( "{0} - Tried to create an MCM when an MCM already exists", mcm.mcmClass.ToString() )
                                );
                                continue;
                            }
                            host = new MCMHost();
                            host.Label = mcm.label;
                            host.worker = (ModConfigurationMenu)Activator.CreateInstance( mcm.mcmClass );
                            if( host.worker == null )
                            {
                                CCL_Log.Error( string.Format( "Unable to create instance of {0}", mcm.mcmClass.ToString() ) );
                                return false;
                            }
                            else
                            {   // Initialize, add it to the menu list and then load it's data
                                host.worker.InjectionSet = mcm;
                                host.worker.Initialize();
                                Controller.Data.MCMHosts.Add( host );
                                LoadHostData( host );
                            }
                        }
                    }
                }
            }
            return true;
        }

        #endregion

        #region Load/Save MCM Data

        private static string HostFilePath( MCMHost host )
        {
            // Generate the config file name
            // A14 - ConfigFolderPath became private - take a step back from the public ConfigFilePath
            // - Fluffy
            string filePath = Path.Combine( GenFilePaths_Extensions.ConfigFolderPath, ConfigFilePrefix );
            filePath += host.key;
            filePath += ConfigFileSuffix;
            return filePath;
        }

        public static void LoadHostData( MCMHost host )
        {
            var filePath = HostFilePath( host );

            if( !File.Exists( filePath ) )
            {
                return;
            }

            try
            {
                // Open it for reading
                Scribe.InitLoading( filePath );
                if( Scribe.mode == LoadSaveMode.LoadingVars )
                {
                    // Version check
                    string version = "";
                    Scribe_Values.LookValue<string>( ref version, "ccl_version" );

                    bool okToLoad = true;
                    var result = Version.Compare( version );
                    if( result == Version.VersionCompare.GreaterThanMax )
                    {
                        CCL_Log.Trace(
                            Verbosity.NonFatalErrors,
                            string.Format( "Data for {0} is newer ({1}) than the version you are using ({2}).", host.Label, version, Version.Current.ToString() ),
                            "Mod Configuration Menu" );
                        okToLoad = false;
                    }
                    else if( result == Version.VersionCompare.Invalid )
                    {
                        CCL_Log.Trace(
                            Verbosity.NonFatalErrors,
                            string.Format( "Data for {0} is corrupt and will be discarded", host.Label ),
                            "Mod Configuration Menu" );
                        okToLoad = false;
                    }

                    if( okToLoad )
                    {
                        // Call the worker scribe
                        var args = new object[]
                        {
                            host.Label,
                            host.worker
                        };
                        Scribe_Deep.LookDeep<MCMHost>( ref host, host.key, args );
                    }
                }
            }
            catch( Exception e )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Unexpected error scribing data for mod {0}\n{1}", host.Label, e.ToString() ),
                    "Mod Configuration Menu" );
            }
            finally
            {
                // Finish
                Scribe.FinalizeLoading();
                Scribe.mode = LoadSaveMode.Inactive;
            }
        }

        public static void SaveHostData( MCMHost host )
        {
            var filePath = HostFilePath( host );

            // Open it for writing
            try
            {
                Scribe.InitWriting( filePath, "ModConfigurationData" );
                if( Scribe.mode == LoadSaveMode.Saving )
                {
                    // Write this library version as the one saved with
                    string version = Version.Current.ToString();
                    Scribe_Values.LookValue<string>( ref version, "ccl_version" );

                    // Call the worker scribe
                    Scribe_Deep.LookDeep<MCMHost>( ref host, host.key );
                }
            }
            catch( Exception e )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Unexpected error scribing data for mod {0}\n{1}", host.Label, e.ToString() ),
                    "Mod Configuration Menu" );
            }
            finally
            {
                // Finish
                Scribe.FinalizeWriting();
                Scribe.mode = LoadSaveMode.Inactive;
                Messages.Message( "ModConfigurationSaved".Translate( host.Label ), MessageSound.Standard );
            }
            host.OpenedThisSession = false;
        }

        #endregion

    }

}
