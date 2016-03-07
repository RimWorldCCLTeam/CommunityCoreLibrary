using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

/*
    TODO:  Alpha 13 API change

    Can't change yet otherwise existing saves will get null errors or name clashes

namespace CommunityCoreLibrary.Controller
{
    internal class LibraryController : SubController
*/

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller validates ModHelperDefs (one per mod, version, etc)
    /// </summary>
	internal class LibrarySubController : SubController
	{

        public override string              Name
        {
            get
            {
                return "Library Core";
            }
        }

        // Override sequence priorities
        public override int                 ValidationPriority
        {
            get
            {
                return 100;
            }
        }

        // Validate ModHelperDefs, CCL load order, CCL versioning
		public override bool                Validate ()
		{
			// Hopefully...
            var stringBuilder = new StringBuilder();
			var rVal = true;

            stringBuilder.AppendLine( "Validation" );
            CCL_Log.CaptureBegin( stringBuilder );

			// Limit one ModHelperDef per mod
			// Create the ordered list by inserting dummies for mods which don't have one
			var allMods = LoadedModManager.LoadedMods.ToList();

			for( int i = 0; i < allMods.Count; i++ )
            {
				var modHelperDef = (ModHelperDef)null;
				var mod = allMods [i];
				var modHelperDefs = Find_Extensions.DefListOfTypeForMod<ModHelperDef> (mod);
				if( !modHelperDefs.NullOrEmpty() )
                {
					if( modHelperDefs.Count > 1 )
                    {
                        stringBuilder.Append( "\t" + mod.name );
                        CCL_Log.AppendSectionNewLine( ref stringBuilder, "Multiple ModHelperDefs detected" );
						rVal = false;
					} else {
						// Validate the def
						modHelperDef = modHelperDefs.First ();
						if (!modHelperDef.IsValid) {
							// Don't do anything special with broken mods
                            stringBuilder.Append( "\t" + mod.name );
                            CCL_Log.AppendSectionNewLine( ref stringBuilder, "ModHelperDef is invalid" );
							rVal = false;
						} else if (!modHelperDef.dummy) {
							// Don't show validation message for dummy defs
                            stringBuilder.Append( "\t" + mod.name );
                            CCL_Log.AppendSection( ref stringBuilder, "ModHelperDef" );
                            CCL_Log.AppendSectionNewLine( ref stringBuilder, "Passed validation, requesting v" + modHelperDef.version );
						}
					}
				} else if (rVal == true) {
					// Doesn't exist, create a dummy for logging but only
					// create if we're not just checking for remaining errors
					modHelperDef = new ModHelperDef ();
					modHelperDef.defName = mod.name + "_ModHelperDef";
					modHelperDef.version = Version.Minimum.ToString ();
					modHelperDef.ModName = mod.name;
					modHelperDef.Verbosity = Verbosity.NonFatalErrors;
					modHelperDef.dummy = true;
				}
				if (rVal == true) {
					// No errors, def is valid or a dummy
					// Associate the def with the mod (the dictionary is to go the other way)
					modHelperDef.mod = mod;
					// Insert it into it's ordered place in the lists
					Controller.Data.Mods.Insert (i, mod);
					Controller.Data.ModHelperDefs.Insert (i, modHelperDef);
					// Add a dictionary entry
					Controller.Data.DictModHelperDefs.Add (mod, modHelperDef);
				}
			}
			// Should now be a complete pair of lists in mod load order
			// as well as a dictionary of mods and their defs

#if DEVELOPER
            //Dump ordered list of mods and their defs
            string dump = "Mod load order:\n";
            for( int i = 0; i < Controller.Data.Mods.Count; i++ )
            {
                dump += "\t[" + i + "] - " + Controller.Data.Mods[ i ].name + " - " + Controller.Data.ModHelperDefs[ i ].defName + ( Controller.Data.ModHelperDefs[ i ].dummy ? " - dummy" : "" ) + "\n";
            }
            CCL_Log.Write( dump );
#endif
            if( rVal )
            {
                // Find CCL in the mod order
                bool found = false;
                int index = -1;
                for( int i = 0; i < allMods.Count; ++i )
                {
                    LoadedMod mod = allMods[ i ];
                    if( mod.name == Controller.Data.UnityObjectName )
                    {
                        found = true;
                        index = i;
                        break;
                    }
                }
                if( !found )
                {
                    stringBuilder.Append( "\tUnable to find mod '" );
                    stringBuilder.Append( Controller.Data.UnityObjectName );
                    stringBuilder.AppendLine( "' in mod load order!" );
                    rVal = false;
                }
                else if( index != 1 )
                {
                    stringBuilder.Append( "\t'" );
                    stringBuilder.Append( Controller.Data.UnityObjectName );
                    stringBuilder.AppendLine( "' must be second in mod load order, immediately after 'Core'! :: Current position is #" + ( index + 1 ).ToString() );
                    rVal = false;
                }

                LoadedMod CCL_Mod = Controller.Data.Mods[ index ];
                ModHelperDef CCL_HelperDef = Find_Extensions.ModHelperDefForMod( CCL_Mod );

                // Validate xml version with assembly version
                var vc = Version.Compare( CCL_HelperDef.version );
                if( vc != Version.VersionCompare.ExactMatch )
                {
                    stringBuilder.AppendLine( "\tModHelperDef version mismatch for Community Core Library!" );
                    rVal = false;
                }

                // CCL rank is #2 in load order and def version matches library
                Controller.Data.cclMod = CCL_Mod;
                Controller.Data.cclHelperDef = CCL_HelperDef;

            }

            // Should be all good or up until the first error encountered
            CCL_Log.CaptureEnd( stringBuilder );
            strReturn = stringBuilder.ToString();

			// Return true if all mods OK, false if any failed validation
            State = rVal ? SubControllerState.Validated : SubControllerState.ValidationError;
			return rVal;
		}

        public override bool                Initialize ()
        {
            // Don't need to keep checking on this controller
            strReturn = string.Empty;
            State = SubControllerState.Hybernating;
            return true;
        }

	}

}
