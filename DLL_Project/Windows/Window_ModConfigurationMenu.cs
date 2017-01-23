using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class Window_ModConfigurationMenu : Window
    {

        #region Control Constants

        public const float MinListWidth = 200f;

        public const float WindowMargin = 6f; // 15 is way too much.
        public const float EntryHeight = 30f;

        #endregion

        #region Instance Data

        protected Rect SelectionRect;
        protected Rect DisplayRect;

        protected Vector2 SelectionScrollPos = default( Vector2 );
        protected Vector2 DisplayScrollPos = default( Vector2 );

        public float SelectionHeight = 9999f;
        public float ContentHeight = 9999f;

        private MCMHost SelectedHost;
        private MCMHost PreviouslySelectedHost;

        private static string _filterString = "";
        private string _lastFilterString = "";
        private int _lastFilterTick;
        private bool _filtered;
        private List<MCMHost> filteredHosts;

        #endregion

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2( 600f, 600f );
            }
        }

        #region Constructor

        private void cTor_Common()
        {
            layer = WindowLayer.Dialog;
            soundAppear = null;
            soundClose = null;
            doCloseButton = false;
            doCloseX = true;
            closeOnEscapeKey = true;
            forcePause = true;
            filteredHosts = Controller.Data.MCMHosts.ListFullCopy();
        }

        public Window_ModConfigurationMenu()
        {
            cTor_Common();
        }

        public Window_ModConfigurationMenu( ModConfigurationMenu selectedMenu )
        {
            cTor_Common();
            this.SelectedHost = filteredHosts.Find( host => host.worker == selectedMenu );
        }

        #endregion

        #region Window PreClose

        public override void PreClose()
        {
            if( SelectedHost != null )
            {
                SelectedHost.worker.PostClose();
            }
            base.PreClose();

            for( int index = 0; index < Controller.Data.MCMHosts.Count; ++index )
            {
                // Get host to work with
                var host = Controller.Data.MCMHosts[ index ];

                if( host.OpenedThisSession )
                {
                    MCMHost.SaveHostData( host );
                }
            }

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
            filteredHosts.Clear();
            foreach( var host in Controller.Data.MCMHosts )
            {
                if( string.IsNullOrEmpty( _filterString ) )
                {
                    filteredHosts.Add( host );
                }
                else if( host.Label.Contains( _filterString ) )
                {
                    filteredHosts.Add( host );
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

        #region Window Rendering

        public override void DoWindowContents( Rect inRect )
        {
            if( Current.ProgramState == ProgramState.Entry )
            {
                absorbInputAroundWindow = true;
            }
            else
            {
                absorbInputAroundWindow = false;
            }

            Text.Font = GameFont.Small;

            GUI.BeginGroup( inRect );

            SelectionRect = new Rect( 0f, 0f, MinListWidth, inRect.height );
            DisplayRect = new Rect(
                SelectionRect.width + WindowMargin, 0f,
                inRect.width - SelectionRect.width - WindowMargin, inRect.height
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
            var filterRect = new Rect( rect.xMin + WindowMargin, rect.yMin + WindowMargin, rect.width - 3 * WindowMargin - 30f, 30f );
            var clearRect = new Rect( filterRect.xMax + WindowMargin + 3f, rect.yMin + WindowMargin + 3f, 24f, 24f );
            _filterString = Widgets.TextField( filterRect, _filterString );
            if( _filterString != "" )
            {
                if( Widgets.ButtonImage( clearRect, Widgets.CheckboxOffTex ) )
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

            if( !filteredHosts.NullOrEmpty() )
            {
                Vector2 cur = Vector2.zero;

                foreach( var host in filteredHosts )
                {
                    if( DrawHost( ref cur, viewRect, host ) )
                    {
                        SelectedHost = host;
                    }
                }

                SelectionHeight = cur.y;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private bool DrawHost( ref Vector2 cur, Rect view, MCMHost host )
        {
            float width = view.width - cur.x - WindowMargin;
            float height = EntryHeight;
            string label = host.Label;

            if( Text.CalcHeight( label, width ) > EntryHeight )
            {
                Text.Font = GameFont.Tiny;
                float height2 = Text.CalcHeight( label, width );
                height = Mathf.Max( height, height2 );
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = new Rect( cur.x + WindowMargin, cur.y, width - WindowMargin, height );
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
            if( SelectedHost == host )
            {
                Widgets.DrawHighlightSelected( buttonRect );
            }
            else
            {
                Widgets.DrawHighlightIfMouseover( buttonRect );
            }
            return Widgets.ButtonInvisible( buttonRect );
        }

        #endregion

        #region Mod Configuration Menu Rendering

        void DrawDisplayArea( Rect rect )
        {
            Widgets.DrawMenuSection( rect );

            if( SelectedHost == null )
            {
                return;
            }
            if(
                ( PreviouslySelectedHost != null )&&
                ( PreviouslySelectedHost != SelectedHost )
            )
            {
                PreviouslySelectedHost.worker.PostClose();
            }
            if( PreviouslySelectedHost != SelectedHost )
            {
                SelectedHost.OpenedThisSession = true;
                SelectedHost.worker.PreOpen();
            }
            PreviouslySelectedHost = SelectedHost;

            Text.Font = GameFont.Medium;
            Text.WordWrap = false;
            var titleRect = new Rect( rect.xMin, rect.yMin, rect.width, 60f );
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label( titleRect, SelectedHost.Label );

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            Rect outRect = rect.ContractedBy( WindowMargin );
            outRect.yMin += 60f;
            Rect viewRect = outRect;
            viewRect.width -= 16f;
            viewRect.height = ContentHeight;

            GUI.BeginGroup( outRect );
            Widgets.BeginScrollView( outRect.AtZero(), ref DisplayScrollPos, viewRect.AtZero() );

            bool userError = false;
            string userErrorStr = string.Empty;
            try
            {
                ContentHeight = SelectedHost.worker.DoWindowContents( viewRect.AtZero() );
            }
            catch( Exception e )
            {
                userError = true;
                userErrorStr = e.ToString();
            }

            Widgets.EndScrollView();
            GUI.EndGroup();

            if( userError )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    userErrorStr,
                    "Mod Configuration Menu"
                );
            }
        }

        #endregion

    }

}
