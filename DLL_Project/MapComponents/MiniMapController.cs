using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

    public class MiniMapController : MapComponent
    {

        #region Fields
        public static bool              visible = false;

        public static bool              dirty = true;
        public static bool              initialized = false;
        public static Vector2           windowSize = new Vector2( 250f, 250f );
        public static List<MiniMap>     visibleMiniMaps = new List<MiniMap>();

        #endregion Fields

        #region Properties

        private Window_MiniMap          GetWindow
        {
            get
            {
                return Find.WindowStack.WindowOfType<Window_MiniMap>();
            }
        }

        #endregion Properties

        #region Base Overrides

        public override void            MapComponentOnGUI()
        {
            // No minimap
            if( !visible )
            {
                // Close the window if needed
                if( GetWindow != null )
                {
                    CloseWindow();
                }
                return;
            }

            // Initialize the map component
            if( !initialized )
            {
                Initialize();
            }

            // Make sure the window is open
            if( GetWindow == null )
            {
                if( !OpenWindow() )
                {
                    return;
                }
            }

            // Update the window
            UpdateWindow();

            // (Re-)sort minimaps
            if( dirty )
            {
                SortMiniMaps();
            }

            // Update the minimaps
            UpdateMiniMaps();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                #region Minimap Header
                Scribe.EnterNode( minimap.miniMapDef.defName );
                #endregion

                var hidden = minimap.Hidden;
                Scribe_Values.LookValue( ref hidden, "hidden" );

                if( Scribe.mode == LoadSaveMode.LoadingVars )
                {
                    minimap.Hidden = hidden;
                }

                #region Handle all MiniMap Overlays
                foreach( var overlay in minimap.overlayWorkers )
                {
                    #region Overlay Header
                    Scribe.EnterNode( overlay.overlayDef.defName );
                    #endregion

                    hidden = overlay.Hidden;
                    Scribe_Values.LookValue( ref hidden, "hidden" );

                    if( Scribe.mode == LoadSaveMode.LoadingVars )
                    {
                        overlay.Hidden = hidden;
                    }

                    #region Finalize Overlay
                    Scribe.ExitNode();
                    #endregion
                }
                #endregion

                #region Finalize Minimap
                Scribe.ExitNode();
                #endregion
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // Sort minimaps in drawOrder
            SortMiniMaps();

            // ready to go
            initialized = true;

            // perform initial update for all overlays
            // do after initialization flag to avoid infinite loop.
            foreach( var minimap in visibleMiniMaps )
            {
                minimap.Update();
            }
        }

        #endregion

        #region Update Methods

        private void UpdateMiniMaps()
        {
            // draw overlays
            foreach ( var minimap in visibleMiniMaps )
            {
                // update overlay grid, stagger it out a bit so stuff doesn't all get updated at the same time if they have the same interval
                if ( ( Time.frameCount + minimap.GetHashCode() ) % minimap.miniMapDef.updateInterval == 0 )
                {
                    minimap.Update();
                }
            }
        }

        private void UpdateWindow()
        {
            var window = GetWindow;
            if( window == null )
            {
                return;
            }

            if( Prefs.DevMode )
            {   // Adjust window position to accomodate the dev icon bar on smaller resolutions
                var devIconBarStart = Screen.width * 0.6666667f;
                int num = 6;
                if (Game.Mode == GameMode.MapPlaying)
                {
                    num += 2;
                }
                var devIconBarSize = ( num * 28.0 - 4.0 + 1.0 );
                var devIconBarRight = devIconBarStart + devIconBarSize;
                var windowX = window.currentWindowRect.x;
                if( devIconBarRight >= windowX )
                {
                    Window_MiniMap.windowRect.y = 0f;
                    var offsetFromTop = 3f + 25f + 3f;
                    if(
                        ( Game.Mode == GameMode.MapPlaying )&&
                        ( Game.GodMode )&&
                        ( devIconBarStart + 60f >= windowX )
                    )
                    {
                        offsetFromTop+= 15f + 3f;
                    }
                    Window_MiniMap.windowRect.y += offsetFromTop;
                }
            }
            if( window.currentWindowRect != Window_MiniMap.windowRect )
            {
                window.currentWindowRect = Window_MiniMap.windowRect;
            }
        }

        #endregion

        #region Open/Close Window

        private void CloseWindow()
        {
            var window = GetWindow;
            if( window != null )
            {
                Find.WindowStack.TryRemove( window );
            }
        }

        private bool OpenWindow()
        {
            var window = GetWindow;
            if( window != null )
            {
                return true;
            }

            Window_MiniMap.windowRect = new Rect( Screen.width - windowSize.x, 0f, windowSize.y, windowSize.x );
            window = new Window_MiniMap( Window_MiniMap.windowRect );
            if( window == null )
            {
                CCL_Log.Error( "Unable to create Window_MiniMap", "MiniMap" );
                return false;
            }
            Find.WindowStack.Add( window );
            return true;
        }

        #endregion

        #region Sort MiniMaps

        private void SortMiniMaps()
        {
            // keep in mind that default sort ordering is FALSE > TRUE (which makes sense given the binary representation).
            visibleMiniMaps = Controller.Data.MiniMaps.Where( overlay => !overlay.Hidden )
                               .OrderBy( overlay => overlay.miniMapDef.alwaysOnTop )
                               .ThenBy( overlay => overlay.miniMapDef.drawOrder )
                               .ToList();

            dirty = false;
        }

        #endregion

    }

}