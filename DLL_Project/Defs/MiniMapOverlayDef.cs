using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class MiniMapOverlayDef : Def
    {
        #region XML Data

        // If more than one overlay is provided for a MiniMap, provide label or labelKey to translate
        public string               labelKey;

        public int                  drawOffset          = 0;

        public bool                 hiddenByDefault     = false;

        public Type                 overlayClass;

        // Can link to a specific minimap here
        public MiniMapDef           miniMapDef          = null;

        #endregion

    }

}
