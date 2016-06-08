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

        public const float              MINWINDOWSIZE = 40f;
        public const float              DEFAULTWINDOWSIZE = 250f;

        public static bool              defaultVisible = false;
        public static Rect              defaultWindowRect;

        public static bool              dirty = true;
        public static bool              initialized = false;
        public static bool              visible;
        public static List<MiniMap>     visibleMiniMaps = new List<MiniMap>();

        #endregion Fields

        #region Constructors

        static                          MiniMapController()
        {
            defaultWindowRect = new Rect( Screen.width - DEFAULTWINDOWSIZE, 0f, DEFAULTWINDOWSIZE, DEFAULTWINDOWSIZE );
        }

        public                          MiniMapController()
        {
            Window_MiniMap.windowRect = defaultWindowRect;
            visible = defaultVisible;
        }

        #endregion

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

        public override void            ExposeData()
        {
            base.ExposeData();

            Scribe_Values.LookValue( ref visible, "visible" );
            if( Scribe.mode == LoadSaveMode.Saving )
            {   // Scribing directly as a rect causing extra formatting '(x:#, y:#, width:#, height:#)' which throws errors on load
                var rectStr = "(" +
                    Window_MiniMap.windowRect.x + "," +
                    Window_MiniMap.windowRect.y + "," +
                    Window_MiniMap.windowRect.width + "," +
                    Window_MiniMap.windowRect.height + ")";
                Scribe_Values.LookValue( ref rectStr, "windowRect" );
            }
            else if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                Scribe_Values.LookValue( ref Window_MiniMap.windowRect, "windowRect" );
            }

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

        private void                    Initialize()
        {
            // Sort minimaps in drawOrder
            SortMiniMaps();

            // ready to go
            initialized = true;
        }

        #endregion

        #region Update Methods

        private bool                    ShouldUpdateMiniMap( MiniMap minimap )
        {
            return (
                ( minimap.dirty )||
                (
                    ( minimap.miniMapDef.updateInterval > 0 )&&
                    ( ( Time.frameCount + minimap.GetHashCode() ) % minimap.miniMapDef.updateInterval == 0 )
                )
            );
        }

        private bool                    ShouldUpdateOverlay( MiniMapOverlay overlay )
        {
            return (
                ( overlay.dirty )||
                (
                    ( overlay.overlayDef.updateInterval > 0 )&&
                    ( ( Time.frameCount + overlay.GetHashCode() ) % overlay.overlayDef.updateInterval == 0 )
                )
            );
        }

        private void                    UpdateMiniMaps()
        {
            // Update minimaps and overlays, stagger it out a bit so stuff doesn't all get updated at the same time if they have the same interval
            foreach( var minimap in visibleMiniMaps )
            {
                // Update minimap
                if( ShouldUpdateMiniMap( minimap ) )
                {
                    minimap.Update();
                    minimap.dirty = false;
                }
                // Update overlays for minimap 
                var workers = minimap.VisibleOverlays;
                if( workers.Any() )
                {
                    foreach( var overlay in workers )
                    {
                        if( ShouldUpdateOverlay( overlay ) )
                        {
                            overlay.Update();
                            overlay.texture.Apply();
                            overlay.dirty = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Open/Close Window

        private void                    CloseWindow()
        {
            var window = GetWindow;
            if( window != null )
            {
                Find.WindowStack.TryRemove( window );
            }
        }

        private bool                    OpenWindow()
        {
            var window = GetWindow;
            if( window != null )
            {
                return true;
            }

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

        private void                    SortMiniMaps()
        {
            // keep in mind that default sort ordering is FALSE > TRUE (which makes sense given the binary representation).
            visibleMiniMaps = Controller.Data.MiniMaps.Where( overlay => !overlay.Hidden )
                               .OrderBy( overlay => overlay.miniMapDef.alwaysOnTop )
                               .ThenBy( overlay => overlay.miniMapDef.drawOrder )
                               .ToList();

            dirty = false;
        }

        #endregion

        #region Find a Minimap or Overlay

        public static MiniMap           FindMinimap( Type minimapType )
        {
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                if( minimap.GetType() == minimapType )
                {
                    return minimap;
                }
            }
            return null;
        }

        public static MiniMap           FindMinimap( MiniMapDef minimapDef )
        {
            return FindMinimap( minimapDef.defName );
        }

        public static MiniMap           FindMinimap( string minimapDefName )
        {
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                if( minimap.miniMapDef.defName == minimapDefName )
                {
                    return minimap;
                }
            }
            return null;
        }

        public static MiniMapOverlay    FindOverlay( Type overlayType )
        {
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                foreach( var overlay in minimap.overlayWorkers )
                {
                    if( overlay.GetType() == overlayType )
                    {
                        return overlay;
                    }
                }
            }
            return null;
        }

        public static MiniMapOverlay    FindOverlay( MiniMapOverlayDef overlayDef )
        {
            return FindOverlay( overlayDef.defName );
        }

        public static MiniMapOverlay    FindOverlay( string overlayDefName )
        {
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                foreach( var overlay in minimap.overlayWorkers )
                {
                    if( overlay.overlayDef.defName == overlayDefName )
                    {
                        return overlay;
                    }
                }
            }
            return null;
        }

        #endregion

        #region Save/Load Minimap and Overlays

        private void                    ExposeDataSave()
        {
            bool hidden;
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                // Note: Minimaps with dynamic overlays break scribing because they have
                // a machine generated defName which may not be the same after load.
                if( minimap.miniMapDef.dynamicOverlays )
                {
                    continue;
                }

                #region Minimap Header

                Scribe.EnterNode( minimap.miniMapDef.defName );

                #endregion

                hidden = minimap.Hidden;
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

        private void                    ExposeDataLoad()
        {
            bool hidden = true; // Don't really need to set this but the compiler complains if we don't
            foreach( var minimap in Controller.Data.MiniMaps )
            {
                if(
                    ( minimap.miniMapDef.dynamicOverlays )||
                    ( !Scribe.curParent.HasChildNode( minimap.miniMapDef.defName ) )
                )
                {   // Dynamic minimap overlays or no saved data for this minimap
                    continue;
                }

                #region Minimap Header

                Scribe.EnterNode( minimap.miniMapDef.defName );

                #endregion

                Scribe_Values.LookValue( ref hidden, "hidden" );
                minimap.Hidden = hidden;

                #region Handle all MiniMap Overlays

                foreach( var overlay in minimap.overlayWorkers )
                {

                    if( !Scribe.curParent.HasChildNode( overlay.overlayDef.defName ) )
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

        #endregion

    }

}