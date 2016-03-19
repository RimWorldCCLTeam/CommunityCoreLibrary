using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _MainTabWindow_Menu
    {

        internal static Vector2 _RequestedTabSize( this MainTabWindow_Menu menu )
        {
            var size = new Vector2( 450f, 365f );
            if( Window_ModConfigurationMenu.AnyMenus )
            {
                size.y += 52f;
            }
            return size;
        }

    }

}
