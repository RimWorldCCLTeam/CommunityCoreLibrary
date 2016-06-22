using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using RimWorld;
using Verse;
using UnityEngine;
using CommunityCoreLibrary;

namespace CommunityCoreLibrary.UI
{
    public static class CCL_Widgets
    {
        private static Texture2D _solidWhite;

        public static Texture2D SolidWhite
        {
            get
            {
                if ( _solidWhite == null )
                    _solidWhite = SolidColorMaterials.NewSolidColorTexture( Color.white );
                return _solidWhite;
            }
        }

        public static void DrawBackground( Rect canvas, Color color, float opacity = -1f )
        {
            if ( opacity >= 0f && opacity <= 1f )
                color.a = opacity;

            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture( canvas, SolidWhite );
            GUI.color = oldColor;
        }

        public static void Label( Rect canvas, string text, Color color, string tip = "" )
        {
            Label( canvas, text, color, GameFont.Small, TextAnchor.UpperLeft, tip );
        }

        public static void Label( Rect canvas, string text, GameFont font, string tip = "" )
        {
            Label( canvas, text, Color.white, font, TextAnchor.UpperLeft, tip );
        }

        public static void Label( Rect canvas, string text, TextAnchor anchor, string tip = "" )
        {
            Label( canvas, text, Color.white, GameFont.Small, anchor, tip );
        }

        public static void Label( Rect canvas, string text, string tip = "" )
        {
            Label( canvas, text, Color.white, GameFont.Small, TextAnchor.UpperLeft, tip );
        }

        public static void Label( Rect canvas, string text, Color color, GameFont font, TextAnchor anchor, string tip = "" )
        {
            // cache old font settings
            Color oldColor = GUI.color;
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;

            // set new ones
            GUI.color = color;
            Text.Font = font;
            Text.Anchor = anchor;

            // draw label and tip
            Verse.Widgets.Label( canvas, text );
            if ( !tip.NullOrEmpty() )
                TooltipHandler.TipRegion( canvas, tip );

            // reset settings
            GUI.color = oldColor;
            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
        }

        public static float NoWrapWidth( this string text )
        {
            var oldWW = Text.WordWrap;
            Text.WordWrap = false;
            float result = Text.CalcSize( text ).x;
            Text.WordWrap = oldWW;
            return result;
        }

        public static void Paragraph( ref Vector2 curPos, float width, string label, Color color, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft, string tip = "", float marginBottom = 6f )
        {
            var oldFont = Text.Font;
            Text.Font = font;
            float height = Text.CalcHeight( label, width );

            Label( new Rect( curPos.x, curPos.y, width, height ), label, color, font, anchor, tip );
            Text.Font = oldFont;
            curPos.y += height + marginBottom;
        }

        public static string StringJoin( this IEnumerable<string> enmumerable, string seperator = ", " )
        {
            return String.Join( seperator, enmumerable.ToArray() );
        }

        public static bool ButtonImage( Rect canvas, Texture2D tex, string altString, string tip = "" )
        {
            if( !tip.NullOrEmpty() )
            {
                TooltipHandler.TipRegion( canvas, tip );
            }
            if( tex == null )
            {
                return Widgets.ButtonText( canvas, altString, false, true );
            }
            return Widgets.ButtonImage( canvas, tex );
        }

    }
}
