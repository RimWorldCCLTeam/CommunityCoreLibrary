using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class MainTabWindow_ModHelp : MainTabWindow
    {

        #region Instance Data

        protected List<ModCategory> _cachedHelpCategories;
        protected HelpDef SelectedHelpDef;

        public const float Margin = 18f;
        public const float EntryHeight = 40f;
        public const float EntryIndent = 50f;

        protected Rect SelectionRect;
        protected Rect DisplayRect;
        protected static Vector2 ArrowImageSize = new Vector2(20f, 20f);

        protected Vector2 selectionScrollPos = default(Vector2);
        protected Vector2 displayScrollPos = default(Vector2);

        #endregion

        #region Category Cache Object

        protected class ModCategory
        {
            readonly List<HelpCategoryDef> helpCategories = new List<HelpCategoryDef>();

            public readonly string ModName;
            public bool Expanded;

            public ModCategory(string modName)
            {
                ModName = modName;
            }

            public List<HelpCategoryDef> HelpCategories
            {
                get
                {
                    return helpCategories.OrderBy(a => a.label).ToList();
                }
            }

            public float DrawHeight
            {
                get
                {
                    return Expanded
                        ? MainTabWindow_ModHelp.EntryHeight + HelpCategories.Sum(cat => cat.DrawHeight)
                        : EntryHeight;
                }
            }

            public void AddCategory(HelpCategoryDef def)
            {
                if (!helpCategories.Contains(def))
                {
                    helpCategories.Add(def);
                }
            }
        }

        #endregion

        #region Category Cache Control

        public override void PreOpen()
        {
            base.PreOpen();
            Reinit();
        }

        private void Reinit()
        {
            _cachedHelpCategories = new List<ModCategory>();
            foreach (var cat in DefDatabase<HelpCategoryDef>.AllDefs)
            {
                if (_cachedHelpCategories.All(t => t.ModName != cat.ModName))
                {
                    var mCat = new ModCategory(cat.ModName);
                    mCat.AddCategory(cat);
                    _cachedHelpCategories.Add(mCat);
                }
                else
                {
                    var mCat = _cachedHelpCategories.Find(t => t.ModName == cat.ModName);
                    mCat.AddCategory(cat);
                }
            }
        }

        #endregion

        #region OTab Rendering

        public override void OTabOnGUI(Rect rect)
        {
            Text.Font = GameFont.Small;

            Rect inRect = rect.ContractedBy(Margin);

            GUI.BeginGroup(inRect);

            SelectionRect = new Rect(0f, 0f, 300f, inRect.height);
            DisplayRect = new Rect(
                SelectionRect.width + Margin, 0f,
                inRect.width - SelectionRect.width - Margin, inRect.height);

            DrawSelectionArea(SelectionRect);
            DrawDisplayArea(DisplayRect);
            Widgets.DrawLineVertical(SelectionRect.xMax + Margin / 2f, 0f, inRect.height);

            GUI.EndGroup();
        }

        void DrawDisplayArea(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            if (SelectedHelpDef == null)
            {
                return;
            }

            GUI.BeginGroup(rect);

            var titleRect = new Rect(0f, 0f, rect.width, 60f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, SelectedHelpDef.LabelCap);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.DrawLineHorizontal(0f, titleRect.yMax, rect.width);

            var outRect = new Rect(
                Margin, titleRect.yMax + Margin,
                rect.width - Margin, rect.height - titleRect.yMax - Margin);

            float height = Text.CalcHeight(SelectedHelpDef.description, outRect.width - 16f);

            var viewRect = new Rect(
                outRect.x, outRect.y,
                outRect.width - 16f, height);

            Widgets.BeginScrollView(outRect, ref displayScrollPos, viewRect);
            Widgets.Label(viewRect, SelectedHelpDef.description);
            Widgets.EndScrollView();

            GUI.EndGroup();
        }

        void DrawSelectionArea(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect);

            Rect outRect = rect.AtZero();
            float height = _cachedHelpCategories.Sum(c => c.DrawHeight);
            var viewRect = new Rect(0f, 0f, outRect.width - 16f, height);
            float curY = outRect.y;

            Widgets.BeginScrollView(outRect, ref selectionScrollPos, viewRect);
            if (_cachedHelpCategories.Count < 1)
            {
                Rect messageRect = outRect.AtZero();
                Widgets.Label(messageRect, "NoHelpDefs".Translate());
            }
            else
            {
                foreach (var m in _cachedHelpCategories)
                {
                    var entryRect = new Rect(0f, curY, viewRect.width, m.DrawHeight);
                    DrawModCategory(entryRect, m);
                    curY += m.DrawHeight;
                }
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        void DrawModCategory(Rect entryRect, ModCategory m)
        {
            GUI.BeginGroup(entryRect);

            float curY = 0f;
            var modRect = new Rect(0f, 0f, entryRect.width, EntryHeight);
            DrawModRow(modRect, m);
            curY += EntryHeight;
            if (m.Expanded)
            {
                foreach (var cat in m.HelpCategories)
                {
                    var catRect = new Rect(0f, curY, entryRect.width, cat.DrawHeight);
                    DrawHelpCategory(catRect, cat);
                    curY += cat.DrawHeight;
                }
            }
            GUI.EndGroup();
        }

        void DrawModRow(Rect modRect, ModCategory mod)
        {
            GUI.BeginGroup(modRect);

            if (modRect.Contains(Event.current.mousePosition))
            {
                Widgets.DrawHighlight(modRect);
            }

            var imageRect = new Rect(
                Margin, modRect.height / 2f - ArrowImageSize.y / 2f,
                ArrowImageSize.x, ArrowImageSize.y);

            Texture2D texture = mod.Expanded ? Icon.HelpMenuArrowUp : Icon.HelpMenuArrowDown;
            GUI.DrawTexture(imageRect, texture);

            var labelRect = new Rect(
                imageRect.xMax + Margin, 0f,
                modRect.width - ArrowImageSize.x - Margin * 2, EntryHeight);

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = Color.yellow;
            Widgets.Label(labelRect, mod.ModName);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.InvisibleButton(modRect))
            {
                mod.Expanded = !mod.Expanded;
            }
            GUI.EndGroup();
        }

        void DrawHelpCategory(Rect catRect, HelpCategoryDef cat)
        {
            GUI.BeginGroup(catRect);

            var catRowRect = new Rect(0f, 0f, catRect.width, EntryHeight);
            DrawHelpCategoryRow(catRowRect, cat);

            if (cat.Expanded)
            {
                float curY = EntryHeight;
                foreach (var helpDef in cat.HelpDefs)
                {
                    var helpRect = new Rect(
                        EntryIndent, curY,
                        catRect.width, EntryHeight);

                    DrawHelpRow(helpRect, helpDef);
                    GUI.color = Color.gray;
                    Widgets.DrawLineHorizontal(0f, curY, catRect.width);
                    GUI.color = Color.white;

                    curY += EntryHeight;
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        void DrawHelpCategoryRow(Rect catRect, HelpCategoryDef cat)
        {
            GUI.BeginGroup(catRect);

            if (catRect.Contains(Event.current.mousePosition))
            {
                Widgets.DrawHighlight(catRect);
            }

            var imageRect = new Rect(
                Margin * 2, catRect.height / 2f - ArrowImageSize.y / 2f,
                ArrowImageSize.x, ArrowImageSize.y);

            Texture2D texture = cat.Expanded ? Icon.HelpMenuArrowUp : Icon.HelpMenuArrowDown;
            GUI.DrawTexture(imageRect, texture);

            var labelRect = new Rect(
                imageRect.xMax + Margin, 0f,
                catRect.width - imageRect.width - Margin * 3, catRect.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, cat.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(0f, 0f, catRect.width);
            GUI.color = Color.white;

            if (Widgets.InvisibleButton(catRect))
            {
                cat.Expanded = !cat.Expanded;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        void DrawHelpRow(Rect hRect, HelpDef hCat)
        {
            if (hCat == SelectedHelpDef)
            {
                Widgets.DrawHighlightSelected(hRect);
            }
            else if (hRect.Contains(Event.current.mousePosition))
            {
                Widgets.DrawHighlight(hRect);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(hRect, hCat.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.InvisibleButton(hRect))
            {
                SelectedHelpDef = hCat;
            }
        }

        #endregion

    }

}
