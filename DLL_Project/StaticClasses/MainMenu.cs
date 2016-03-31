using UnityEngine;

namespace CommunityCoreLibrary
{

    public abstract class                   MainMenu
    {

        public virtual bool                 RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return true;
        }

        public abstract void                ClickAction();

    }

}
