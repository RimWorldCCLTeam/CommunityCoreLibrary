using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{
    public class OTab_ModHelp : OTab
    {
        protected static List<ModCategory> _cachedHelpCategories;
        protected static HelpDef SelectedHelpDef = null;

        public const float Margin = 18f;
        public const float EntryHeight = 40f;
        public const float EntryIndent = 50f;

        protected static Rect SelectionRect;
        protected static Rect DisplayRect;
        protected static Vector2 ArrowImageSize = new Vector2(20f, 20f);

        protected Vector2 selectionScrollPos = default(Vector2);
        protected Vector2 displayScrollPos = default(Vector2);

        public OTab_ModHelp()
        {
            title = "HelpOTabTitle".Translate();
            orderPriority = 998;
            Reinit();
        }

        protected class ModCategory
        {
            private string modName;
            private List<HelpCategoryDef> helpCategories = new List<HelpCategoryDef>();

            public ModCategory(string modName)
            {
                this.modName = modName;
            }

            public string ModName
            {
                get { return modName; }
            }

            public List<HelpCategoryDef> HelpCategories
            {
                get { return helpCategories.OrderBy((a) => a.label).ToList(); }
            }

            public float DrawHeight
            {
                get
                {
                    return Expanded ? OTab_ModHelp.EntryHeight + HelpCategories.Sum(cat => cat.DrawHeight) : EntryHeight;
                }
            }

            public bool Expanded { get; set; }

            public void AddCategory(HelpCategoryDef def)
            {
                if (!helpCategories.Contains(def))
                    helpCategories.Add(def);
            }
        }

        public void Reinit()
        {
            _cachedHelpCategories = new List<ModCategory>();
            foreach (var cat in DefDatabase<HelpCategoryDef>.AllDefs)
            {
                if (!_cachedHelpCategories.Any((t) => t.ModName == cat.ModName))
                {
                    ModCategory mCat = new ModCategory(cat.ModName);
                    mCat.AddCategory(cat);
                    _cachedHelpCategories.Add(mCat);
                }
                else
                {
                    ModCategory mCat = _cachedHelpCategories.Find((t) => t.ModName == cat.ModName);
                    mCat.AddCategory(cat);
                }
            }
        }

        public override void OTabOnGUI(Rect rect)
        {
            Text.Font = GameFont.Small;

            Rect inRect = rect.ContractedBy(Margin);
            try
            {
                GUI.BeginGroup(inRect);

                SelectionRect = new Rect(0f, 0f, 300f, inRect.height);
                DisplayRect = new Rect(SelectionRect.width + Margin, 0f, inRect.width - SelectionRect.width - Margin,
                    inRect.height);

                DrawSelectionArea(SelectionRect);
                DrawDisplayArea(DisplayRect);
                Widgets.DrawLineVertical(SelectionRect.xMax + Margin / 2f, 0f, inRect.height);

            }
            catch (Exception e)
            {
                Log.Error("Exception while drawing Help OTab: \n" + e.ToString());
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DrawDisplayArea(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            if (SelectedHelpDef == null)
                return;
            try
            {
                GUI.BeginGroup(rect);

                Rect titleRect = new Rect(0f, 0f, rect.width, 60f);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(titleRect, SelectedHelpDef.LabelCap);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;

                Widgets.DrawLineHorizontal(0f, titleRect.yMax, rect.width);

                Rect outRect = new Rect(Margin, titleRect.yMax + Margin, rect.width - Margin,
                    rect.height - titleRect.yMax - Margin);
                float height = Text.CalcHeight(SelectedHelpDef.description, outRect.width - 16f);
                Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width - 16f, height);
                Widgets.BeginScrollView(outRect, ref displayScrollPos, viewRect);
                Widgets.Label(viewRect, SelectedHelpDef.description);
                Widgets.EndScrollView();
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DrawSelectionArea(Rect rect)
        {
            try
            {
                Widgets.DrawMenuSection(rect);
                GUI.BeginGroup(rect);
                Rect outRect = rect.AtZero();
                float height = _cachedHelpCategories.Sum((c) => c.DrawHeight);
                Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, height);
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
                        Rect entryRect = new Rect(0f, curY, viewRect.width, m.DrawHeight);
                        DrawModCategory(entryRect, m);
                        curY += m.DrawHeight;
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error("Exception while drawing selection error: \n" + e.ToString());
            }
            finally
            {
                Widgets.EndScrollView();
                GUI.EndGroup();
            }
        }

        private void DrawModCategory(Rect entryRect, ModCategory m)
        {
            try
            {
                GUI.BeginGroup(entryRect);

                float curY = 0f;
                Rect modRect = new Rect(0f, 0f, entryRect.width, EntryHeight);
                DrawModRow(modRect, m);
                curY += EntryHeight;
                if (m.Expanded)
                {
                    foreach (var cat in m.HelpCategories)
                    {
                        Rect catRect = new Rect(0f, curY, entryRect.width, cat.DrawHeight);
                        DrawHelpCategory(catRect, cat);
                        curY += cat.DrawHeight;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception drawing mod category: " + m.ModName + "\n" + e.ToString());
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DrawModRow(Rect modRect, ModCategory mod)
        {
            try
            {
                GUI.BeginGroup(modRect);

                if (modRect.Contains(Event.current.mousePosition))
                    Widgets.DrawHighlight(modRect);

                Rect imageRect = new Rect(Margin, modRect.height / 2f - ArrowImageSize.y / 2f, ArrowImageSize.x, ArrowImageSize.y);
                Texture2D texture = mod.Expanded ? Icon.ArrowUp : Icon.ArrowDown;
                GUI.DrawTexture(imageRect, texture);

                Rect labelRect = new Rect(imageRect.xMax + Margin, 0f, modRect.width - ArrowImageSize.x - Margin * 2, EntryHeight);
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = Color.yellow;
                Widgets.Label(labelRect, mod.ModName);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.InvisibleButton(modRect))
                {
                    mod.Expanded = !mod.Expanded;
                }
            }
            finally
            {
                GUI.EndGroup();
            }
        }

        private void DrawHelpCategory(Rect catRect, HelpCategoryDef cat)
        {
            try
            {
                GUI.BeginGroup(catRect);

                Rect catRowRect = new Rect(0f, 0f, catRect.width, EntryHeight);
                DrawHelpCategoryRow(catRowRect, cat);

                if (cat.Expanded)
                {
                    float curY = EntryHeight;
                    foreach (var helpDef in cat.HelpDefs)
                    {
                        Rect helpRect = new Rect(EntryIndent, curY, catRect.width, EntryHeight);
                        DrawHelpRow(helpRect, helpDef);
                        GUI.color = Color.gray;
                        Widgets.DrawLineHorizontal(0f, curY, catRect.width);
                        GUI.color = Color.white;
                        curY += EntryHeight;
                    }
                }
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();
            }
        }

        private void DrawHelpCategoryRow(Rect catRect, HelpCategoryDef cat)
        {
            try
            {
                GUI.BeginGroup(catRect);

                if (catRect.Contains(Event.current.mousePosition))
                    Widgets.DrawHighlight(catRect);

                Rect imageRect = new Rect(Margin * 2, catRect.height / 2f - ArrowImageSize.y / 2f, ArrowImageSize.x, ArrowImageSize.y);
                Texture2D texture = cat.Expanded ? Icon.ArrowUp : Icon.ArrowDown;
                GUI.DrawTexture(imageRect, texture);

                Rect labelRect = new Rect(imageRect.xMax + Margin, 0f, catRect.width - imageRect.width - Margin * 3, catRect.height);
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
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();
            }
        }

        private void DrawHelpRow(Rect hRect, HelpDef hCat)
        {
            if (hCat == SelectedHelpDef)
                Widgets.DrawHighlightSelected(hRect);
            else if (hRect.Contains(Event.current.mousePosition))
                Widgets.DrawHighlight(hRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(hRect, hCat.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.InvisibleButton(hRect))
            {
                OTab_ModHelp.SelectedHelpDef = hCat;
            }
        }

    }
}
