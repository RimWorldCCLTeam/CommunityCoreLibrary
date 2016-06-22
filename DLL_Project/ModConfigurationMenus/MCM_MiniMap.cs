using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MCM_MiniMap : ModConfigurationMenu
    {

        /// <summary>
        /// Draws the mod configuration window contents.
        /// </summary>
        /// <returns>The final height of the window rect.</returns>
        /// <param name="rect">Rect</param>
        public override float               DoWindowContents( Rect rect )
        {
            var listing = new Listing_Standard( rect );
            {
                listing.ColumnWidth = rect.width - 4f;

                #region Main Toggle
                var toggleLabel = "MiniMap.MCMToggleMain".Translate();
                listing.CheckboxLabeled( toggleLabel, ref MiniMap.MiniMapController.defaultVisible );
                listing.Gap();
                #endregion

                #region Handle all MiniMaps and Overlays

                foreach( var minimap in Controller.Data.MiniMaps )
                {
                    if( minimap.IsOrHasIConfigurable )
                    {
                        var iMinimap = minimap as IConfigurable;

                        #region Minimap Header
                        MinimapHeader( listing, minimap );
                        #endregion

                        #region Handle MiniMap IConfigurable
                        if( iMinimap != null )
                        {
                            #region Minimap IConfigurable
                            var iMinimapRect = new Rect( listing.Indentation(), listing.CurHeight, listing.ColumnWidth(), 9999f );
                            GUI.BeginGroup( iMinimapRect );
                            var iMinimapHeight = iMinimap.DrawMCMRegion( iMinimapRect.AtZero() );
                            GUI.EndGroup();
                            listing.Gap( iMinimapHeight + 6f );
                            #endregion
                        }
                        #endregion

                        #region Handle all MiniMap Overlays
                        foreach( var overlay in minimap.overlayWorkers )
                        {
                            var iOverlay = overlay as IConfigurable;

                            #region Handle Overlay IConfigurable
                            if( iOverlay != null )
                            {

                                #region Overlay Header
                                OverlayHeader( listing, overlay );
                                #endregion

                                #region Overlay IConfigurable
                                var iOverlayRect = new Rect( listing.Indentation(), listing.CurHeight, listing.ColumnWidth(), 9999f );
                                GUI.BeginGroup( iOverlayRect );
                                var iOverlayHeight = iOverlay.DrawMCMRegion( iOverlayRect.AtZero() );
                                GUI.EndGroup();
                                listing.Gap( iOverlayHeight + 6f );
                                listing.Undent();
                                #endregion

                            }
                            #endregion
                        }
                        #endregion

                        #region Final Undentation
                        listing.Gap();
                        listing.Undent();
                        #endregion
                    }
                }

                #endregion

            }
            listing.End();
            return listing.CurHeight;
        }

        private void MinimapHeader( Listing_Standard listing, MiniMap.MiniMap minimap )
        {
            var str = minimap.LabelCap;
            listing.Label( str );
            listing.Gap( 6 );
            listing.Indent();
        }

        private void OverlayHeader( Listing_Standard listing, MiniMap.MiniMapOverlay overlay )
        {
            var str = overlay.LabelCap;
            listing.Label( str );
            listing.Gap( 6 );
            listing.Indent();
        }

        /// <summary>
        /// Exposes the data.
        /// Called with Scribe.mode = LoadingVars after Initialize()
        /// Called with Scribe.mode = SavingVars when the main MCM is closed
        /// </summary>
        public override void                ExposeData()
        {

            Scribe_Values.LookValue( ref MiniMap.MiniMapController.defaultVisible, "visible" );

            #region Handle all MiniMaps and Overlays

            foreach( var minimap in Controller.Data.MiniMaps )
            {
                if( minimap.IsOrHasIConfigurable )
                {
                    #region Minimap Header
                    Scribe.EnterNode( minimap.miniMapDef.defName );
                    #endregion

                    #region Handle MiniMap IConfigurable
                    var iMinimap = minimap as IConfigurable;
                    if( iMinimap != null )
                    {
                        iMinimap.ExposeData();
                    }
                    #endregion

                    #region Handle all MiniMap Overlays
                    foreach( var overlay in minimap.overlayWorkers )
                    {
                        var iOverlay = overlay as IConfigurable;
                        if( iOverlay != null )
                        {
                            #region Overlay Header
                            Scribe.EnterNode( overlay.overlayDef.defName );
                            #endregion

                            #region Handle Overlay IConfigurable
                            iOverlay.ExposeData();
                            #endregion

                            #region Finalize Overlay
                            Scribe.ExitNode();
                            #endregion
                        }
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

}
