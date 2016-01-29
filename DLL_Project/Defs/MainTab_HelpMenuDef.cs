using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace CommunityCoreLibrary
{
    
    public class MainTab_HelpMenuDef : MainTabDef
    {
        
        public Vector2              windowSize  = new Vector2(MainTabWindow_ModHelp.MinWidth, MainTabWindow_ModHelp.MinHeight);
        public float                listWidth   = MainTabWindow_ModHelp.MinListWidth;
        public bool                 pauseGame   = false;

    }

}
