using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapController : MapComponent
    {
        #region Fields

        public static bool              dirty = true;
        public static bool              initialized = false;
        public static bool              visible = false;
        public static List<MiniMap>     visibleMiniMaps = new List<MiniMap>();
        public static Vector2           windowSize = new Vector2( 250f, 250f );

        #endregion Fields

        #region Properties

        private Window_MiniMap GetWindow
        {
            get
            {
                return Find.WindowStack.WindowOfType<Window_MiniMap>();
            }
        }

        #endregion Properties

        #region Methods

        public override void ExposeData()
        {
            base.ExposeData();
            foreach ( var minimap in Controller.Data.MiniMaps )
            {
                #region Minimap Header

                Scribe.EnterNode( minimap.miniMapDef.defName );

                #endregion Minimap Header

                var hidden = minimap.Hidden;
                Scribe_Values.LookValue( ref hidden, "hidden" );

                if ( Scribe.mode == LoadSaveMode.LoadingVars )
                {
                    minimap.Hidden = hidden;
                }

                #region Handle all MiniMap Overlays

                foreach ( var overlay in minimap.overlayWorkers )
                {
                    #region Overlay Header

                    Scribe.EnterNode( overlay.overlayDef.defName );

                    #endregion Overlay Header

                    hidden = overlay.Hidden;
                    Scribe_Values.LookValue( ref hidden, "hidden" );

                    if ( Scribe.mode == LoadSaveMode.LoadingVars )
                    {
                        overlay.Hidden = hidden;
                    }

                    #region Finalize Overlay

                    Scribe.ExitNode();

                    #endregion Finalize Overlay
                }

                #endregion Handle all MiniMap Overlays

                #region Finalize Minimap

                Scribe.ExitNode();

                #endregion Finalize Minimap
            }
        }

        public override void MapComponentOnGUI()
        {
            // No minimap
            if ( !visible )
            {
                // Close the window if needed
                if ( GetWindow != null )
                {
                    CloseWindow();
                }
                return;
            }

            // Initialize the map component
            if ( !initialized )
            {
                Initialize();
            }

            // Make sure the window is open
            if ( GetWindow == null )
            {
                if ( !OpenWindow() )
                {
                    return;
                }
            }

            // (Re-)sort minimaps
            if ( dirty )
            {
                SortMiniMaps();
            }

            // Update the minimaps
            UpdateMiniMaps();
        }

        private void CloseWindow()
        {
            var window = GetWindow;
            if ( window != null )
            {
                Find.WindowStack.TryRemove( window );
            }
        }

        private void Initialize()
        {
            // Sort minimaps in drawOrder
            SortMiniMaps();

            // ready to go
            initialized = true;

            // perform initial update for all overlays
            // do after initialization flag to avoid infinite loop.
            foreach ( var minimap in visibleMiniMaps )
            {
                minimap.Update();
            }
        }

        private bool OpenWindow()
        {
            var window = GetWindow;
            if ( window != null )
            {
                return true;
            }

            window = new Window_MiniMap( Window_MiniMap.windowRect )
            {
                currentWindowRect = new Rect( Screen.width - windowSize.x, 0f, windowSize.y, windowSize.x )
            };
            if ( window == null )
            {
                CCL_Log.Error( "Unable to create Window_MiniMap", "MiniMap" );
                return false;
            }
            Find.WindowStack.Add( window );
            return true;
        }

        private void SortMiniMaps()
        {
            // keep in mind that default sort ordering is FALSE > TRUE (which makes sense given the binary representation).
            visibleMiniMaps = Controller.Data.MiniMaps.Where( overlay => !overlay.Hidden )
                               .OrderBy( overlay => overlay.miniMapDef.alwaysOnTop )
                               .ThenBy( overlay => overlay.miniMapDef.drawOrder )
                               .ToList();

            dirty = false;
        }

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

        #endregion Methods
    }
}