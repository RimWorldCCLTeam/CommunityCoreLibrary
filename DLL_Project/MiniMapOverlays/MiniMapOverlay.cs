using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    
    public abstract class MiniMapOverlay // : IConfigurable
    {
        
        #region Instance Data

        // TODO:  Make overlays individually selectable, currently fixed at def default
        private bool                _hidden = false;

        private Texture2D           _texture;
        private MiniMap             minimap;
        public MiniMapOverlayDef    def;

        #endregion Instance Data

        #region Properties

        public Texture2D texture
        {
            get
            {
                if ( _texture == null )
                {
                    _texture = new Texture2D( MiniMap_Utilities.Size.x, MiniMap_Utilities.Size.z );

                    // not sure clearing pixels is strictly necessary, but can't hurt much
                    _texture.SetPixels( MiniMap_Utilities.GetClearPixelArray );
                    _texture.Apply();
                }
                return _texture;
            }
        }

        public virtual bool                 Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;

                // make entire minimap visible if any overlay is made visible
                if( !_hidden )
                {
                    minimap.Hidden = value;
                }

                // update overlay since this was paused while hidden
                Update();

                // mark the controller dirty so everything is properly sorted
                MiniMapController.dirty = true;
            }
        }

        public string               label
        {
            get
            {
                if( def.labelKey.NullOrEmpty() )
                {
                    return def.label;
                }
                return def.labelKey.Translate();
            }
        }

        public string               LabelCap
        {
            get
            {
                return label.CapitalizeFirst();
            }
        }

        #endregion

        public void ClearTexture( bool apply = false )
        {
            texture.SetPixels( MiniMap_Utilities.GetClearPixelArray );
            if ( apply )
                texture.Apply();
        }

        #region Constructors

        public                      MiniMapOverlay( MiniMap minimap, MiniMapOverlayDef overlayDef )
        {
            this.minimap = minimap;
            this.def = overlayDef;
            _hidden = overlayDef.hiddenByDefault;
        }

        #endregion Constructors

        #region Methods

        // Required, update your texture, caller (MiniMap.Update()) will re-Apply()
        public abstract void        Update();

        // Optional float menu option for this overlay
        public virtual List<FloatMenuOption>  GetFloatMenuOptions()
        {
            // create empty list of options
            var options = new List<FloatMenuOption>();

            // add a toggle option for overlays that are not always on, and only if there's more than one overlay on this minimap 'mode'
            if ( !minimap.miniMapDef.alwaysVisible && minimap.overlayWorkers.Count > 1 )
                options.Add( new FloatMenuOption( "MiniMap.ToggleX".Translate( LabelCap ), delegate
                { Hidden = !Hidden; } ) );

            // done!
            return options;
        }

        #endregion Methods
    }
}