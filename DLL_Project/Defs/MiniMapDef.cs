using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

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

        // Can link overlays directly here
        public List<MiniMapOverlayDef> overlays         = null;

        // Undecided if this is the best place/way to hook this
        // Optional link to MCM handler (must be declared in ModHelperDef)
        public Type                 mcmClass            = null;

        #endregion

        #region Instance Data

        public MiniMap              miniMapWorker       = null;

        public ModConfigurationMenu mcmWorker           = null;

        #endregion

    }

}
