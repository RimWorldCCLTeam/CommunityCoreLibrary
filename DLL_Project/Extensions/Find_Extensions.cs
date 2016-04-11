using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Find_Extensions
    {

        // Suffixes which (may) need to be removed to find the mod for a def
        private static List<string> def_suffixes = new List<string>(){
            "_Frame",
            "_Blueprint",
            "_Blueprint_Install",
            "_Corpse",
            "_Leather",
            "_Meat"
        };

        // This is a safe method of fetching a map component of a specified type
        // If an instance of the component doesn't exist or map isn't loaded it will return null
        public static T                     MapComponent<T>() where T : MapComponent
        {
            if (
                ( Find.Map == null )||
                ( Find.Map.components.NullOrEmpty() )
            )
            {
                return null;
            }
            return Find.Map.components.FirstOrDefault( c => c.GetType() == typeof( T ) ) as T;
        }

        public static MapComponent          MapComponent( Type t )
        {
            if (
                ( Find.Map == null )||
                ( Find.Map.components.NullOrEmpty() )
            )
            {
                return null;
            }
            return Find.Map.components.FirstOrDefault( c => c.GetType() == t );
        }

        // Get the def set of a specific type for a specific mod
        /*
        public static ModDefSet<T>          DefSetOfTypeForMod<T>( LoadedMod mod ) where T : Def, new()
        {
            if( mod == null )
            {
                return null;
            }
            var defSets = typeof( LoadedMod ).GetField( "defSets", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( mod ) as Dictionary<System.Type, ModDefSet>;
            ModDefSet modDefSet = (ModDefSet) null;
            if( !defSets.TryGetValue( typeof (T), out modDefSet ) )
            {
                return null;
            }
            return (ModDefSet<T>) modDefSet;
        }
        */

        // Get the def list of a specific type for a specific mod
        public static List<T>               DefListOfTypeForMod<T>( LoadedMod mod ) where T : Def, new()
        {
            if( mod == null )
            {
                return null;
            }
            var list = mod.AllDefs.Where(def => (
               ( def.GetType() == typeof( T ) )
            ) ).ToList();
            return list.ConvertAll( def => ( (T) def ) );
        }

        // Search for mod by def
        public static LoadedMod             ModByDef<T>( T def, int InitialIndex = -1 ) where T : Def, new()
        {
            var allMods = Controller.Data.Mods;
            int Start = InitialIndex;
            if(
                ( Start < 0 )||
                ( Start >= allMods.Count )
            )
            {
                Start = allMods.Count - 1;
            }
            var defName = def.defName;
            if(
                ( def is ThingDef )||
                ( def is TerrainDef )
            )
            {
                // Trim suffix off of thing and terrain defs
                foreach( var suffix in def_suffixes )
                {
                    if( defName.EndsWith( suffix ) )
                    {
                        defName = defName.Remove( def.defName.Length - suffix.Length );
                        break;
                    }
                }
            }
            // Search for def in mod list
            for( int i = Start; i >= 0; i-- )
            {
                var defs = DefListOfTypeForMod<T>( allMods[ i ] );
                if( defs.Exists( d => d.defName == defName ) )
                {
                    return allMods[ i ];
                }
            }
            // None found
            return null;
        }

        // Get the specific mod of a specific def of a specific type.
        // Scans in reverse order (last mod fist) for a specific def
        // of a specific type.
        // Optional InitialIndex to be used to continue scanning to find
        // all instances of the same def in all mods.
        public static LoadedMod             ModByDefOfType<T>( string defName, int InitialIndex = -1 ) where T : Def, new()
        {
            if( defName.NullOrEmpty() )
            {
                return null;
            }
            var allMods = Controller.Data.Mods;
            int Start = InitialIndex;
            if(
                ( Start < 0 )||
                ( Start >= allMods.Count )
            )
            {
                Start = allMods.Count - 1;
            }
            for( int i = Start; i >= 0; i-- )
            {
                var defs = DefListOfTypeForMod<T>( allMods[ i ] );
                if( defs.Exists( d => d.defName == defName ) )
                {
                    return allMods[ i ];
                }
            }
            return null;
        }

        // Get a mods index in the load order by mod
        public static int                   ModIndexByMod( LoadedMod mod )
        {
            if( mod == null )
            {
                return -1;
            }
            var allMods = Controller.Data.Mods;
            for( int i = 0; i < allMods.Count; i++ )
            {
                if( allMods[ i ] == mod )
                {
                    return i;
                }
            }
            return -1;
        }

        // Get a mod by index in the load order
        public static LoadedMod             ModByModIndex( int Index )
        {
            var allMods = Controller.Data.Mods;
            if(
                ( Index < 0 )||
                ( Index > allMods.Count - 1 )
            )
            {
                return null;
            }
            return allMods[ Index ];
        }

        // Get the ModHelperDef for a mod
        public static ModHelperDef          ModHelperDefForMod( LoadedMod mod )
        {
            if( mod == null )
            {
                return null;
            }
            var rVal = (ModHelperDef)null;
            if( Controller.Data.DictModHelperDefs.TryGetValue( mod, out rVal ) )
            {
                return rVal;
            }
            return null;
        }

        // Get the hightest tracing level for global functions
        public static Verbosity             HightestVerbosity
        {
            get
            {
                if( Controller.Data.Trace_Current_Mod != null )
                {
                    // Return the current mods tracing level
#if RELEASE
                    if( Controller.Data.Trace_Current_Mod.Verbosity > Verbosity.Default )
                    {
                        // Highest level in release is Validation
                        return Verbosity.Default;
                    }
#endif
                    return Controller.Data.Trace_Current_Mod.Verbosity;
                }
#if RELEASE
                // Highest (default) level in release is Validation
                return Verbosity.Default;
#elif DEBUG
                // Find the highest level in all mods
                var highestVerbosity = Verbosity.Default;
                foreach( var modHelperDef in Controller.Data.ModHelperDefs )
                {
                    if( modHelperDef.Verbosity > highestVerbosity )
                    {
                        highestVerbosity = modHelperDef.Verbosity;
                    }
                }
                return highestVerbosity;
#endif
            }
        }

    }

}
