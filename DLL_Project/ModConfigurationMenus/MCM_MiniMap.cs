using System.Collections.Generic;

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
            }
            listing.End();
            return 0f;
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
