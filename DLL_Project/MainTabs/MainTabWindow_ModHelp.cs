using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CommunityCoreLibrary.StaticClasses;
using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class MainTabWindow_ModHelp : MainTabWindow
    {

        #region Instance Data

        protected static List<ModCategory>  _cachedHelpCategories;
        protected HelpDef                   SelectedHelpDef;

        public const float                  Margin                  = 6f; // 15 is way too much.
        public const float                  EntryHeight             = 30f;
        public const float                  EntryIndent             = 15f;

        protected Rect                      SelectionRect;
        protected Rect                      DisplayRect;
        protected static Vector2            ArrowImageSize          = new Vector2(10f, 10f);

        protected Vector2                   selectionScrollPos      = default(Vector2);
        protected Vector2                   displayScrollPos        = default(Vector2);

        public const float                  MinWidth                = 600f;
        public const float                  MinHeight               = 400f;
        public const float                  MinListWidth            = 200f;
        public float                        ActualHeight            = 9999f;

        private static string               _filterString           = "";
        private string                      _lastFilterString       = "";
        private int                         _lastFilterTick;
        private bool                        _filtered;

        public override MainTabWindowAnchor Anchor
        {
            get
            {
                return MainTabWindowAnchor.Right;
            }
        }

        public override Vector2             RequestedTabSize
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

        private MainTab_HelpMenuDef         TabDef
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
            readonly List<HelpCategoryDef>  helpCategories = new List<HelpCategoryDef>();

            public readonly string          ModName;

            public bool                     Expanded;

            public ModCategory( string modName )
            {
                ModName = modName;
            }

            public List<HelpCategoryDef>    HelpCategories
            {
                get
                {
                    return helpCategories.OrderBy(a => a.label).ToList();
                }
            }

            public bool                     ShouldDraw
            {
                get;
                set;
            }

            public bool                     MatchesFilter( string filter )
            {
                return (
                    ( filter == "" )||
                    ( ModName.ToUpper().Contains( filter.ToUpper() ) )
                );
            }

            public bool                     ThisOrAnyChildMatchesFilter( string filter )
            {
                return (
                    ( MatchesFilter( filter ) )||
                    ( HelpCategories.Any( hc => hc.ThisOrAnyChildMatchesFilter( filter ) ) )
                );
            }

            public void                     Filter( string filter )
            {
                ShouldDraw = ThisOrAnyChildMatchesFilter( filter );
                Expanded = (
                    ( filter != "" )&&
                    ( ThisOrAnyChildMatchesFilter( filter ) )
                );

                foreach( HelpCategoryDef hc in HelpCategories )
                {
                    hc.Filter( filter, MatchesFilter( filter ) );
                }
            }

            public float                    DrawHeight
            {
                get
                {
                    return Expanded
                        ? MainTabWindow_ModHelp.EntryHeight + HelpCategories.Where( hc => hc.ShouldDraw ).Sum( hc => hc.DrawHeight )
                            : EntryHeight;
                }
            }

            public void                     AddCategory( HelpCategoryDef def )
            {
                if( !helpCategories.Contains( def ) )
                {
                    helpCategories.Add( def );
                }
            }
        }

        #endregion

        #region Category Cache Control

        public override void                PreOpen()
        {
            base.PreOpen();

            //Set whether the window forces a pause
            if( TabDef != null )
            {
                this.forcePause = TabDef.pauseGame;
            }

            // Build the help system
            Recache();

            // set initial Filter
            Filter();
        }

        public static void                  Recache()
        {
            _cachedHelpCategories = new List<ModCategory>();
            foreach( var helpCategory in DefDatabase<HelpCategoryDef>.AllDefs )
            {
                // parent modcategory does not exist, create it.
                if( _cachedHelpCategories.All( t => t.ModName != helpCategory.ModName ) )
                {
                    var mCat = new ModCategory( helpCategory.ModName );
                    mCat.AddCategory( helpCategory );
                    _cachedHelpCategories.Add( mCat );
                }
                // add to existing modcategory
                else
                {
                    var mCat = _cachedHelpCategories.Find( t => t.ModName == helpCategory.ModName );
                    mCat.AddCategory( helpCategory );
                }
            }
        }

        #endregion

        #region Filter

        private void                        _filterUpdate()
        {
            // filter after a short delay.
            // Log.Message(_filterString + " | " + _lastFilterTick + " | " + _filtered);
            if( _filterString != _lastFilterString )
            {
                _lastFilterString = _filterString;
                _lastFilterTick = 0;
                _filtered = false;
            }
            else if( !_filtered )
            {
                if( _lastFilterTick > 60 )
                {
                    Filter();
                }
                _lastFilterTick++;
            }
        }

        public void                         Filter()
        {
            foreach( ModCategory mc in _cachedHelpCategories )
            {
                mc.Filter( _filterString );
            }
            _filtered = true;
        }

        #endregion


        #region OTab Rendering

        public override void                DoWindowContents( Rect rect )
        {
            base.DoWindowContents( rect );

            Text.Font = GameFont.Small;

            GUI.BeginGroup( rect );

            float selectionWidth = TabDef != null ? ( TabDef.listWidth >= MinListWidth ? TabDef.listWidth : MinListWidth ) : MinListWidth;
            SelectionRect = new Rect( 0f, 0f, selectionWidth, rect.height );
            DisplayRect = new Rect(
                SelectionRect.width + Margin, 0f,
                rect.width - SelectionRect.width - Margin, rect.height
            );

            DrawSelectionArea( SelectionRect );
            DrawDisplayArea( DisplayRect );

            GUI.EndGroup();
        }

        void                                DrawDisplayArea( Rect rect )
        {
            float paragraphMargin = 8f;
            float inset = 30f;

            Widgets.DrawMenuSection( rect );

            if( SelectedHelpDef == null )
            {
                return;
            }

            var titleRect = new Rect(rect.xMin, rect.yMin, rect.width, 60f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, SelectedHelpDef.LabelCap);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect outRect = rect.ContractedBy(Margin);
            outRect.yMin += 60f;
            Rect viewRect = outRect;
            viewRect.width -= 16f;
            viewRect.height = ActualHeight;

            GUI.BeginGroup(outRect);
            Widgets.BeginScrollView(outRect.AtZero(), ref displayScrollPos, viewRect.AtZero());

            Vector2 cur = Vector2.zero;

            HelpDetailSectionHelper.DrawText(ref cur, viewRect, SelectedHelpDef.description);

            cur.y += paragraphMargin;

            foreach (HelpDetailSection section in SelectedHelpDef.HelpDetailSections)
            {
                cur.x = 0f;
                if (!string.IsNullOrEmpty(section.Label))
                {
                    HelpDetailSectionHelper.DrawText(ref cur, viewRect, section.Label);
                    cur.x = inset;
                }
                if (section.StringDescs != null)
                {
                    foreach (string s in section.StringDescs)
                    {
                        HelpDetailSectionHelper.DrawText(ref cur, viewRect, s);
                    }
                }
                if (section.KeyDefs != null)
                {
                    foreach (DefStringTriplet defStringTriplet in section.KeyDefs)
                    {
                        if (HelpDetailSectionHelper.DrawDefLink(ref cur, viewRect, defStringTriplet))
                        {
                            // bit ugly, but since the helper can't return true if the helpdef doesn't exist, we can fetch it again here.
                            // TODO: better way of passing along helpdef. Perhaps make a resolve references step to add helpdef so we don't have to find it in realtime?
                            // TODO: open categories in selection area and scroll to correct position.
                            SelectedHelpDef =
                                DefDatabase<HelpDef>.AllDefsListForReading.First(hd => hd.keyDef == defStringTriplet.Def);
                        }
                    }
                }
                cur.y += paragraphMargin;
            }

            ActualHeight = cur.y + 60f;

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        void                                DrawSelectionArea( Rect rect )
        {
            // TODO: inset scrollbar a few pts.
            // TODO: adapt buttons to text height.
            Widgets.DrawMenuSection( rect );
            GUI.BeginGroup( rect );

            _filterUpdate();
            Rect filterRect = new Rect( Margin, Margin, rect.width - 3 * Margin - 30f, 30f );
            Rect clearRect = new Rect( filterRect.xMax + Margin + 3f, Margin + 3f, 24f, 24f );
            _filterString = Widgets.TextField( filterRect, _filterString );
            if( _filterString != "" )
            {
                if( Widgets.ImageButton( clearRect, Widgets.CheckboxOffTex ) )
                {
                    _filterString = "";
                    Filter();
                }
            }

            Rect outRect = rect.AtZero();
            outRect.yMin += 40f;
            float height = _cachedHelpCategories.Where( mc => mc.ShouldDraw ).Sum( c => c.DrawHeight );
            var viewRect = new Rect(
                0f, 0f,
                ( height > outRect.height ? outRect.width - 16f : outRect.width ), height
            );

            Widgets.BeginScrollView( outRect, ref selectionScrollPos, viewRect );
            if( _cachedHelpCategories.Count( mc => mc.ShouldDraw ) < 1 )
            {
                Rect messageRect = outRect.AtZero();
                Widgets.Label( messageRect, "NoHelpDefs".Translate() );
            }
            else
            {
                float curY = 0f;
                float curX = 0f;
                foreach( var mc in _cachedHelpCategories.Where( mc => mc.ShouldDraw ) )
                {
                    var entryRect = new Rect( 0f, curY, viewRect.width, mc.DrawHeight );
                    DrawModCategory( entryRect, curX, mc );
                    GUI.color = Color.gray;
                    Widgets.DrawLineHorizontal( 0f, curY, viewRect.width );
                    GUI.color = Color.white;
                    curY += mc.DrawHeight;
                }
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal( 0f, curY, viewRect.width );
                GUI.color = Color.white;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        void                                DrawModCategory( Rect entryRect, float curX, ModCategory mc )
        {
            GUI.BeginGroup( entryRect );

            float curY = 0f;
            var modRect = new Rect( 0f, 0f, entryRect.width, EntryHeight );
            DrawModRow( modRect, curX, mc );
            curY += EntryHeight;
            if( mc.Expanded )
            {
                foreach( var hc in mc.HelpCategories.Where( hc => hc.ShouldDraw ) )
                {
                    var catRect = new Rect( 0f, curY, entryRect.width, hc.DrawHeight );
                    DrawHelpCategory( catRect, curX + EntryIndent, hc );
                    curY += hc.DrawHeight;
                }
            }
            GUI.EndGroup();
        }

        void                                DrawModRow( Rect modRect, float curX, ModCategory mc )
        {
            GUI.BeginGroup( modRect );

            if( Mouse.IsOver( modRect ) )
            {
                Widgets.DrawHighlight( modRect );
            }

            var imageRect = new Rect(
                Margin + curX, modRect.height / 2f - ArrowImageSize.y / 2f,
                ArrowImageSize.x, ArrowImageSize.y
            );

            Texture2D texture = mc.Expanded ? Icon.HelpMenuArrowDown : Icon.HelpMenuArrowRight;
            GUI.DrawTexture( imageRect, texture );

            var labelRect = new Rect(
                imageRect.xMax + Margin, 0f,
                modRect.width - ArrowImageSize.x - Margin * 2 - curX, EntryHeight
            );

            Text.Anchor = TextAnchor.MiddleLeft;
            //GUI.color = Color.yellow;
            Widgets.Label( labelRect, mc.ModName );
            //GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            if( Widgets.InvisibleButton( modRect ) )
            {
                mc.Expanded = !mc.Expanded;
            }
            GUI.EndGroup();
        }

        void                                DrawHelpCategory( Rect catRect, float curX, HelpCategoryDef hc )
        {
            GUI.BeginGroup( catRect );

            var catRowRect = new Rect( 0f, 0f, catRect.width, EntryHeight );
            DrawHelpCategoryRow( catRowRect, curX, hc );

            if( hc.Expanded )
            {
                float curY = EntryHeight;
                foreach( var hd in hc.HelpDefs.Where( hd => hd.ShouldDraw ) )
                {
                    var helpRect = new Rect(
                        0f, curY,
                        catRect.width, EntryHeight
                    );

                    DrawHelpRow( helpRect, curX, hd );
                    GUI.color = Color.gray;
                    Widgets.DrawLineHorizontal( 0f, curY, catRect.width );
                    GUI.color = Color.white;

                    curY += EntryHeight;
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        void                                DrawHelpCategoryRow( Rect catRect, float curX, HelpCategoryDef hc )
        {
            GUI.BeginGroup( catRect );

            if( Mouse.IsOver( catRect ) )
            {
                Widgets.DrawHighlight( catRect );
            }

            var imageRect = new Rect(
                Margin + curX, catRect.height / 2f - ArrowImageSize.y / 2f,
                ArrowImageSize.x, ArrowImageSize.y
            );

            Texture2D texture = hc.Expanded ? Icon.HelpMenuArrowDown : Icon.HelpMenuArrowRight;
            GUI.DrawTexture( imageRect, texture );

            var labelRect = new Rect(
                imageRect.xMax + Margin, 0f,
                catRect.width - imageRect.width - Margin * 3 - curX, catRect.height
            );

            Text.Anchor = TextAnchor.MiddleLeft;

            if( Text.CalcHeight( hc.LabelCap, labelRect.width ) > EntryHeight)
            {
                Text.Font = GameFont.Tiny;
            }
            Widgets.Label( labelRect, hc.LabelCap );
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal( 0f, 0f, catRect.width );
            GUI.color = Color.white;

            if( Widgets.InvisibleButton( catRect ) )
            {
                hc.Expanded = !hc.Expanded;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }

        void                                DrawHelpRow( Rect hRect, float curX, HelpDef hd )
        {
            if( hd == SelectedHelpDef )
            {
                Widgets.DrawHighlightSelected( hRect );
            }
            else if( Mouse.IsOver( hRect ) )
            {
                Widgets.DrawHighlight( hRect );
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = hRect;
            labelRect.xMin += curX + Margin * 2 + ArrowImageSize.x;
            labelRect.width -= curX + Margin * 2 + ArrowImageSize.x;
            if( Text.CalcHeight( hd.LabelCap, labelRect.width ) > EntryHeight )
            {
                Text.Font = GameFont.Tiny;
            }
            Widgets.Label( labelRect, hd.LabelCap );
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            if( Widgets.InvisibleButton( hRect ) )
            {
                SelectedHelpDef = hd;
            }
        }

        #endregion

    }

}
