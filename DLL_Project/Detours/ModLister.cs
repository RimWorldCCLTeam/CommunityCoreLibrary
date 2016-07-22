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

        internal static List<ModMetaData>   GetMods()
        {
            if( _mods == null )
            {
                _mods = typeof( ModLister ).GetField( "mods", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (List<ModMetaData>)_mods.GetValue( null );
        }

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
