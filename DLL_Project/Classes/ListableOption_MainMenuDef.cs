using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;
using MainMenuDrawerExt = CommunityCoreLibrary.MainMenuDrawer_Extensions;

namespace CommunityCoreLibrary
{

    public class ListableOption_MainMenu : ListableOption
    {
        
        MainMenuDef     menuDef;

        public ListableOption_MainMenu( MainMenuDef def ) : base( def.label, def.menuWorker.ClickAction )
        {
            menuDef = def;
        }

        public override float DrawOption( Vector2 pos, float width )
        {
            label = menuDef.Label;
            float height = Mathf.Max( minHeight, Text.CalcHeight( label, width ) );
            GUI.color = menuDef.menuWorker.Color;
            if( Widgets.ButtonText( new Rect( pos.x, pos.y, width, height ), label, true, true ) )
            {
                GUI.color = Color.white;
                if( menuDef.closeMainTab )
                {
                    MainMenuDrawerExt.CloseMainTab();
                }
                this.action();
            }
            GUI.color = Color.white;
            return height;
        }

    }

}
