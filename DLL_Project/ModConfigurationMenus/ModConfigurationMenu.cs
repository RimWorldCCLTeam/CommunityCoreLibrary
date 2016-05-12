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

        /// NOTE:  If your def is marked as PreloadMCMs, your Initialize()
        ///        and ExposeData() will be called before the injection
        ///        sequence starts.  Default behaviour is after the injection
        ///        sequence ends.  If will happen at one time or the other,
        ///        not both.

        /// <summary>
        /// Exposes the data.
        /// Called with Scribe.mode = LoadingVars after Initialize()
        /// Called with Scribe.mode = SavingVars when the main MCM is closed
        /// </summary>
        public abstract void                ExposeData();

        /// <summary>
        /// Initialize this instance.
        /// Called after the worker is created and before ExposeData() with Scribe.mode = LoadingVars
        /// </summary>
        public virtual void                 Initialize()
        {
        }

        /// <summary>
        /// Called after your MCM has been selected and before it is displayed.
        /// </summary>
        public virtual void                 PreOpen()
        {
        }

        /// <summary>
        /// Called after your MCM is closed (ie, another MCM is switched
        /// to or the main MCM window is closed while your MCM is active).
        /// </summary>
        public virtual void                 PostClose()
        {
        }

        /// <summary>
        /// The injection data, override MCMInjectionSet with your custom data.
        /// This field will hold the xml data.
        /// </summary>
        public MCMInjectionSet              InjectionSet;

    }

}
