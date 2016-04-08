using System;

using Verse;

namespace CommunityCoreLibrary
{

    public class CompProperties_HideItem : CompProperties
    {
        
        #region XML Data

        public bool                         preventItemSelection = false;

        #endregion

        public                              CompProperties_HideItem()
        {
            compClass = typeof( CompHideItem );
        }

    }

}
