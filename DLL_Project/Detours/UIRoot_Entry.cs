using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace CommunityCoreLibrary.Detour
{

    internal static class _UIRoot_Entry
    {

        #region Detoured Methods

        internal static bool        _ShouldShowMainMenuGUI_get( this UIRoot_Entry entry )
        {
            if( 
                ( Controller.Data.RestartWarningIsOpen )||
                ( Controller.Data.ReloadingPlayData )
            )
            {
                return false;
            }
            WindowStack windowStack = Find.WindowStack;
            for( int index = 0; index < windowStack.Count; ++index )
            {
                if(
                    ( windowStack[ index ].layer == WindowLayer.Dialog )&&
                    ( !( windowStack[index] is EditWindow_Log ) )
                )
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

    }

}
