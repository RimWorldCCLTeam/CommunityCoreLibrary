using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{
    public class MiniMapController : MapComponent
    {
        #region Fields

        public static bool          dirty = true;
        public bool                 hidden = false;
        public bool                 initialized = false;
        public List<MiniMap>        miniMaps = new List<MiniMap>();
        public Vector2              size = new Vector2( 250f, 250f );
        public List<MiniMap>        visibleMiniMaps = new List<MiniMap>();

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Base Overrides

        public override void MapComponentOnGUI()
        {
            // actual drawing logic is handled in the window's DoWindowContent();
            AddWindowIfNecessary();

            // set some global vars
            if ( !initialized )
                Initialize();

            // (re-)sort overlays
            if ( dirty )
                SortOverlays();

            // draw overlays
            foreach ( var overlay in visibleMiniMaps )
            {
                // update overlay grid, stagger it out a bit so stuff doesn't all get updated at the same time if they have the same interval
                if ( ( Time.frameCount + overlay.GetHashCode() ) % overlay.def.updateInterval == 0 )
                {
                    overlay.Update();
                }
            }
        }

        #endregion

        #region Methods

        private void AddWindowIfNecessary()
        {
            if( hidden )
            {
                return;
            }
            var window = Find.WindowStack.WindowOfType<Window_MiniMap>();
            if( window == null )
            {
                Window_MiniMap.windowRect = new Rect( Screen.width - size.x, 0f, size.y, size.x );
                window = new Window_MiniMap( Window_MiniMap.windowRect, this );
                if( window == null )
                {
                    CCL_Log.Error( "Unable to create Window_MiniMap", "MiniMap" );
                    return;
                }
                Find.WindowStack.Add( window );
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

        private void Initialize()
        {
            var errors = false;
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            var miniMapDefs = DefDatabase<MiniMapDef>.AllDefsListForReading;
            foreach( var miniMapDef in miniMapDefs )
            {
                if(
                    ( miniMapDef.miniMapClass == null )||
                    (
                        ( miniMapDef.miniMapClass != typeof( MiniMap ) )&&
                        ( !miniMapDef.miniMapClass.IsSubclassOf( typeof( MiniMap ) ) )
                    )
                )
                {
                    CCL_Log.TraceMod(
                        miniMapDef,
                        Verbosity.NonFatalErrors,
                        string.Format( "Unable to resolve miniMapClass for '{0}' to 'CommunityCoreLibrary.MiniMap'", miniMapDef.defName )
                    );
                    errors = true;
                }
                else
                {
                    miniMapDef.miniMapWorker = (MiniMap) Activator.CreateInstance( miniMapDef.miniMapClass, new System.Object[] { miniMapDef } );
                    if( miniMapDef.miniMapWorker == null )
                    {
                        CCL_Log.TraceMod(
                            miniMapDef,
                            Verbosity.NonFatalErrors,
                            string.Format( "Unable to create instance of '{0}' for '{1}'", miniMapDef.miniMapClass.Name, miniMapDef.defName )
                        );
                        errors = true;
                    }
                    else
                    {
                        miniMapDef.miniMapWorker.def = miniMapDef;
                        miniMaps.Add( miniMapDef.miniMapWorker );
                    }
                }
            }

            // quickly add some test overlays
            // TODO: Migrate to def-based system
            /*
            overlays.Add( new MiniMap_Fog( this ) );
            overlays.Add( new MiniMap_ViewPort( this ) );
            overlays.Add( new MiniMap_Colonists( this ) );
            overlays.Add( new MiniMap_Wildlife( this ) );
            overlays.Add( new MiniMap_NonColonistPawns( this ) );
            overlays.Add( new MiniMap_PowerGrid( this ) );
            // overlays.Add( new MiniMap_Terrain( this ) );
            // overlays.Add( new MiniMap_Minerals( this ) );
            */

            // sort them in drawOrder
            SortOverlays();

            // ready to go
            initialized = true;

            // perform initial update for all overlays
            // do after initialization flag to avoid infinite loop.
            foreach( var overlay in visibleMiniMaps )
            {
                overlay.Update();
            }

            CCL_Log.CaptureEnd( stringBuilder, errors ? "Initialization Errors" : "Initialized" );
            if( errors )
            {
                CCL_Log.Error( stringBuilder.ToString(), "MiniMap" );
            }
            else
            {
                CCL_Log.Message( stringBuilder.ToString(), "MiniMap" );
            }
        }

        private void SortOverlays()
        {
            // keep in mind that default sort ordering is FALSE > TRUE (which makes sense given the binary representation).
            visibleMiniMaps = miniMaps.Where( overlay => !overlay.Hidden )
                               .OrderBy( overlay => overlay.def.alwaysOnTop )
                               .ThenBy( overlay => overlay.def.drawOrder )
                               .ToList();

            dirty = false;
        }

        #endregion Methods
    }
}