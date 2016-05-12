using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{
    
    public abstract class MiniMapOverlay
    {
        
        #region Instance Data

        private bool                _hidden = false;

        public Texture2D            texture;
        private MiniMap             minimap;
        public MiniMapOverlayData   overlayData;

        #endregion Instance Data

        #region Properties

        public bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
                if( !_hidden )
                {
                    minimap.Hidden = value;
                }
                MiniMapController.dirty = true;
            }
        }

        public string LabelCap
        {
            get
            {
                if( overlayData.labelKey.NullOrEmpty() )
                {
                    return overlayData.label.CapitalizeFirst();
                }
                return overlayData.label.Translate().CapitalizeFirst();
            }
        }

        #endregion

        #region Constructors

        public MiniMapOverlay( MiniMap minimap, MiniMapOverlayData overlayData )
        {
            this.minimap = minimap;
            this.overlayData = overlayData;

            // create texture
            texture = new Texture2D( MiniMap.Size.x, MiniMap.Size.z );
            if( texture == null )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Unable to create texture for '{0}'", this.GetType().Name )
                );
                return;
            }

            // transparent.... for now!
            for ( int x = 0; x < MiniMap.Size.x; x++ )
                for ( int z = 0; z < MiniMap.Size.z; z++ )
                    texture.SetPixel( x, z, Color.clear );

            // apply changes
            texture.Apply();
        }

        #endregion Constructors

        #region Methods

        public abstract void Update();

        #endregion Methods
    }
}