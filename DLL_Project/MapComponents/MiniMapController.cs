using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

    public class MiniMapController : MapComponent
    {

        #region Fields

        public const float              MINWINDOWSIZE = 40f;
        public const float              DEFAULTWINDOWSIZE = 250f;
        
        public static Rect              defaultWindowRect;

        public static bool              dirty = true;
        public static bool              initialized = false;
        public static bool              visible = false;
        public static List<MiniMap>     visibleMiniMaps = new List<MiniMap>();

        private static string           regExPattern = "\\W";
        private static Regex            regEx;

        #endregion Fields

        #region Constructors

        static                          MiniMapController()
        {
            defaultWindowRect = new Rect( Screen.width - DEFAULTWINDOWSIZE, 0f, DEFAULTWINDOWSIZE, DEFAULTWINDOWSIZE );
            regEx = new Regex( regExPattern );
        }

        public MiniMapController() : base( null)  // Core cares about this apparently
        {
            Window_MiniMap.minimapRect = defaultWindowRect;
        }

        public                          MiniMapController( Map map ) : base( map )
        {
            Window_MiniMap.minimapRect = defaultWindowRect;
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

        public override void            MapComponentUpdate()
        {
            // No minimap
            if (!visible || WorldRendererUtility.WorldRenderedNow )
            {
                // Close the window if needed
                if (GetWindow != null)
                {
                    CloseWindow();
                }
                return;
            }
        }

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
                var rectStr = string.Format(
                    "({0},{1},{2},{3})",
                    Window_MiniMap.minimapRect.x,
                    Window_MiniMap.minimapRect.y,
                    Window_MiniMap.minimapRect.width,
                    Window_MiniMap.minimapRect.height
                );
                Scribe_Values.LookValue( ref rectStr, "minimapRect" );
            }
            else if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                Scribe_Values.LookValue( ref Window_MiniMap.minimapRect, "minimapRect" );
                var window = GetWindow;
                if(
                    ( visible )&&
                    ( window != null )
                )
                {
                    window.windowRect = Window_MiniMap.minimapRect;
                }
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

            window = new Window_MiniMap();
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

        public static string            GenSaveKey( string inputString )
        {
            // Remove non-valid xml tag characters from the string
            var regExed = regEx.Replace( inputString, "" );
            //Log.Message( string.Format( "GenSaveKey( '{0}' ) = '{1}'", inputString, regExed ) );
            return regExed;
        }

        private void                    ExposeDataSave()
        {
            bool hidden;
            foreach( var minimap in Controller.Data.MiniMaps )
            {

                #region Minimap Header
                if( !Scribe.EnterNode( minimap.SaveKey ) )
                {
                    continue;
                }
                #endregion

                hidden = minimap.Hidden;
                Scribe_Values.LookValue( ref hidden, "hidden", minimap.miniMapDef.hiddenByDefault, true );

                #region Handle all MiniMap Overlays

                foreach( var overlay in minimap.overlayWorkers )
                {
                    #region Overlay Header
                    var saveKey = overlay.SaveKey;
                    if(
                        ( string.IsNullOrEmpty( saveKey ) )||
                        ( !Scribe.EnterNode( overlay.SaveKey ) )
                    )
                    {
                        continue;
                    }
                    #endregion

                    hidden = overlay.Hidden;
                    Scribe_Values.LookValue( ref hidden, "hidden", overlay.overlayDef.hiddenByDefault, true );

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
                
                #region Minimap Header
                if( !Scribe.EnterNode( minimap.SaveKey ) )
                {   // No saved data for this minimap
                    continue;
                }
                #endregion

                Scribe_Values.LookValue( ref hidden, "hidden", minimap.miniMapDef.hiddenByDefault, true );
                minimap.Hidden = hidden;

                if( minimap.miniMapDef.dynamicOverlays )
                {   // Rebuild overlays for minimap
                    minimap.Reset();
                }

                #region Handle all MiniMap Overlays

                foreach( var overlay in minimap.overlayWorkers )
                {
                    #region Overlay Header
                    var saveKey = overlay.SaveKey;
                    if(
                        ( string.IsNullOrEmpty( saveKey ) )||
                        ( !Scribe.EnterNode( saveKey ) )
                    )
                    {   // No saved data for this overlay
                        continue;
                    }
                    #endregion

                    Scribe_Values.LookValue( ref hidden, "hidden", overlay.overlayDef.hiddenByDefault, true );
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