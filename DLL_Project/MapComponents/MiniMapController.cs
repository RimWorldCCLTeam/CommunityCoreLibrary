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
        
        public static bool              dirty = true;
        public static bool              initialized = false;
        public static bool              visible = false;
        public static List<MiniMap>     visibleMiniMaps = new List<MiniMap>();
        public static Vector2           windowSize = new Vector2( 250f, 250f );

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
            if( Scribe.mode == LoadSaveMode.Saving )
            {
                ExposeDataSave();
            }
            else if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                ExposeDataLoad();
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
            UpdateMiniMaps( true );
        }

        #endregion

        #region Update Methods

        private bool ShouldUpdateMiniMap( MiniMap minimap )
        {
            return (
                ( minimap.miniMapDef.updateInterval > 0 )&&
                ( ( Time.frameCount + minimap.GetHashCode() ) % minimap.miniMapDef.updateInterval == 0 )
            );
        }

        private bool ShouldUpdateOverlay( MiniMapOverlay overlay )
        {
            return (
                ( overlay.overlayDef.updateInterval > 0 )&&
                ( ( Time.frameCount + overlay.GetHashCode() ) % overlay.overlayDef.updateInterval == 0 )
            );
        }

        private void UpdateMiniMaps( bool forceUpdate = false )
        {
            // Update minimaps and overlays, stagger it out a bit so stuff doesn't all get updated at the same time if they have the same interval
            foreach( var minimap in visibleMiniMaps )
            {
                // Update minimap
                if(
                    ( forceUpdate )||
                    ( ShouldUpdateMiniMap( minimap ) )
                )
                {
                    minimap.Update();
                }
                // Update overlays for minimap 
                var workers = minimap.VisibleOverlays;
                if( workers.Any() )
                {
                    foreach( var overlay in workers )
                    {
                        if(
                            ( forceUpdate )||
                            ( ShouldUpdateOverlay( overlay ) )
                        )
                        {
                            overlay.Update();
                            overlay.texture.Apply();
                        }
                    }
                }
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

        #region Save/Load Minimap and Overlays

        private void ExposeDataSave()
        {
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                #region Minimap Header

                Scribe.EnterNode( minimap.miniMapDef.defName );

                #endregion

                var hidden = minimap.Hidden;
                Scribe_Values.LookValue( ref hidden, "hidden" );

                #region Handle all MiniMap Overlays

                foreach( var overlay in minimap.overlayWorkers )
                {
                    #region Overlay Header

                    Scribe.EnterNode( overlay.overlayDef.defName );

                    #endregion

                    hidden = overlay.Hidden;
                    Scribe_Values.LookValue( ref hidden, "hidden" );

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

        private void ExposeDataLoad()
        {
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                if( !XmlNodeExists( Scribe.curParent, minimap.miniMapDef.defName ) )
                {   // No saved data for this minimap
                    continue;
                }

                #region Minimap Header

                Scribe.EnterNode( minimap.miniMapDef.defName );

                #endregion

                bool hidden = true;
                Scribe_Values.LookValue( ref hidden, "hidden" );
                minimap.Hidden = hidden;

                #region Handle all MiniMap Overlays

                foreach( var overlay in minimap.overlayWorkers )
                {

                    if( !XmlNodeExists( Scribe.curParent, overlay.overlayDef.defName ) )
                    {   // No saved data for this overlay
                        continue;
                    }

                    #region Overlay Header

                    Scribe.EnterNode( overlay.overlayDef.defName );

                    #endregion

                    Scribe_Values.LookValue( ref hidden, "hidden" );
                    overlay.Hidden = hidden;

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

        private bool XmlNodeExists( System.Xml.XmlNode parentNode, string name )
        {
            var xmlEnumerator = parentNode.ChildNodes.GetEnumerator();
            while( xmlEnumerator.MoveNext() )
            {
                var node = (System.Xml.XmlNode) xmlEnumerator.Current;
                if( node.Name == name )
                {   // Node exists
                    return true;
                }
            }
            return false;
        }

        #endregion

    }

}