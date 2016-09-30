using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _ModLister
    {

        internal static FieldInfo           _mods;

        static                              _ModLister()
        {
            _mods = typeof( ModLister ).GetField( "mods", Controller.Data.UniversalBindingFlags );
            if( _mods == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'mods' in 'ModLister'",
                    "Detour.ModLister" );
            }
        }

        internal static List<ModMetaData>   GetMods()
        {
            return (List<ModMetaData>)_mods.GetValue( null );
        }

        [DetourClassMethod( typeof( ModLister ), "InstalledModsListHash" )]
        internal static int _InstalledModsListHash( bool activeOnly )
        {
            int num = 0;
            List<ModMetaData> mods;
            if( activeOnly )
            {
                mods = ModsConfig.ActiveModsInLoadOrder.ToList();
            }
            else
            {
                mods = GetMods();
            }
            for( int index = 0; index < mods.Count; ++index )
            {
                var hash = mods[ index ].GetHashCode();
                var orHash = ( hash << ( index & 15 ) ) | ( hash >> ( index & 15 ) );
                num ^= orHash;
            }
            return num;
        }

    }

}
