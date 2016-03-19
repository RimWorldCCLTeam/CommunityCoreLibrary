using UnityEngine;

namespace CommunityCoreLibrary
{

    public abstract class                   ModConfigurationMenu
    {

        /// <summary>
        /// Draws the mod configuration window contents.
        /// </summary>
        /// <returns>The final height of the window rect.</returns>
        /// <param name="rect">Rect</param>
        public abstract float               DoWindowContents( Rect rect );

    }

}
