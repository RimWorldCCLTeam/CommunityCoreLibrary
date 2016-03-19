using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class Window_ModConfigurationMenu : Window
    {

        private class MenuWorkers
        {
            public string                   Label;
            public ModConfigurationMenu     worker;
        }

        #region Control Constants

        public const float                  MinListWidth            = 200f;

        public const float                  Margin                  = 6f; // 15 is way too much.
        public const float                  EntryHeight             = 30f;

        #endregion

        #region Instance Data

        protected Rect                      SelectionRect;
        protected Rect                      DisplayRect;

        protected Vector2                   SelectionScrollPos      = default(Vector2);
        protected Vector2                   DisplayScrollPos        = default(Vector2);

        public float                        SelectionHeight         = 9999f;
        public float                        ContentHeight           = 9999f;

        private MenuWorkers                 SelectedMenu;

        private static string               _filterString           = "";
        private string                      _lastFilterString       = "";
        private int                         _lastFilterTick;
        private bool                        _filtered;
        private List< MenuWorkers >         filteredMenus;

        private static List< MenuWorkers >  allMenus;

        public static bool                  AnyMenus
        {
            get
            {
                return !allMenus.NullOrEmpty();
            }
        }

        #endregion

        public override Vector2 InitialWindowSize
        {
            get
            {
                return new Vector2( 600f, 400f );
            }
        }

        #region Constructor

        static Window_ModConfigurationMenu()
        {
            allMenus = new List<MenuWorkers>();

            // Get the mods with config menus
            foreach( var mhd in Controller.Data.ModHelperDefs )
            {
                if( !mhd.ConfigurationWindows.NullOrEmpty() )
                {
                    foreach( var mcm in mhd.ConfigurationWindows )
                    {
                        var menu = new MenuWorkers();
                        menu.Label = mcm.label;
                        menu.worker = (ModConfigurationMenu) Activator.CreateInstance( mcm.Window );
                        if( menu.worker == null )
                        {
                            CCL_Log.Error( "Unable to create instance of {0}", mcm.Window.ToString() );
                        }
                        else
                        {
                            allMenus.Add( menu );
                        }
                    }
                }
            }

        }

        public Window_ModConfigurationMenu()
        {
            layer = WindowLayer.GameUI;
            soundAppear = null;
            soundClose = null;
            doCloseButton = false;
            doCloseX = true;
            closeOnEscapeKey = true;
            forcePause = true;
            filteredMenus = allMenus.ListFullCopy();
        }

        #endregion

        #region Filter

        private void _filterUpdate()
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

        public void Filter()
        {
            filteredMenus.Clear();
            foreach( var menu in allMenus )
            {
                if( string.IsNullOrEmpty( _filterString  ) )
                {
                    filteredMenus.Add( menu );
                }
                else if( menu.Label.Contains( _filterString ) )
                {
                    filteredMenus.Add( menu );
                }
            }
            _filtered = true;
        }

        public void ResetFilter()
        {
            _filterString = "";
            _lastFilterString = "";
            Filter();
        }

        #endregion

        #region ITab Rendering

        public override void DoWindowContents( Rect rect )
        {
            if( Game.Mode == GameMode.Entry )
            {
                absorbInputAroundWindow = true;
            }
            else
            {
                absorbInputAroundWindow = false;
            }

            Text.Font = GameFont.Small;

            GUI.BeginGroup( rect );

            SelectionRect = new Rect( 0f, 0f, MinListWidth, rect.height );
            DisplayRect = new Rect(
                SelectionRect.width + Margin, 0f,
                rect.width - SelectionRect.width - Margin, rect.height
            );

            DrawSelectionArea( SelectionRect );
            DrawDisplayArea( DisplayRect );

            GUI.EndGroup();
        }

        #endregion

        #region Selection Area Rendering

        void DrawSelectionArea( Rect rect )
        {
            Widgets.DrawMenuSection( rect );

            _filterUpdate();
            var filterRect = new Rect( rect.xMin + Margin, rect.yMin + Margin, rect.width - 3 * Margin - 30f, 30f );
            var clearRect = new Rect( filterRect.xMax + Margin + 3f, rect.yMin + Margin + 3f, 24f, 24f );
            _filterString = Widgets.TextField( filterRect, _filterString );
            if( _filterString != "" )
            {
                if( Widgets.ImageButton( clearRect, Widgets.CheckboxOffTex ) )
                {
                    ResetFilter();
                }
            }

            Rect outRect = rect;
            outRect.yMin += 40f;
            outRect.xMax -= 2f; // some spacing around the scrollbar

            float viewWidth = SelectionHeight > outRect.height ? outRect.width - 16f : outRect.width;
            var viewRect = new Rect( 0f, 0f, viewWidth, SelectionHeight );

            GUI.BeginGroup( outRect );
            Widgets.BeginScrollView( outRect.AtZero(), ref SelectionScrollPos, viewRect );

            if( !filteredMenus.NullOrEmpty() )
            {
                Vector2 cur = Vector2.zero;

                foreach( var menu in filteredMenus )
                {
                    if( DrawModEntry( ref cur, viewRect, menu ) )
                    {
                        SelectedMenu = menu;
                    }
                }

                SelectionHeight = cur.y;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private bool DrawModEntry( ref Vector2 cur, Rect view, MenuWorkers menu )
        {
            float   width       = view.width - cur.x - Margin;
            float   height      = EntryHeight;
            string  label       = menu.Label;

            if( Text.CalcHeight( label, width ) > EntryHeight )
            {
                Text.Font = GameFont.Tiny;
                float height2 = Text.CalcHeight( label, width );
                height = Mathf.Max( height, height2 );
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect( cur.x + Margin, cur.y, width - Margin, height );
            Widgets.Label( labelRect, label );
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // full viewRect width for overlay and button
            Rect buttonRect = view;
            buttonRect.yMin = cur.y;
            cur.y += height;
            buttonRect.yMax = cur.y;
            GUI.color = Color.grey;
            Widgets.DrawLineHorizontal( view.xMin, cur.y, view.width );
            GUI.color = Color.white;
            if( SelectedMenu == menu )
            {
                Widgets.DrawHighlightSelected( buttonRect );
            }
            else
            {
                Widgets.DrawHighlightIfMouseover( buttonRect );
            }
            return Widgets.InvisibleButton( buttonRect );
        }

        #endregion

        #region Mod Configuration Menu Rendering

        void DrawDisplayArea( Rect rect )
        {
            Widgets.DrawMenuSection( rect );

            if( SelectedMenu == null )
            {
                return;
            }

            Text.Font = GameFont.Medium;
            Text.WordWrap = false;
            var titleRect = new Rect( rect.xMin, rect.yMin, rect.width, 60f );
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label( titleRect, SelectedMenu.Label );

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            Rect outRect = rect.ContractedBy( Margin );
            outRect.yMin += 60f;
            Rect viewRect = outRect;
            viewRect.width -= 16f;
            viewRect.height = ContentHeight;

            GUI.BeginGroup( outRect );
            Widgets.BeginScrollView( outRect.AtZero(), ref DisplayScrollPos, viewRect.AtZero() );

            ContentHeight = SelectedMenu.worker.DoWindowContents( viewRect );

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        #endregion

    }

}
