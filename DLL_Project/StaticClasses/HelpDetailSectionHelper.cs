using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.StaticClasses
{
    class HelpDetailSectionHelper
    {

        #region DefStringTriplet list builder

        public static List<DefStringTriplet> BuildDefStringTripletList(List<Def> defs, string[] prefixes = null, string[] suffixes = null)
        {
            bool pre = false, suf = false;
            if (prefixes != null)
            {
                if (prefixes.Length != defs.Count)
                {
                    throw new Exception("Prefix array length does not match Def list length.");
                }
                pre = true;
            }

            if (suffixes != null)
            {
                if (suffixes.Length != defs.Count)
                {
                    throw new Exception("Suffix array length does not match Def list length.");
                }
                suf = true;
            }

            List<DefStringTriplet> ret = new List<DefStringTriplet>();
            for (int i = 0; i < defs.Count; i++)
            {
                ret.Add(new DefStringTriplet(defs[i], pre ? prefixes[i] : null, suf ? suffixes[i] : null));
            }

            return ret;
        }

        #endregion

        #region GUI drawer functions

        public static void DrawText(ref Vector2 cur, Rect rect, string text)
        {
            float width = rect.width - cur.x;
            float height = Text.CalcHeight(text, rect.width - cur.x);
            Rect textRect = new Rect(cur.x, cur.y, width, height);
            Widgets.Label(textRect, text);
            cur.y += height - 6f; // offset to make lineheights fit better
        }

        public static bool DrawDefLink(ref Vector2 cur, Rect rect, DefStringTriplet def, bool useHelpDef = true)
        {
            bool        hasPrefix               = !string.IsNullOrEmpty(def.Prefix);
            bool        hasSuffix               = !string.IsNullOrEmpty(def.Suffix);
            bool        prefixNewline           = false;
            bool        suffixNewline           = false;
            bool        defLinkResolves         = true;
            bool        clicked                 = false;

            float       width                   = rect.width - cur.x;
            float       space                   = 6f;
            float       lineheightOffset        = -6f;
            float       startX                  = cur.x;

            Vector2     prefixSize              = Vector2.zero;
            Vector2     labelSize               = Text.CalcSize(def.Def.LabelCap);
            Vector2     suffixSize              = Vector2.zero;

            if (useHelpDef)
            {
                // check if this helpDef is implemented. If we're not using helpdefs, we assume we know what we are doing.
                if (!DefDatabase<HelpDef>.AllDefsListForReading.Any(hd => hd.keyDef == def.Def))
                {
                    defLinkResolves = false;
                }
            }

            // determine available size and how we can fill it.
            if (hasPrefix)
            {
                prefixSize = Text.CalcSize(def.Prefix);
            }

            if (hasSuffix)
            {
                suffixSize = Text.CalcSize(def.Suffix);
            }

            // set newline toggles if prefix / label / suffix are too big.
            if (prefixSize.x + labelSize.x + suffixSize.x > width)
            {
                if (prefixSize.x + labelSize.x > width || prefixSize.x > suffixSize.x)
                {
                    prefixNewline = true;
                }
                if (suffixSize.x + labelSize.x > width || suffixSize.x > prefixSize.x)
                {
                    suffixNewline = true;
                }
            }

            // Do the drawing
            if (hasPrefix)
            {
                prefixSize.y = Text.CalcHeight(def.Prefix, width);
                Rect prefixRect = new Rect(cur.x, cur.y, prefixSize.x, prefixSize.y);
                Widgets.Label(prefixRect, def.Prefix);
                if (prefixNewline)
                {
                    cur.y += prefixSize.y + lineheightOffset;
                }
                else
                {
                    cur.x += prefixSize.x + space;
                }
            }

            prefixSize.y = Text.CalcHeight(def.Def.LabelCap, width);
            Rect labelRect = new Rect(cur.x, cur.y, labelSize.x, labelSize.y);
            if (defLinkResolves)
            {
                GUI.Label(labelRect, "<b>" + def.Def.LabelCap + "</b>");
                clicked = Widgets.InvisibleButton(labelRect);
            }
            else
            {
                // make information that could potentially be a helpdef italic in debug mode.
#if DEBUG
                GUI.Label(labelRect, "<i>" + def.Def.LabelCap + "</i>");
#else
                GUI.Label(labelRect, def.Def.LabelCap);
#endif
            }

            if (suffixNewline)
            {
                cur.y += labelSize.y + lineheightOffset;
                cur.x = startX;
            }
            else
            {
                cur.x += labelSize.x + space;
            }

            if (hasSuffix)
            {
                suffixSize.y = Text.CalcHeight(def.Suffix, width);
                Rect suffixRect = new Rect(cur.x, cur.y, suffixSize.x, suffixSize.y);
                Widgets.Label(suffixRect, def.Suffix);
                cur.y += suffixSize.y + lineheightOffset;
            }
            else
            {
                cur.y += labelSize.y + lineheightOffset;
            }

            cur.x = startX;

            return clicked;

        }




#endregion
    }
}
