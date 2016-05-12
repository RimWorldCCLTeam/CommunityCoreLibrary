using UnityEngine;

namespace CommunityCoreLibrary
{

    // TODO:  Fill this out

    public class                            MCM_MiniMap_Pawns : ModConfigurationMenu
    {

        /// <summary>
        /// Draws the mod configuration window contents.
        /// </summary>
        /// <returns>The final height of the window rect.</returns>
        /// <param name="rect">Rect</param>
        public override float               DoWindowContents( Rect rect )
        {
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
