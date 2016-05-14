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
                listing.OverrideColumnWidth = rect.width;

                #region Main Description
                var descLabel = "MiniMap.MCMDescription".Translate();
                listing.DoLabel( descLabel );
                listing.DoGap();
                #endregion

                #region Main Toggle
                var toggleLabel = "MiniMap.MCMToggleMain".Translate();
                listing.DoLabelCheckbox( toggleLabel, ref MiniMap.MiniMapController.visible );
                listing.DoGap();
                #endregion

                #region Handle all MiniMaps and Overlays

                foreach( var minimap in Controller.Data.MiniMaps )
                {
                    bool drewMinimapHeader = false;
                    var iMinimap = minimap as IConfigurable;

                    #region Handle MiniMap IConfigurable
                    if( iMinimap != null )
                    {
                        #region Minimap Header
                        MinimapHeader( listing, minimap );
                        drewMinimapHeader = true;
                        #endregion

                        #region Minimap IConfigurable
                        var iMinimapRect = new Rect( listing.Indentation(), listing.CurHeight, listing.ColumnWidth(), 9999f );
                        GUI.BeginGroup( iMinimapRect );
                        var iMinimapHeight = iMinimap.DrawMCMRegion( iMinimapRect.AtZero() );
                        GUI.EndGroup();
                        listing.DoGap( iMinimapHeight + 6f );
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

                            #region Draw MiniMap Header if needed
                            if( !drewMinimapHeader )
                            {
                                #region Minimap Header
                                MinimapHeader( listing, minimap );
                                drewMinimapHeader = true;
                                #endregion
                            }
                            #endregion

                            #region Overlay Header
                            OverlayHeader( listing, overlay );
                            #endregion

                            #region Overlay IConfigurable
                            var iOverlayRect = new Rect( listing.Indentation(), listing.CurHeight, listing.ColumnWidth(), 9999f );
                            GUI.BeginGroup( iOverlayRect );
                            var iOverlayHeight = iOverlay.DrawMCMRegion( iOverlayRect.AtZero() );
                            GUI.EndGroup();
                            listing.DoGap( iOverlayHeight + 6f );
                            listing.Undent();
                            #endregion

                        }
                        #endregion
                    }
                    #endregion

                    #region Final Undentation
                    if( drewMinimapHeader )
                    {
                        listing.DoGap();
                        listing.Undent();
                    }
                    #endregion
                }

                #endregion

            }
            listing.End();
            return listing.CurHeight;
        }

        private void MinimapHeader( Listing_Standard listing, MiniMap.MiniMap minimap )
        {
            var str = minimap.LabelCap;
            listing.DoLabel( str );
            listing.DoGap( 6 );
            listing.Indent();
        }

        private void OverlayHeader( Listing_Standard listing, MiniMap.MiniMapOverlay overlay )
        {
            var str = overlay.LabelCap;
            listing.DoLabel( str );
            listing.DoGap( 6 );
            listing.Indent();
        }

        /// <summary>
        /// Exposes the data.
        /// Called with Scribe.mode = LoadingVars after Initialize()
        /// Called with Scribe.mode = SavingVars when the main MCM is closed
        /// </summary>
        public override void                ExposeData()
        {

        }

    }

}
