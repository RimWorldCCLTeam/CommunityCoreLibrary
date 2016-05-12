using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class MiniMapOverlayData
    {
        #region XML Data

        // If more than one overlay is provided for a MiniMap, provide one or the other of label or labelKey
        public string               label;
        public string               labelKey;

        public int                  drawOffset          = 0;

        public bool                 hiddenByDefault     = false;

        public Type                 overlayClass;

        #endregion

    }

    public class MiniMapDef : Def
    {

        #region XML Data

        public string               labelKey            = string.Empty;

        public int                  updateInterval      = 250;
        public int                  drawOrder           = 100;

        public bool                 hiddenByDefault     = false;
        public bool                 alwaysVisible       = false;
        public bool                 alwaysOnTop         = false;

        // if alwaysVisible == false, no icon is needed and is ignored
        // true, toggle icon is required
        public string               iconTex             = string.Empty;

        public Type                 miniMapClass        = null;

        public List<MiniMapOverlayData> overlays            = null;

        #endregion

        #region Instance Data

        public MiniMap              miniMapWorker       = null;

        #endregion

    }

}
