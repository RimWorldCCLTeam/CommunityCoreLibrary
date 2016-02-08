using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static partial class Validation
    {

        public static class Mods
        {

            public static bool              Validate()
            {
                // Hopefully...
                var rVal = true;

                // Limit one ModHelperDef per mod
                // Create the ordered list by inserting dummies for mods which don't have one
                var allMods = LoadedModManager.LoadedMods.ToList();

                for( int i = 0; i < allMods.Count; i++ )
                {
                    var modHelperDef = (ModHelperDef) null;
                    var mod = allMods[ i ];
                    var modHelperDefs = Find_Extensions.DefListOfTypeForMod<ModHelperDef>( mod );
                    if( !modHelperDefs.NullOrEmpty() )
                    {
                        if( modHelperDefs.Count > 1 )
                        {
                            CCL_Log.Error( "Multiple ModHelperDefs detected", mod.name );
                            rVal = false;
                        }
                        else
                        {
                            // Validate the def
                            modHelperDef = modHelperDefs.First();
                            if( !modHelperDef.IsValid )
                            {
                                // Don't do anything special with broken mods
                                CCL_Log.Error( "ModHelperDef is invalid", mod.name );
                                rVal = false;
                            }
                            else if( !modHelperDef.dummy )
                            {
                                // Don't show validation message for dummy defs
                                CCL_Log.TraceMod(
                                    modHelperDef,
                                    Verbosity.Validation,
                                    "Passed validation, requesting v" + modHelperDef.version,
                                    "Mod Helper"
                                );
                            }
                        }
                    }
                    else if( rVal == true )
                    {
                        // Doesn't exist, create a dummy for logging but only
                        // create if we're not just checking for remaining errors
                        modHelperDef = new ModHelperDef();
                        modHelperDef.defName = mod.name + "_ModHelperDef";
                        modHelperDef.version = Version.Minimum.ToString();
                        modHelperDef.ModName = mod.name;
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
                    dump += "\t[" + i + "] - " + Controller.Data.Mods[ i ].name + " - " + Controller.Data.ModHelperDefs[ i ].defName + ( Controller.Data.ModHelperDefs[ i ].dummy ? " - dummy" : "" ) + "\n";
                }
                CCL_Log.Write( dump );
#endif
                
                // Return true if all mods OK, false if any failed validation
                return rVal;
            }

        }

    }

}
