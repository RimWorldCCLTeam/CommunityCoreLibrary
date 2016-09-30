using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _PlaySettings
    {

        internal static List<ToggleSettingDef>  toggleSettingDefs;

        static                              _PlaySettings()
        {
            toggleSettingDefs = DefDatabase<ToggleSettingDef>.AllDefs.ToList();
            toggleSettingDefs.Sort( (x, y) => x.order > y.order ? 1 : -1 );
        }

        [DetourClassMethod( typeof( PlaySettings ), "ExposeData" )]
        internal static void                _ExposeData( this PlaySettings _this )
        {
            foreach( var toggleSetting in toggleSettingDefs )
            {
                if( toggleSetting.exposeValue )
                {
                    bool value = toggleSetting.toggleWorker.Value;
                    Scribe_Values.LookValue( ref value, toggleSetting.saveKey, false, false );

                    if( Scribe.mode == LoadSaveMode.LoadingVars )
                    {
                        toggleSetting.toggleWorker.Value = value;
                    }
                }
            }
        }

        [DetourClassMethod( typeof( PlaySettings ), "DoPlaySettingsGlobalControls" )]
        internal static void                _DoPlaySettingsGlobalControls( this PlaySettings _this, WidgetRow row )
        {
            foreach( var toggleSetting in toggleSettingDefs )
            {
                if( toggleSetting.enableButton )
                {
                    bool value = toggleSetting.toggleWorker.Value;

                    row.ToggleableIcon( ref value, toggleSetting.icon, toggleSetting.Label, toggleSetting.soundDef, toggleSetting.tutorTag );

                    if( value == toggleSetting.toggleWorker.Value )
                    {
                        continue;
                    }

                    toggleSetting.toggleWorker.Value = value;
                }
            }
        }

    }

}
