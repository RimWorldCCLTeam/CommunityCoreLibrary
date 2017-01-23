using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMap_Areas : MiniMap
    {
        #region Constructors

        public MiniMap_Areas( MiniMapDef def ) : base( def ) { }

        #endregion Constructors

        #region Methods
        
        public override void Reset()
        {
            overlayWorkers.Clear();
            UpdateAreaOverlays();
        }

        public override void Update()
        {
            UpdateAreaOverlays();
        }

        private void UpdateAreaOverlays()
        {
            // get area manager
            var manager = Find.VisibleMap == null? null : Find.VisibleMap.areaManager;
            if( manager == null )
            {
                return;
            }

            // since this is called every frame (FRAME, not tick), just checking area count should be good enough to detect changes.
            if ( manager.AllAreas.Count == overlayWorkers.Count )
            {
                return;
            }

            // check if we need to add area overlays
            foreach( var area in manager.AllAreas )
            {
                if ( !overlayWorkers.Any( w => ((MiniMapOverlay_Area)w).area == area ) )
                    overlayWorkers.Add( new MiniMapOverlay_Area( this, new MiniMapOverlayDef(), area ) );
            }

            // check if we should remove area overlays
            if ( overlayWorkers?.Any() ?? false )
            {
                for ( int i = overlayWorkers.Count - 1; i >= 0; i-- )
                {
                    MiniMapOverlay_Area overlay = overlayWorkers[i] as MiniMapOverlay_Area;
                    if ( overlay == null || !manager.AllAreas.Contains( overlay.area ) )
                        overlayWorkers.RemoveAt( i );
                }
            }
        }

        #endregion Methods
    }
}