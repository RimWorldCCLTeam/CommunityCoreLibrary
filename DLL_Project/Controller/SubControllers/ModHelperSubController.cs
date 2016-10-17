//#define CCL_SHOULD_BE_FIRST
// Can't actually be the first in the load order as we are overriding some XML
// and loading first would revert the changes back to the default values from core.

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
	internal class ModHelperSubController : SubController
	{

        private const int                   ModIndexInvalid = -1;

        private const int                   ModIndexOne = 0;
        private const int                   ModIndexTwo = 1;

#if CCL_SHOULD_BE_FIRST
        private const int                   RequiredCCLModIndex = ModIndexOne;
        private const int                   RequiredCoreModIndex = ModIndexTwo;
#else
        private const int                   RequiredCCLModIndex = ModIndexTwo;
        private const int                   RequiredCoreModIndex = ModIndexOne;
#endif

        private const string                RequiredCCLModIndexString = RequiredCCLModIndex == ModIndexOne ? "first" : "second";
        private const string                RequiredCCLModRelativeIndexString = RequiredCCLModIndex == ModIndexOne ? "before" : "after";
        private const string                RequiredCoreModIndexString = RequiredCCLModIndex == ModIndexOne ? "second" : "first";

        private static int                  CurentCCLModIndex = ModIndexInvalid;
        private static int                  CurrentCoreModIndex = ModIndexInvalid;

        public override string              Name => "Mod Helper";

		// Override sequence priorities
		public override int                 ValidationPriority      => 100;
        public override int                 InitializationPriority  => 50;

		// Validate ModHelperDefs, CCL load order, CCL versioning
		public override bool                Validate()
		{
			// Hopefully...
			var stringBuilder = new StringBuilder();
			var rVal = true;

			CCL_Log.CaptureBegin( stringBuilder );

			// Limit one ModHelperDef per mod
			// Create the ordered list by inserting dummies for mods which don't have one
			//var allMods = LoadedModManager.RunningMods.ToList();
            var allMods = ModsConfig.ActiveModsInLoadOrder.ToList();

#if DEVELOPER
            // Dump list of mods
            string dump = "Initial mod load order:\n";
            for( int i = 0; i < allMods.Count; ++i )
            {
                dump += string.Format( "\t[{0} - {1}] - {2}\n",
                                      i,
                                      allMods[ i ].Identifier,
                                      allMods[ i ].Name
                                     );
            }
            CCL_Log.Write( dump );
#endif

            // Find Core and CCL in the mod order
            CurrentCoreModIndex = ModIndexInvalid;
            CurentCCLModIndex = ModIndexInvalid;
            for( int i = 0; i < allMods.Count; ++i )
            {
                var mod = allMods[ i ];
                if( mod.Identifier == ModContentPack.CoreModIdentifier )
                {
                    CurrentCoreModIndex = i;
                }
                if( mod.Name == Controller.Data.UnityObjectName )
                {
                    Controller.Data.cclModIdentifier = mod.Identifier;
                    CurentCCLModIndex = i;
                }
            }
            if( CurrentCoreModIndex == ModIndexInvalid )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "Unable to find '{0}' in mod load order!", ModContentPack.CoreModIdentifier ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            else if( CurrentCoreModIndex != RequiredCoreModIndex )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "'{0}' must be {1} in mod load order!", ModContentPack.CoreModIdentifier, RequiredCoreModIndexString ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            if( CurentCCLModIndex == ModIndexInvalid )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "Unable to find '{0}' in mod load order!", Controller.Data.UnityObjectName ) );
                //rVal = false; // Don't throw as an error, will be caught special
            }
            else if( CurentCCLModIndex != RequiredCCLModIndex )
            {
                LongEventHandler.ExecuteWhenFinished( ShowLoadOrderWindow );
                CCL_Log.Error( string.Format( "'{0}' must be {3} in mod load order, immediately {4} '{1}'! :: Current position is #{2}", Controller.Data.UnityObjectName, ModContentPack.CoreModIdentifier, ( CurentCCLModIndex + 1 ).ToString(), RequiredCCLModIndexString, RequiredCCLModRelativeIndexString ) );
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
                        var contentPack = Find_Extensions.ModByName( mod.Name, true );
                        modHelperDef.mod = contentPack;
    					// Insert it into it's ordered place in the lists
                        Controller.Data.Mods.Insert( i, contentPack );
    					Controller.Data.ModHelperDefs.Insert( i, modHelperDef );
    					// Add a dictionary entry
                        Controller.Data.DictModHelperDefs.Add( contentPack, modHelperDef );
    				}
    			}
    			// Should now be a complete pair of lists in mod load order
    			// as well as a dictionary of mods and their defs

#if DEVELOPER
                //Dump ordered list of mods and their defs
                dump = "Mod load order:\n";
                for( int i = 0; i < Controller.Data.Mods.Count; i++ )
                {
                    dump += string.Format( "\t[{0} - {1}] - {2} - {3}{4}\n",
                                          i,
                                          Controller.Data.Mods[ i ].Identifier,
                                          Controller.Data.Mods[ i ].Name,
                                          Controller.Data.ModHelperDefs[ i ].defName,
                                          ( Controller.Data.ModHelperDefs[ i ].dummy ? " - dummy" : ""
                                          ) );
                }
                CCL_Log.Write( dump );
#endif

    			if( rVal )
    			{
                    ModContentPack CCL_Mod = Controller.Data.Mods[ CurentCCLModIndex ];
    				ModHelperDef CCL_HelperDef = Find_Extensions.ModHelperDefForMod( CCL_Mod );

    				// Validate xml version with assembly version
    				var vc = Version.Compare( CCL_HelperDef.minCCLVersion );
    				if( vc != Version.VersionCompare.ExactMatch )
    				{
                        CCL_Log.Error( string.Format( "Version mismatch for {0}!", Controller.Data.UnityObjectName ), "ModHelperDef" );
    					rVal = false;
    				}

    				// CCL in the correct load order position and def version matches library
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
            for( int index = 0; index < allMods.Count; ++index )
            {   // Activate CCL and core
                var mod = allMods[ index ];
                if( index == RequiredCCLModIndex )
                {   // Activate CCL
                    ModsConfig.SetActive( Controller.Data.cclModIdentifier, true );
                }
                else if( index == RequiredCoreModIndex )
                {   // Activate core mod
                    ModsConfig.SetActive( ModContentPack.CoreModIdentifier, true );
                }
            }
            // Activate everything else in the same order
            for( int index = 0; index < allMods.Count; ++index )
            {
                var mod = allMods[ index ];
                if(
                    ( mod.Name != ModContentPack.CoreModIdentifier )&&
                    ( mod.Name != Controller.Data.cclModIdentifier )
                )
                {
                    ModsConfig.SetActive( mod.Identifier, true );
                }
            }
            // Now save the config
            ModsConfig.Save();
        }

	}

}
