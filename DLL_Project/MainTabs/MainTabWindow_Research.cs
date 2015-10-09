using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityCoreLibrary;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace CommunityCoreLibrary
{
    public class MainTabWindow_Research : MainTabWindow
    {
        // UI settings
        private const float LeftAreaWidth = 330f;
        private const int ProjectIntervalY = 25;
        private Vector2 _projectListScrollPosition = Vector2.zero;
        private Vector2 _contentScrollPos = Vector2.zero;
        private Vector2 _margin = new Vector2(6f, 6f);
        private Vector2 _buttonSize = new Vector2(100f, 50f);

        // Core toggles and resources
        protected ResearchProjectDef SelectedProject;
        private ShowResearch _showResearchedProjects = ShowResearch.Available;
        private bool _noBenchWarned;

        // Progress bar textures
        private static readonly Texture2D BarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
        private static readonly Texture2D BarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        // Filter toggles and resources
        private IEnumerable<ResearchProjectDef> _source;
        private SortOptions _sortBy = SortOptions.Cost;
        private bool _asc = true;
        private string _filter = "";
        private string _oldFilter;
        private bool _helpDefLogged;

        // enums
        private enum ShowResearch
        {
            All,
            Completed,
            Available
        }

        public enum SortOptions
        {
            Name,
            Cost
        }

        public override float TabButtonBarPercent
        {
            get
            {
                ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
                if (currentProj == null)
                {
                    return 0f;
                }
                return currentProj.PercentComplete;
            }
        }

        // The tab definitions for the research lists.
        private List<TabRecord> SourceTabs
        {
            get
            {
                List<TabRecord> list = new List<TabRecord>();
                TabRecord item = new TabRecord("RI.All".Translate(), delegate
                {
                    this._showResearchedProjects = ShowResearch.All;
                    RefreshSource();
                }, _showResearchedProjects == ShowResearch.All);
                list.Add(item);
                TabRecord item2 = new TabRecord("Researched".Translate(), delegate
                {
                    this._showResearchedProjects = ShowResearch.Completed;
                    RefreshSource();
                }, _showResearchedProjects == ShowResearch.Completed);
                list.Add(item2);
                TabRecord item3 = new TabRecord("RI.Available".Translate(), delegate
                {
                    this._showResearchedProjects = ShowResearch.Available;
                    RefreshSource();
                }, _showResearchedProjects == ShowResearch.Available);
                list.Add(item3);
                return list;
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            SelectedProject = Find.ResearchManager.currentProj;
            _filter = "";
            _oldFilter = "";
            RefreshSource();
            MainTabWindow_ModHelp.Recache();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            // warn the player if no research bench is built
            if (!_noBenchWarned)
            {
                if (!Find.ListerBuildings.ColonistsHaveBuilding(ThingDefOf.ResearchBench))
                {
                    Find.WindowStack.Add(new Dialog_Message("ResearchMenuWithoutBench".Translate()));
                }
                _noBenchWarned = true;
            }

            // Title
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 300f), "Research".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // create content areas
            Rect sidebar = new Rect(0f, 75f, LeftAreaWidth, inRect.height - 75f);
            Rect content = new Rect(sidebar.xMax + _margin.x, 45f, inRect.width - sidebar.width - _margin.x, inRect.height - 45f);

            // draw boxes around content areas
            Widgets.DrawMenuSection(sidebar, false);
            Widgets.DrawMenuSection(content);

            // plop in extra row for input + sort buttons
            // set up rects
            Rect sortFilterRow = sidebar.ContractedBy(10f);
            sortFilterRow.height = 30f;
            Rect filterRect = new Rect(sortFilterRow);
            filterRect.width = sortFilterRow.width - 110f;
            Rect deleteFilter = new Rect(filterRect.xMax + _margin.x, filterRect.yMin + 3f, 24f, 24f);
            Rect sortByName = new Rect(deleteFilter.xMax + _margin.x, filterRect.yMin + 3f, 24f, 24f);
            Rect sortByCost = new Rect(sortByName.xMax + _margin.x, filterRect.yMin + 3f, 24f, 24f);

            // finetune rects

            // tooltips
            TooltipHandler.TipRegion(filterRect, "RI.filterTooltip".Translate());
            if (_filter != "")
            {
                TooltipHandler.TipRegion(deleteFilter, "RI.deleteFilterTooltip".Translate());
            }
            TooltipHandler.TipRegion(sortByName, "RI.sortByNameTooltip".Translate());
            TooltipHandler.TipRegion(sortByCost, "RI.sortByCostTooltip".Translate());


            // filter input, update in realtime - it's not a very expensive op, and we're paused anyway.
            _filter = Widgets.TextField(filterRect, _filter);
            if (_oldFilter != _filter)
            {
                _oldFilter = _filter;
                RefreshSource();
            }
            if (_filter != "")
            {
                if (Widgets.ImageButton(deleteFilter, Widgets.CheckboxOffTex))
                {
                    _filter = "";
                    RefreshSource();
                }
            }

            // sort options
            if (Widgets.ImageButton(sortByName, Icon.SortByName))
            {
                if (_sortBy != SortOptions.Name)
                {
                    _sortBy = SortOptions.Name;
                    _asc = false;
                    RefreshSource();
                }
                else
                {
                    _asc = !_asc;
                    RefreshSource();
                }
            }
            if (Widgets.ImageButton(sortByCost, Icon.SortByCost))
            {
                if (_sortBy != SortOptions.Cost)
                {
                    _sortBy = SortOptions.Cost;
                    _asc = true;
                    RefreshSource();
                }
                else
                {
                    _asc = !_asc;
                    RefreshSource();
                }
            }

            // contract sidebar area for margins, bump down to compensate for filter.
            Rect sidebarInner = sidebar.ContractedBy(10f);
            sidebarInner.yMin += ProjectIntervalY + _margin.y;
            sidebarInner.height -= ProjectIntervalY + _margin.y;

            // set height
            float height = ProjectIntervalY * _source.Count() + 100;

            // begin scrollview and group (group sets x,y coordinates of rects in it to be relative to the groups rect)
            Rect sidebarContent = new Rect(0f, 0f, sidebarInner.width - 16f, height);
            Widgets.BeginScrollView(sidebarInner, ref _projectListScrollPosition, sidebarContent);
            Rect position = sidebarContent.ContractedBy(_margin.x);
            GUI.BeginGroup(position);

            // Draw the list of researches in the source chosen.
            int curY = 0;

            foreach (ResearchProjectDef current in from rp in _source
                                                   select rp)
            {
                Rect sidebarRow = new Rect(0f, curY, position.width, ProjectIntervalY);
                DrawResearchRow(current, sidebarRow);
                curY += ProjectIntervalY;
            }
            GUI.EndGroup();
            Widgets.EndScrollView();

            // Draw the source selection tabs.
            TabDrawer.DrawTabs(sidebar, SourceTabs);

            // Draw the content area.
            DrawResearchContent(content);
        }

        private void DrawResearchContent(Rect rect)
        {
            // Find the corresponding helpdef
            HelpDef helpDef = DefDatabase<HelpDef>.AllDefsListForReading.Find(hd => (
               (hd.keyDef == SelectedProject)
            ));

            // if either is null - something is badly wrong.
            if (SelectedProject == null || helpDef == null)
            {
                return;
            }

            // Set up rects
            Rect descRect = rect.ContractedBy(_margin.x);
            descRect.height -= _buttonSize.y * 2 + _margin.y * 2;
            Rect controlRect = rect.ContractedBy(_margin.x);
            controlRect.yMin = descRect.yMax + _margin.y;

            #region description
            GUI.BeginGroup(descRect);

            var titleRect = new Rect(0f, 0f, rect.width, 60f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, SelectedProject.LabelCap);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            var outRect = rect.AtZero().ContractedBy(_margin.x);
            outRect.yMin += titleRect.height;

            float height = Text.CalcHeight(helpDef.description, outRect.width - 16f);

            var viewRect = new Rect(
                outRect.x, outRect.y,
                outRect.width - 16f, height
            );

            Widgets.BeginScrollView(outRect, ref _contentScrollPos, viewRect);
            Widgets.Label(viewRect, helpDef.description);
            Widgets.EndScrollView();

            GUI.EndGroup();
            #endregion

            #region controls

            GUI.BeginGroup(controlRect);
            Rect buttonRect = new Rect(controlRect.width / 2f - _buttonSize.x / 2, 0f, _buttonSize.x, _buttonSize.y);
            if (SelectedProject.IsFinished)
            {
                Widgets.DrawMenuSection(buttonRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(buttonRect, "Finished".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else if (SelectedProject == Find.ResearchManager.currentProj)
            {
                Widgets.DrawMenuSection(buttonRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(buttonRect, "InProgress".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else if (!SelectedProject.PrereqsFulfilled)
            {
                Widgets.DrawMenuSection(buttonRect);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(buttonRect, "RI.PreReqLocked".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                if (Widgets.TextButton(buttonRect, "Research".Translate()))
                {
                    SoundDef.Named("ResearchStart").PlayOneShotOnCamera();
                    Find.ResearchManager.currentProj = SelectedProject;
                }
                if (Prefs.DevMode)
                {
                    Rect devButtonRect = buttonRect;
                    devButtonRect.x += devButtonRect.width + _margin.x;
                    if (Widgets.TextButton(devButtonRect, "Debug Insta-finish"))
                    {
                        Find.ResearchManager.currentProj = SelectedProject;
                        Find.ResearchManager.InstantFinish(SelectedProject);
                    }
                }
            }
            Rect progressRect = new Rect(_buttonSize.x + _margin.x, _buttonSize.y, controlRect.width, _buttonSize.x);
            Widgets.FillableBar(progressRect, SelectedProject.PercentComplete, BarFillTex, BarBgTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(progressRect, SelectedProject.ProgressNumbersString);
            Text.Anchor = TextAnchor.UpperLeft;

            #endregion

        }

        private void DrawResearchRow(ResearchProjectDef current, Rect sidebarRow)
        {
            if (SelectedProject == current)
            {
                GUI.DrawTexture(sidebarRow, TexUI.HighlightTex);
            }

            string text = current.LabelCap + " (" + current.totalCost.ToString("F0") + ")";
            Rect sidebarRowInner = new Rect(sidebarRow);
            sidebarRowInner.x += 6f;
            sidebarRowInner.width -= 6f;
            float num2 = Text.CalcHeight(text, sidebarRowInner.width);
            if (sidebarRowInner.height < num2)
            {
                sidebarRowInner.height = num2 + 3f;
            }
            // give the label a colour if we're in the all tab.
            Color textColor;
            if (_showResearchedProjects == ShowResearch.All)
            {
                if (current.IsFinished)
                {
                    textColor = new Color(1f, 1f, 1f);
                }
                else if (!current.PrereqsFulfilled)
                {
                    textColor = new Color(.6f, .6f, .6f);
                }
                else
                {
                    textColor = new Color(.8f, .85f, 1f);
                }
            }
            else
            {
                textColor = new Color(.8f, .85f, 1f);
            }
            if (Widgets.TextButton(sidebarRowInner, text, false, true, textColor))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                SelectedProject = current;
            }
        }

        private void RefreshSource()
        {
            if (_showResearchedProjects == ShowResearch.All)
            {
                _source = from proj in DefDatabase<ResearchProjectDef>.AllDefs
                          where !proj.prerequisites.Contains(proj)
                          select proj;
            }
            else if (_showResearchedProjects == ShowResearch.Completed)
            {
                _source = from proj in DefDatabase<ResearchProjectDef>.AllDefs
                          where proj.IsFinished && proj.PrereqsFulfilled
                          select proj;
            }
            else
            {
                _source = from proj in DefDatabase<ResearchProjectDef>.AllDefs
                          where !proj.IsFinished && proj.PrereqsFulfilled
                          select proj;
            }

            if (_filter != "")
            {
                _source = _source.Where(rpd => rpd.label.ToUpper().Contains(_filter.ToUpper()));
            }

            switch (_sortBy)
            {
                case SortOptions.Cost:
                    _source = _source.OrderBy(rpd => rpd.totalCost);
                    break;
                case SortOptions.Name:
                    _source = _source.OrderBy(rpd => rpd.LabelCap);
                    break;
            }

            if (_asc) _source = _source.Reverse();
        }
    }
}
