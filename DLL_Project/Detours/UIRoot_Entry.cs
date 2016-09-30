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

    internal class _UIRoot_Entry : UIRoot_Entry
    {

        #region Detoured Methods

        [DetourClassProperty( typeof( UIRoot_Entry ), "ShouldDoMainMenu" )]
        internal static bool        _ShouldDoMainMenu
        {
            get
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
        }

        #endregion

    }

}
