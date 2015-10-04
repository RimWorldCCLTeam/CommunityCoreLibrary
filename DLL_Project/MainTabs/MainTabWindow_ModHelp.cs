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

        protected static List<ModCategory> _cachedHelpCategories;
        protected HelpDef SelectedHelpDef;

        public const float MarginX = 6f; // 15 is way too much.
        public const float EntryHeight = 30f;
        public const float EntryIndent = 15f;

        protected Rect SelectionRect;
        protected Rect DisplayRect;
        protected static Vector2 ArrowImageSize = new Vector2(10f, 10f);

        protected Vector2 selectionScrollPos = default(Vector2);
        protected Vector2 displayScrollPos = default(Vector2);

        public const float MinWidth = 600f;
        public const float MinHeight = 400f;
        public const float MinListWidth = 200f;

        public override MainTabWindowAnchor Anchor
        {
            get
            {
                return MainTabWindowAnchor.Right;
            }
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                if (TabDef != null)
                {
                    return new Vector2(TabDef.windowSize.x > MinWidth ? TabDef.windowSize.x : MinWidth, TabDef.windowSize.y > MinHeight ? TabDef.windowSize.y : MinHeight);
                }
                return new Vector2(MinWidth, MinHeight);
            }
        }

        private MainTab_HelpMenuDef TabDef
        {
            get
            {
                return def as MainTab_HelpMenuDef;
            }
        }

        #endregion

        #region Constructor

        public MainTabWindow_ModHelp()
        {
            this.layer = WindowLayer.GameUI;
            this.soundAppear = null;
            this.soundClose = null;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.closeOnEscapeKey = true;
        }

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

            //Set whether the window forces a pause
            if (TabDef != null)
            {
                this.forcePause = TabDef.pauseGame;
            }

            // Build the help system
            Recache();
        }

        public static void Recache()
        {
            _cachedHelpCategories = new List<ModCategory>();
            foreach (var cat in DefDatabase<HelpCategoryDef>.AllDefs)
            {
                AddToNewOrExistingModHelpCategory(cat, ref _cachedHelpCategories);
            }
            _filteredHelpCategories = _cachedHelpCategories;
        }

        private static void AddToNewOrExistingModHelpCategory(HelpCategoryDef helpCategory, ref List<ModCategory> modCategoryCache, bool expanded = false)
        {
            // parent modcategory does not exist, create it.
            if (modCategoryCache.All(t => t.ModName != helpCategory.ModName))
            {
                var mCat = new ModCategory(helpCategory.ModName);
                mCat.AddCategory(helpCategory);
                mCat.Expanded = expanded;
                modCategoryCache.Add(mCat);
            }
            // add to existing modcategory
            else
            {
                var mCat = modCategoryCache.Find(t => t.ModName == helpCategory.ModName);
                mCat.AddCategory(helpCategory);
            }
        }

        private static void AddToNewOrExistingHelpCategory(HelpDef help, ref HelpCategoryDef helpCategory,
            bool expanded = false)
        {
            
        }

        #endregion

        #region OTab Rendering

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);

            Text.Font = GameFont.Small;

            GUI.BeginGroup(rect);

            float selectionWidth = TabDef != null ? (TabDef.listWidth >= MinListWidth ? TabDef.listWidth : MinListWidth) : MinListWidth;
            SelectionRect = new Rect(0f, 0f, selectionWidth, rect.height);
            DisplayRect = new Rect(
                SelectionRect.width + MarginX, 0f,
                rect.width - SelectionRect.width - MarginX, rect.height);

            DrawSelectionArea(SelectionRect);
            DrawDisplayArea(DisplayRect);

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

            var outRect = rect.AtZero().ContractedBy(MarginX);
            outRect.yMin += titleRect.height;

            float height = Text.CalcHeight(SelectedHelpDef.description, outRect.width - 16f);

            var viewRect = new Rect(
                outRect.x, outRect.y,
                outRect.width - 16f, height);

            Widgets.BeginScrollView(outRect, ref displayScrollPos, viewRect);
            Widgets.Label(viewRect, SelectedHelpDef.description);
            Widgets.EndScrollView();

            GUI.EndGroup();
        }

        private string _filterString = "";

        private string _lastFilterString = "";

        private int _lastFilterTick;

        private bool _filtered;

        private void _filterUpdate()
        {
            // filter after a short delay.
            if (_filterString != _lastFilterString)
            {
                _lastFilterString = _filterString;
                _lastFilterTick = 0;
                _filtered = false;
            }
            else if (!_filtered)
            {
                if (_lastFilterTick > 60) Filter();
                _lastFilterTick++;
            }
        }

        public void Filter()
        {
            if (_filterString == "")
            {
                _filteredHelpCategories = _cachedHelpCategories;
            }
            // start the worlds deepest nesting.
            else
            {
                _filteredHelpCategories = new List<ModCategory>();
                foreach (ModCategory cachedHelpCategory in _cachedHelpCategories)
                {
                    // modlabel matches, add entire modcategory.
                    if (cachedHelpCategory.ModName.ToUpper().Contains(_filterString.ToUpper()))
                    {
                        _filteredHelpCategories.Add(cachedHelpCategory);
                    }
                    else
                    {
                        foreach (HelpCategoryDef cat in cachedHelpCategory.HelpCategories)
                        {
                            // labelcap matches, add entire helpcategory.
                            if (cat.LabelCap.ToUpper().Contains(_filterString.ToUpper()))
                            {
                                cat.Expanded = true;
                                AddToNewOrExistingModHelpCategory(cat, ref _filteredHelpCategories, true);
                            }
                            else
                            {
                                foreach (HelpDef help in cat.HelpDefs)
                                {
                                    // help labelcap matches - can't do much other than adding the entire category without making local copies of the defs, which I don't want to do right now.
                                    // todo: make local copies of helpcategory defs so we can filter down the helpdefs for viewing.
                                    if (help.LabelCap.ToUpper().Contains(_filterString.ToUpper()))
                                    {
                                        cat.Expanded = true;
                                        AddToNewOrExistingModHelpCategory(cat, ref _filteredHelpCategories, true);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            _filtered = true;
        }

        private static List<ModCategory> _filteredHelpCategories;

        void DrawSelectionArea(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect);
            
            Rect filterRect = new Rect(MarginX, MarginX, rect.width - 3 * MarginX - 30f, 30f);
            Rect clearRect = new Rect(filterRect.xMax + MarginX + 3f, MarginX + 3f, 24f, 24f);
            _filterString = Widgets.TextField(filterRect, _filterString);
            if (_filterString != "")
            {
                if (Widgets.ImageButton(clearRect, Widgets.CheckboxOffTex))
                {
                    _filterString = "";
                    Filter();
                }
                _filterUpdate();
            }

            Rect outRect = rect.AtZero();
            outRect.yMin += 40f;
            float height = _filteredHelpCategories.Sum(c => c.DrawHeight);
            var viewRect = new Rect(0f, 0f, (height > outRect.height ? outRect.width - 16f : outRect.width), height);
            
            Widgets.BeginScrollView(outRect, ref selectionScrollPos, viewRect);
            if (_filteredHelpCategories.Count < 1)
            {
                Rect messageRect = outRect.AtZero();
                Widgets.Label(messageRect, "NoHelpDefs".Translate());
            }
            else
            {
                float curY = 0f;
                float curX = 0f;
                foreach (var m in _filteredHelpCategories)
                {
                    var entryRect = new Rect(0f, curY, viewRect.width, m.DrawHeight);
                    DrawModCategory(entryRect, curX, m);
                    GUI.color = Color.gray;
                    Widgets.DrawLineHorizontal(0f, curY, viewRect.width);
                    GUI.color = Color.white;
                    curY += m.DrawHeight;
                }
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(0f, curY, viewRect.width);
                GUI.color = Color.white;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        void DrawModCategory(Rect entryRect, float curX, ModCategory m)
        {
            GUI.BeginGroup(entryRect);

            float curY = 0f;
            var modRect = new Rect(0f, 0f, entryRect.width, EntryHeight);
            DrawModRow(modRect, curX, m);
            curY += EntryHeight;
            if (m.Expanded)
            {
                foreach (var cat in m.HelpCategories)
                {
                    var catRect = new Rect(0f, curY, entryRect.width, cat.DrawHeight);
                    DrawHelpCategory(catRect, curX + EntryIndent, cat);
                    curY += cat.DrawHeight;
                }
            }
            GUI.EndGroup();
        }

        void DrawModRow(Rect modRect, float curX, ModCategory mod)
        {
            GUI.BeginGroup(modRect);

            if (Mouse.IsOver(modRect))
            {
                Widgets.DrawHighlight(modRect);
            }

            var imageRect = new Rect(
                MarginX + curX, modRect.height / 2f - ArrowImageSize.y / 2f,
                ArrowImageSize.x, ArrowImageSize.y);

            Texture2D texture = mod.Expanded ? Icon.HelpMenuArrowDown : Icon.HelpMenuArrowRight;
            GUI.DrawTexture(imageRect, texture);

            var labelRect = new Rect(
                imageRect.xMax + MarginX, 0f,
                modRect.width - ArrowImageSize.x - MarginX * 2 - curX, EntryHeight);

            Text.Anchor = TextAnchor.MiddleLeft;
            //GUI.color = Color.yellow;
            Widgets.Label(labelRect, mod.ModName);
            //GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.InvisibleButton(modRect))
            {
                mod.Expanded = !mod.Expanded;
            }
            GUI.EndGroup();
        }

        void DrawHelpCategory(Rect catRect, float curX, HelpCategoryDef cat)
        {
            GUI.BeginGroup(catRect);

            var catRowRect = new Rect(0f, 0f, catRect.width, EntryHeight);
            DrawHelpCategoryRow(catRowRect, curX, cat);

            if (cat.Expanded)
            {
                float curY = EntryHeight;
                foreach (var helpDef in cat.HelpDefs)
                {
                    var helpRect = new Rect(
                        0f, curY,
                        catRect.width, EntryHeight);

                    DrawHelpRow(helpRect, curX + EntryIndent, helpDef);
                    GUI.color = Color.gray;
                    Widgets.DrawLineHorizontal(0f, curY, catRect.width);
                    GUI.color = Color.white;

                    curY += EntryHeight;
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        void DrawHelpCategoryRow(Rect catRect, float curX, HelpCategoryDef cat)
        {
            GUI.BeginGroup(catRect);

            if (Mouse.IsOver(catRect))
            {
                Widgets.DrawHighlight(catRect);
            }

            var imageRect = new Rect(
                MarginX + curX, catRect.height / 2f - ArrowImageSize.y / 2f,
                ArrowImageSize.x, ArrowImageSize.y);

            Texture2D texture = cat.Expanded ? Icon.HelpMenuArrowDown : Icon.HelpMenuArrowRight;
            GUI.DrawTexture(imageRect, texture);

            var labelRect = new Rect(
                imageRect.xMax + MarginX, 0f,
                catRect.width - imageRect.width - MarginX * 3 - curX, catRect.height);

            Text.Anchor = TextAnchor.MiddleLeft;

            if (Text.CalcHeight(cat.LabelCap, labelRect.width) > EntryHeight) Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, cat.LabelCap);
            Text.Font = GameFont.Small;
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

        void DrawHelpRow(Rect hRect, float curX, HelpDef hCat)
        {
            if (hCat == SelectedHelpDef)
            {
                Widgets.DrawHighlightSelected(hRect);
            }
            else if (Mouse.IsOver(hRect))
            {
                Widgets.DrawHighlight(hRect);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = hRect;
            labelRect.xMin += curX;
            labelRect.width -= curX;
            if (Text.CalcHeight(hCat.LabelCap, labelRect.width) > EntryHeight) Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, hCat.LabelCap);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.InvisibleButton(hRect))
            {
                SelectedHelpDef = hCat;
            }
        }

        #endregion

    }

}
