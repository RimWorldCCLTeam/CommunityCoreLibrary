using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _PlayDataLoader
    {

        internal static void _ClearAllPlayData()
        {
            Controller.Data.RequireRestart = true;
#if DEBUG
            if(
                ( !Controller.Data.WarnedAboutRestart )||
                ( Controller.Data.PlayWithoutRestart )
            )
            {
#endif
#if RELEASE
                if( !Controller.Data.WarnedAboutRestart )
                {
                    Window_WarnRestart.messageKey = "WarnAboutRestart";
                }
                else
                {
                    Window_WarnRestart.messageKey = "ReallyWarnAboutRestart";
                }
#else
                Window_WarnRestart.messageKey = "WarnAboutRestartDebug";
#endif
                Window_WarnRestart.callbackBeforeRestart = null;
                Find.WindowStack.Add( new Window_WarnRestart() );
#if DEBUG
            }
#endif
        }

        internal static void _LoadAllPlayData( bool recovering = false )
        {
            if( Controller.Data.RestartWarningIsOpen )
            {
                Controller.MainMonoBehaviour.QueueLoadAllPlayData( recovering );
            }
        }

    }

}
