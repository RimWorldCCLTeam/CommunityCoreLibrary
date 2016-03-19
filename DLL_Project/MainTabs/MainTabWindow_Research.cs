using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CommunityCoreLibrary
{
    /// <summary>
    /// This thing is broken.
    /// </summary>
    public class MainTabWindow_Research : MainTabWindow, IHelpDefView
    {
        // UI settings
        private const float     LeftAreaWidth               = 330f;

        private const int       ProjectIntervalY            = 25;
        private Vector2         _projectListScrollPosition  = Vector2.zero;
        private Vector2         _contentScrollPos           = Vector2.zero;
        private Vector2         _margin                     = new Vector2(6f, 6f);
        private Vector2         _buttonSize                 = new Vector2(100f, 50f);
        private float           _contentHeight               = 9999f;

        // Progress bar textures
        private static readonly Texture2D BarFillTex        = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

        private static readonly Texture2D BarBgTex          = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        // toggles and resources
        private IEnumerable<ResearchProjectDef> _researchDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where( rpd => rpd.GetHelpDef() != null);

        private IEnumerable<HelpDef> _advancedHelpDefs        = DefDatabase<HelpDef>.AllDefsListForReading.Where(hd => hd.keyDef is AdvancedResearchDef);
        private IEnumerable<HelpDef> _source;
        private SortOptions     _sortBy                     = SortOptions.Cost;
        private bool            _asc                        = true;
        private string          _filter                     = "";
        private string          _oldFilter;
        protected HelpDef       SelectedProject;
        private ShowResearch    _showResearchedProjects     = ShowResearch.Available;
        private bool            _noBenchWarned;

        // enums
        private enum ShowResearch
        {
            All,
            Completed,
            Available,
            Advanced
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
                if ( currentProj == null )
                {
                    return 0f;
                }
                return currentProj.PercentComplete;
            }
        }

        private List<TabRecord> _sourceTabs;

        // The tab definitions for the research lists.
        private List<TabRecord> SourceTabs
        {
            get
            {
                if ( _sourceTabs == null )
                {
                    _sourceTabs = new List<TabRecord>();
                    TabRecord item = new TabRecord( "RI.All".Translate(), delegate
                    {
                        this._showResearchedProjects = ShowResearch.All;
                        RefreshSource();
                    }, _showResearchedProjects == ShowResearch.All );
                    _sourceTabs.Add( item );

                    TabRecord item2 = new TabRecord( "Researched".Translate(), delegate
                    {
                        this._showResearchedProjects = ShowResearch.Completed;
                        RefreshSource();
                    }, _showResearchedProjects == ShowResearch.Completed );
                    _sourceTabs.Add( item2 );

                    TabRecord item3 = new TabRecord( "RI.Available".Translate(), delegate
                    {
                        this._showResearchedProjects = ShowResearch.Available;
                        RefreshSource();
                    }, _showResearchedProjects == ShowResearch.Available );
                    _sourceTabs.Add( item3 );

                    // Add Advanced Research tab if there are any helpdefs for it.
                    // This ensures we don't grab any that didn't pass the 'relevancy' check.
                    if ( DefDatabase<HelpDef>.AllDefsListForReading.Any( hd => hd.keyDef is AdvancedResearchDef ) )
                    {
                        TabRecord AR = new TabRecord( "AutoHelpSubCategoryAdvanced".Translate(), delegate
                        {
                            _showResearchedProjects = ShowResearch.Advanced;
                            RefreshSource();
                        }, _showResearchedProjects == ShowResearch.Advanced );
                        _sourceTabs.Add( AR );
                    }
                }

                return _sourceTabs;
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            if( Find.ResearchManager.currentProj != null )
            {
                SelectedProject = Find.ResearchManager.currentProj.GetHelpDef();
            }
            _filter = "";
            _oldFilter = "";
            RefreshSource();
            MainTabWindow_ModHelp.Recache();
        }

        public override void DoWindowContents( Rect inRect )
        {
            base.DoWindowContents( inRect );

            // warn the player if no research bench is built
            if ( !_noBenchWarned )
            {
                if ( !Find.ListerBuildings.allBuildingsColonist.Any( b => (
                     ( b.def.thingClass == typeof( Building_ResearchBench ) )||
                     ( b.def.thingClass.IsSubclassOf( typeof( Building_ResearchBench ) ) )
                 ) ) )
                {
                    Find.WindowStack.Add( new Dialog_Message( "ResearchMenuWithoutBench".Translate() ) );
                }
                _noBenchWarned = true;
            }

            // Title
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label( new Rect( 0f, 0f, inRect.width, 300f ), "Research".Translate() );
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // create content areas
            Rect sidebar = new Rect( 0f, 75f, LeftAreaWidth, inRect.height - 75f );
            Rect content = new Rect( sidebar.xMax + _margin.x, 45f, inRect.width - sidebar.width - _margin.x, inRect.height - 45f );

            // draw boxes around content areas
            Widgets.DrawMenuSection( sidebar, false );
            Widgets.DrawMenuSection( content );

            // plop in extra row for input + sort buttons
            // set up rects
            Rect sortFilterRow = sidebar.ContractedBy( 10f );
            sortFilterRow.height = 30f;
            Rect filterRect = new Rect( sortFilterRow );
            filterRect.width = sortFilterRow.width - 110f;
            Rect deleteFilter = new Rect( filterRect.xMax + _margin.x, filterRect.yMin + 3f, 24f, 24f );
            Rect sortByName = new Rect( deleteFilter.xMax + _margin.x, filterRect.yMin + 3f, 24f, 24f );
            Rect sortByCost = new Rect( sortByName.xMax + _margin.x, filterRect.yMin + 3f, 24f, 24f );

            // tooltips
            TooltipHandler.TipRegion( filterRect, "RI.filterTooltip".Translate() );
            if ( _filter != "" )
            {
                TooltipHandler.TipRegion( deleteFilter, "RI.deleteFilterTooltip".Translate() );
            }
            TooltipHandler.TipRegion( sortByName, "RI.sortByNameTooltip".Translate() );
            TooltipHandler.TipRegion( sortByCost, "RI.sortByCostTooltip".Translate() );

            // filter input, update in realtime - it's not a very expensive op, and we're paused anyway.
            _filter = Widgets.TextField( filterRect, _filter );
            if ( _oldFilter != _filter )
            {
                _oldFilter = _filter;
                RefreshSource();
            }
            if ( _filter != "" )
            {
                if ( Widgets.ImageButton( deleteFilter, Widgets.CheckboxOffTex ) )
                {
                    _filter = "";
                    RefreshSource();
                }
            }

            // sort options
            if ( Widgets.ImageButton( sortByName, Icon.SortByName ) )
            {
                if ( _sortBy != SortOptions.Name )
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
            if ( Widgets.ImageButton( sortByCost, Icon.SortByCost ) )
            {
                if ( _sortBy != SortOptions.Cost )
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
            Rect sidebarInner = sidebar.ContractedBy( 10f );
            sidebarInner.yMin += ProjectIntervalY + _margin.y;
            sidebarInner.height -= ProjectIntervalY + _margin.y;

            // set height
            float height = ProjectIntervalY * _source.Count();

            // begin scrollview and group
            Rect sidebarContent = new Rect( 0f, 0f, sidebarInner.width - 16f, height );
            Widgets.BeginScrollView( sidebarInner, ref _projectListScrollPosition, sidebarContent );
            Rect position = sidebarContent.ContractedBy( _margin.x );
            GUI.BeginGroup( position );

            // Draw the list of researches in the source chosen.
            int curY = 0;

            foreach ( HelpDef current in from rp in _source
                                         select rp )
            {
                Rect sidebarRow = new Rect( 0f, curY, position.width, ProjectIntervalY );
                DrawResearchRow( current, sidebarRow );
                curY += ProjectIntervalY;
            }
            GUI.EndGroup();
            Widgets.EndScrollView();

            // Draw the source selection tabs.
            TabDrawer.DrawTabs( sidebar, SourceTabs );

            // Draw the content area.
            DrawResearchContent( content );
        }

        private void DrawResearchContent( Rect rect )
        {
            if ( SelectedProject == null )
            {
                return;
            }

            // Set up rects
            Rect titleRect = new Rect( rect.xMin, rect.yMin, rect.width, 60f );
            Rect descRect = rect.ContractedBy( _margin.x * 2 );
            descRect.yMin = titleRect.yMax;
            descRect.yMax -= _buttonSize.y * 2 + _margin.y * 2;
            Rect controlRect = rect.ContractedBy( _margin.x * 2 );
            controlRect.yMin = descRect.yMax + _margin.y;

            // description
            float paragraphMargin = 8f;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label( titleRect, SelectedProject.LabelCap );
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect viewRect = descRect;
            viewRect.width -= 16f;
            viewRect.height = _contentHeight;

            Widgets.BeginScrollView( descRect, ref _contentScrollPos, viewRect.AtZero() );

            Vector2 cur = Vector2.zero;

            HelpDetailSectionHelper.DrawText( ref cur, viewRect.width - cur.x, SelectedProject.description );

            cur.y += paragraphMargin;

            foreach ( HelpDetailSection section in SelectedProject.HelpDetailSections )
            {
                section.Draw( ref cur, viewRect.width, this );
            }

            _contentHeight = cur.y;

            Widgets.EndScrollView();

            // controls
            GUI.BeginGroup( controlRect );
            Rect buttonRect = new Rect( controlRect.width / 2f - _buttonSize.x / 2, 0f, _buttonSize.x, _buttonSize.y );

            Def selectedProjectDef = SelectedProject.keyDef;

            // regular research
            if ( selectedProjectDef is ResearchProjectDef )
            {
                ResearchProjectDef selectedResearchProjectDef = (ResearchProjectDef)selectedProjectDef;
                if ( selectedResearchProjectDef.IsFinished )
                {
                    Widgets.DrawMenuSection( buttonRect );
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label( buttonRect, "Finished".Translate() );
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if ( selectedResearchProjectDef == Find.ResearchManager.currentProj )
                {
                    Widgets.DrawMenuSection( buttonRect );
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label( buttonRect, "InProgress".Translate() );
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if ( !selectedResearchProjectDef.PrereqsFulfilled )
                {
                    Widgets.DrawMenuSection( buttonRect );
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label( buttonRect, "RI.PreReqLocked".Translate() );
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else
                {
                    if ( Widgets.TextButton( buttonRect, "Research".Translate() ) )
                    {
                        SoundDef.Named( "ResearchStart" ).PlayOneShotOnCamera();
                        Find.ResearchManager.currentProj = selectedResearchProjectDef;
                    }
                    if ( Prefs.DevMode )
                    {
                        Rect devButtonRect = buttonRect;
                        devButtonRect.x += devButtonRect.width + _margin.x;
                        if ( Widgets.TextButton( devButtonRect, "Debug Insta-finish" ) )
                        {
                            Find.ResearchManager.currentProj = selectedResearchProjectDef;
                            Find.ResearchManager.InstantFinish( selectedResearchProjectDef );
                        }
                    }
                }
                Rect progressRect = new Rect( _margin.x, _buttonSize.y + _margin.y, controlRect.width - 2 * _margin.x, _buttonSize.y );
                Widgets.FillableBar( progressRect, selectedResearchProjectDef.PercentComplete, BarFillTex, BarBgTex, true );
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label( progressRect, selectedResearchProjectDef.ProgressNumbersString );
            }

            // advanced research
            if ( selectedProjectDef is AdvancedResearchDef )
            {
                AdvancedResearchDef selectedAdvancedResearchDef = (AdvancedResearchDef)selectedProjectDef;
                List<ResearchProjectDef> prereqs = selectedAdvancedResearchDef.researchDefs;

                if ( selectedAdvancedResearchDef.IsFinished )
                {
                    Widgets.DrawMenuSection( buttonRect );
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label( buttonRect, "Finished".Translate() );
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if ( prereqs.Any( rd => rd.IsFinished ) )
                {
                    Widgets.DrawMenuSection( buttonRect );
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label( buttonRect, "InProgress".Translate() );
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if ( selectedAdvancedResearchDef.IsLockedOut() )
                {
                    Widgets.DrawMenuSection( buttonRect );
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label( buttonRect, "RI.PreReqLocked".Translate() );
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                // progress, slightly different for advanced research def
                Rect progressRect = new Rect( _margin.x, _buttonSize.y + _margin.y, controlRect.width - 2 * _margin.x, _buttonSize.y );
                float percentComplete = prereqs.Count( rd => rd.IsFinished ) / prereqs.Count;
                Widgets.FillableBar( progressRect, percentComplete, BarFillTex, BarBgTex, true );
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label( progressRect, prereqs.Count( rd => rd.IsFinished ) + " / " + prereqs.Count );
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        private void DrawResearchRow( HelpDef helpDef, Rect sidebarRow )
        {
            if ( SelectedProject == helpDef )
            {
                GUI.DrawTexture( sidebarRow, TexUI.HighlightTex );
            }
            string text = helpDef.LabelCap;

            Def def = helpDef.keyDef;

            if ( def is ResearchProjectDef )
            {
                text += " (" + ( (ResearchProjectDef)def ).totalCost.ToString( "F0" ) + ")";
            }
            else if ( def is AdvancedResearchDef )
            {
                text += " (" + ( (AdvancedResearchDef)def ).TotalCost.ToString( "F0" ) + ")";
            }
#if DEBUG
            else
            {
                text += " (" + def.GetType() + ")";
            }
#endif
            Rect sidebarRowInner = new Rect( sidebarRow );
            sidebarRowInner.x += 6f;
            sidebarRowInner.width -= 6f;
            float num2 = Text.CalcHeight( text, sidebarRowInner.width );
            if ( sidebarRowInner.height < num2 )
            {
                sidebarRowInner.height = num2 + 3f;
            }
            // give the label a color if we're in the all tab.
            Color textColor = new Color( .8f, .85f, 1f );
            ;
            if ( def is ResearchProjectDef )
            {
                if ( ( (ResearchProjectDef)def ).IsFinished )
                {
                    textColor = new Color( 1f, 1f, 1f );
                }
                else if ( !( (ResearchProjectDef)def ).PrereqsFulfilled )
                {
                    textColor = new Color( .6f, .6f, .6f );
                }
            }
            else if ( def is AdvancedResearchDef )
            {
                if ( ( (AdvancedResearchDef)def ).IsFinished )
                {
                    textColor = new Color( 1f, 1f, 1f );
                }
                else if ( ( (AdvancedResearchDef)def ).IsLockedOut() )
                {
                    textColor = new Color( .6f, .6f, .6f );
                }
            }

            if ( Widgets.TextButton( sidebarRowInner, text, false, true, textColor ) )
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                SelectedProject = helpDef;
            }
        }

        private void RefreshSource()
        {
            if ( _showResearchedProjects == ShowResearch.All )
            {
                _source = from hd in _researchDefs
                          where !hd.prerequisites.Contains( hd ) && hd.GetHelpDef() != null
                          select hd.GetHelpDef();
            }
            else if ( _showResearchedProjects == ShowResearch.Completed )
            {
                _source = from hd in _researchDefs
                          where hd.IsFinished && hd.GetHelpDef() != null
                          select hd.GetHelpDef();
            }
            else if ( _showResearchedProjects == ShowResearch.Available )
            {
                _source = from hd in _researchDefs
                          where !hd.IsFinished && hd.PrereqsFulfilled && hd.GetHelpDef() != null
                          select hd.GetHelpDef();
            }
            else if ( _showResearchedProjects == ShowResearch.Advanced )
            {
                _source = from hd in _advancedHelpDefs
                          where hd.keyDef is AdvancedResearchDef
                          select hd;
            }

            // just to make sure _source is a valid IEnumerable at this point
            if ( _source == null )
                _source = new List<HelpDef>();

            if ( _filter != "" )
            {
                _source = _source.Where( rpd => rpd.label.ToUpper().Contains( _filter.ToUpper() ) );
            }

            if ( _source.Count() > 2 )
            {
                switch ( _sortBy )
                {
                    case SortOptions.Cost:
                        if ( _showResearchedProjects != ShowResearch.Advanced )
                        {
                            _source = _source.OrderBy( hd => ( (ResearchProjectDef)hd.keyDef ).totalCost );
                        }
                        else
                        {
                            _source =
                                _source.OrderBy(
                                    hd => ( (AdvancedResearchDef)hd.keyDef ).TotalCost );
                        }
                        break;

                    case SortOptions.Name:
                        _source = _source.OrderBy( rpd => rpd.LabelCap ?? string.Empty );
                        break;
                }

                if ( _asc )
                    _source = _source.Reverse();
            }
        }

        public void JumpTo( HelpDef def )
        {
            Find.MainTabsRoot.SetCurrentTab( this.def, false );
            _showResearchedProjects = ShowResearch.All;
            RefreshSource();
            SelectedProject = def;
        }

        public bool Accept( HelpDef def )
        {
            return def.keyDef is ResearchProjectDef;
        }

        public IHelpDefView SecondaryView( HelpDef def )
        {
            MainTabDef helpTab = DefDatabase<MainTabDef>.GetNamed( "CCL_ModHelp", false );
            return helpTab.Window as MainTabWindow_ModHelp;
        }
    }
}