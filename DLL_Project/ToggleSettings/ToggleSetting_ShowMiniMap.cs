using System;

using RimWorld;
using Verse;

using CommunityCoreLibrary.MiniMap;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowMiniMap : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return MiniMapController.visible;
            }
            set
            {
                MiniMapController.visible = value;
            }
        }

    }

}
