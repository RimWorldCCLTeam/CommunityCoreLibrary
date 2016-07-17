using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

	/// <summary>
	/// This controller validates ModHelperDefs (one per mod, version, etc)
	/// </summary>
	internal class LibrarySubController : SubController
	{

        private static int coreModIndex = -1;
        private static int cclModIndex = -1;

        public override string              Name => "Library Core";

		// Override sequence priorities
		public override int                 ValidationPriority      => 100;
        public override int                 InitializationPriority  => 50;

		// Validate ModHelperDefs, CCL load order, CCL versioning
		public override bool Validate()
		{
			// Hopefully...
			var stringBuilder = new StringBuilder();
			var rVal = true;

			CCL_Log.CaptureBegin( stringBuilder );

			// Limit one ModHelperDef per mod
			// Create the ordered list by inserting dummies for mods which don't have one
			var allMods = LoadedModManager.RunningMods.ToList();

            // Find Core and CCL in the mod order
            coreModIndex = -1;
            cclModIndex = -1;
            for( int i = 0; i < allMods.Count; ++i )
            {
                ModContentPack mod = allMods[ i ];
                if( mod.Identifier == ModContentPack.CoreModIdentifier )
                {
                    coreModIndex = i;
                }
                if( mod.Name == Controller.Data.UnityObjectName )
                {
                    Controller.Data.cclModIdentifier = mod.Identifier;
                    cclModIndex = i;
                }
            }
            if( coreModIndex == -1 )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "Unable to find '{0}' in mod load order!", ModContentPack.CoreModIdentifier ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            else if( coreModIndex != 0 )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "'{0}' must be first in mod load order!", ModContentPack.CoreModIdentifier ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            if( cclModIndex == -1 )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "Unable to find '{0}' in mod load order!", Controller.Data.UnityObjectName ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            else if( cclModIndex != 1 )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "'{0}' must be second in mod load order, immediately after '{1}'! :: Current position is #{2}", Controller.Data.UnityObjectName, ModContentPack.CoreModIdentifier, ( cclModIndex + 1 ).ToString() ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            else if( Controller.Data.cclModIdentifier == string.Empty )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "Unable to identify '{0}' in mod load order!", Controller.Data.UnityObjectName ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            if( rVal )
            {
    			for( int i = 0; i < allMods.Count; i++ )
    			{
    				var modHelperDef = (ModHelperDef)null;
    				var mod = allMods[ i ];
    				var modHelperDefs = Find_Extensions.DefListOfTypeForMod<ModHelperDef>( mod );
    				if( !modHelperDefs.NullOrEmpty() )
    				{
    					if( modHelperDefs.Count > 1 )
    					{
                            CCL_Log.Error( string.Format( "'{0}' has multiple ModHelperDefs!", mod.Name ) );
    						rVal = false;
    					}
    					else
    					{
    						// Validate the def
    						modHelperDef = modHelperDefs.First();
    						if( !modHelperDef.IsValid )
    						{
    							// Don't do anything special with broken mods
                                CCL_Log.Error( string.Format( "ModHelperDef for '{0}' is invalid!", mod.Name ) );
    							rVal = false;
    						}
    						else if( !modHelperDef.dummy )
    						{
    							// Don't show validation message for dummy defs
                                CCL_Log.Message( string.Format( "{0} :: Passed validation, requesting v{1}", mod.Name, modHelperDef.minCCLVersion ), "ModHelperDef" );
    						}
    					}
    				}
    				else if( rVal == true )
    				{
    					// Doesn't exist, create a dummy for logging but only
    					// create if we're not just checking for remaining errors
    					modHelperDef = new ModHelperDef();
    					modHelperDef.defName = mod.Name + "_ModHelperDef";
    					modHelperDef.minCCLVersion = Version.Minimum.ToString();
    					modHelperDef.ModName = mod.Name;
    					modHelperDef.Verbosity = Verbosity.NonFatalErrors;
    					modHelperDef.dummy = true;
    				}
    				if( rVal == true )
    				{
    					// No errors, def is valid or a dummy
    					// Associate the def with the mod (the dictionary is to go the other way)
    					modHelperDef.mod = mod;
    					// Insert it into it's ordered place in the lists
    					Controller.Data.Mods.Insert( i, mod );
    					Controller.Data.ModHelperDefs.Insert( i, modHelperDef );
    					// Add a dictionary entry
    					Controller.Data.DictModHelperDefs.Add( mod, modHelperDef );
    				}
    			}
    			// Should now be a complete pair of lists in mod load order
    			// as well as a dictionary of mods and their defs

#if DEVELOPER
                //Dump ordered list of mods and their defs
                string dump = "Mod load order:\n";
                for( int i = 0; i < Controller.Data.Mods.Count; i++ )
                {
                    dump += string.Format( "\t[{0} - {1}] - {2} - {3}{4}\n", i, Controller.Data.Mods[ i ].Identifier, Controller.Data.Mods[ i ].Name, Controller.Data.ModHelperDefs[ i ].defName, ( Controller.Data.ModHelperDefs[ i ].dummy ? " - dummy" : "" ) );
                }
                CCL_Log.Write( dump );
#endif
    			if( rVal )
    			{
                    ModContentPack CCL_Mod = Controller.Data.Mods[ cclModIndex ];
    				ModHelperDef CCL_HelperDef = Find_Extensions.ModHelperDefForMod( CCL_Mod );

    				// Validate xml version with assembly version
    				var vc = Version.Compare( CCL_HelperDef.minCCLVersion );
    				if( vc != Version.VersionCompare.ExactMatch )
    				{
                        CCL_Log.Error( string.Format( "Version mismatch for {0}!", Controller.Data.UnityObjectName ), "ModHelperDef" );
    					rVal = false;
    				}

    				// CCL rank is #2 in load order and def version matches library
    				Controller.Data.cclMod = CCL_Mod;
    				Controller.Data.cclHelperDef = CCL_HelperDef;

    			}

            }

			// Should be all good or up until the first error encountered
            CCL_Log.CaptureEnd(
				stringBuilder,
				rVal ? "Validated" : "Errors during validation"
			);
			strReturn = stringBuilder.ToString();
            // Return true if all mods OK, false if any failed validation
			State = rVal ? SubControllerState.Validated : SubControllerState.ValidationError;
			return rVal;
		}

		public override bool Initialize()
		{
			// Don't need to keep checking on this controller
            if( !MCMHost.InitializeHosts( false ) )
            {
                strReturn = "Errors initializing Mod Configuration Menus";
                State = SubControllerState.InitializationError;
                return false;
            }
			strReturn = "Mod Configuration Menus initialized";
			State = SubControllerState.Hybernating;
			return true;
		}

        private static void ShowLoadOrderWindow()
        {
            Controller.Data.RequireRestart = true;
            Window_WarnRestart.messageKey = "BadLoadOrder";
            Window_WarnRestart.callbackBeforeRestart = CorrectLoadOrderBeforeRestart;
            Find.WindowStack.Add( new Window_WarnRestart() );
        }

        private static void CorrectLoadOrderBeforeRestart()
        {
            var allMods = LoadedModManager.RunningMods.ToList();
            // Deactivate all mods
            foreach( var mod in allMods )
            {
                ModsConfig.SetActive( mod.Name, false );
            }
            // Activate core first
            ModsConfig.SetActive( ModContentPack.CoreModIdentifier, true );
            if( cclModIndex != -1 )
            { // Activate CCL second
                ModsConfig.SetActive( Controller.Data.cclModIdentifier, true );
            }
            // Activate everything else in the same order
            foreach( var mod in allMods )
            {
                if(
                    ( mod.Name != ModContentPack.CoreModIdentifier )&&
                    ( mod.Name != Controller.Data.cclModIdentifier )
                )
                {
                    ModsConfig.SetActive( mod.Name, true );
                }
            }
            // Now save the config
            ModsConfig.Save();
        }

	}

}
