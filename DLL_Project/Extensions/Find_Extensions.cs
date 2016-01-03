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

        // This is a safe method of fetching a map component of a specified type
        // If an instance of the component doesn't exist or map isn't loaded it will return null
        public static MapComponent          MapComponent( Type t )
        {
            if(
                ( Find.Map != null )&&
                ( !Find.Map.components.NullOrEmpty() )&&
                ( Find.Map.components.Exists( c => c.GetType() == t ) )
            )
            {
                return Find.Map.components.Find( c => c.GetType() == t );
            }
            return null;
        }

        // Get the def set of a specific type for a specific mod
        public static ModDefSet<T>          DefSetOfModOfType<T>( LoadedMod mod ) where T : Def, new()
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

        // Get the def list of a specific type for a specific mod
        public static List<T>               DefListOfModOfType<T>( LoadedMod mod ) where T : Def, new()
        {
            if( mod == null )
            {
                return null;
            }
            var modDefSet = DefSetOfModOfType<T>( mod );
            if( modDefSet == null )
            {
                return null;
            }
            return modDefSet.AllDefs.ToList();
        }

        // Get the specific mod of a specific def of a specific type
        public static LoadedMod             ModByDefOfType<T>( string defName, int InitialIndex = -1 ) where T : Def, new()
        {
            if( defName.NullOrEmpty() )
            {
                return null;
            }
            var allMods = LoadedModManager.LoadedMods.ToList();
            int Start = InitialIndex;
            if(
                ( Start < 0 )||
                ( Start >= allMods.Count )
            )
            {
                Start = allMods.Count - 1;
            }
            for( int i = Start; i > 0; i-- )
            {
                var defSet = DefSetOfModOfType<T>( allMods[ i ] );
                if( defSet.DefNamed( defName, false ) != null )
                {
                    return allMods[i];
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
            var allMods = LoadedModManager.LoadedMods.ToList();
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
            var allMods = LoadedModManager.LoadedMods.ToList();
            if(
                ( Index < 0 )||
                ( Index > allMods.Count - 1 )
            )
            {
                return null;
            }
            return allMods[ Index ];
        }

        // Get a mods ModHelperDef by mod
        public static ModHelperDef           ModHelperDefByMod( LoadedMod mod )
        {
            if( mod == null )
            {
                return null;
            }
            var modHelperDefs = Find_Extensions.DefListOfModOfType<ModHelperDef>( mod );
            if( !modHelperDefs.NullOrEmpty() )
            {
                return modHelperDefs.First();
            }
            return null;
        }

    }

}
