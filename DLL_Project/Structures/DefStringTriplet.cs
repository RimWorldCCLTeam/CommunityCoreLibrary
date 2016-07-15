using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public struct DefStringTriplet
    {
        public Def Def;
        public string Prefix;
        public string Suffix;

        private float _height;
        private bool _heightSet;

        public DefStringTriplet(Def def, string prefix = null, string suffix = null )
        {
            Def = def;
            Prefix = prefix;
            Suffix = suffix;
            _height = 0;
            _heightSet = false;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            if (Prefix != "")
            {
                s.Append(Prefix + " ");
            }
            s.Append(Def.LabelCap);
            if (Suffix != "")
            {
                s.Append(" " + Suffix);
            }
            return s.ToString();
        }

        public void Draw( ref Vector2 cur, Vector3 colWidths, IHelpDefView window = null )
        {
            // calculate height of row, or fetch from cache
            if( !_heightSet )
            {
                List<float> heights = new List<float>();
                if( !Prefix.NullOrEmpty() )
                {
                    heights.Add( Text.CalcHeight( Prefix, colWidths.x ) );
                }
                heights.Add( Text.CalcHeight( Def.LabelStyled(), colWidths.y ) );
                if( !Suffix.NullOrEmpty() )
                {
                    heights.Add( Text.CalcHeight( Suffix, colWidths.z ) );
                }
                _height = heights.Max();
                _heightSet = true;
            }

            // draw text
            if( !Prefix.NullOrEmpty() )
            {
                Rect prefixRect = new Rect( cur.x, cur.y, colWidths.x, _height );
                Widgets.Label( prefixRect, Prefix );
            }
            if( !Suffix.NullOrEmpty() )
            {
                Rect suffixRect = new Rect( cur.x + colWidths.x + colWidths.y + 2 * HelpDetailSection._columnMargin, cur.y, colWidths.z, _height );
                Widgets.Label( suffixRect, Suffix );
            }

            Rect labelRect;
            if( Def.IconTexture() != null )
            {
                Rect iconRect =
                new Rect( cur.x + colWidths.x + ( Prefix.NullOrEmpty() ? 0 : 1 ) * HelpDetailSection._columnMargin,
                          cur.y + 2f,
                          16f,
                          16f );
                labelRect =
                new Rect( cur.x + colWidths.x + 20f + ( Prefix.NullOrEmpty() ? 0 : 1 ) * HelpDetailSection._columnMargin,
                          cur.y,
                          colWidths.y - 20f,
                          _height );
                Def.DrawColouredIcon( iconRect );
                Widgets.Label( labelRect, Def.LabelStyled() );
            }
            else
            {
                labelRect =
                new Rect( cur.x + colWidths.x + ( Prefix.NullOrEmpty() ? 0 : 1 ) * HelpDetailSection._columnMargin,
                          cur.y,
                          colWidths.y,
                          _height );
                Widgets.Label( labelRect, Def.LabelStyled() );
            }
            

            // def interactions (if any)
            // if we have a window set up to catch jumps, and there is a helpdef available, draw a button on the def text.
            HelpDef helpDef = Def.GetHelpDef();
            if(
                ( window != null )&&
                ( helpDef != null )
            )
            {
                TooltipHandler.TipRegion( labelRect, Def.description + (Def.description.NullOrEmpty() ? "" : "\n\n" ) + "JumpToTopic".Translate() );
                if ( Widgets.ButtonInvisible( labelRect ) )
                {
                    if ( window.Accept( helpDef ) )
                    {
                        window.JumpTo( helpDef );
                    }
                    else
                    {
                        window.SecondaryView( helpDef ).JumpTo( helpDef );
                    }
                }
            }
            if(
                ( helpDef == null )&&
                ( !Def.description.NullOrEmpty() )
            )
            {
                TooltipHandler.TipRegion( labelRect, Def.description );
            }
            cur.y += _height - MainTabWindow_ModHelp.LineHeigthOffset;
        }
    }
}
