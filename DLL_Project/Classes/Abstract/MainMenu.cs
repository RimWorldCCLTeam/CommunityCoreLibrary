using UnityEngine;

namespace CommunityCoreLibrary
{

    public abstract class                   MainMenu
    {

        public virtual Color                Color
        {
            get
            {
                return Color.white;
            }
        }

        public virtual bool                 RenderNow( bool anyMapFiles )
        {
            return true;
        }

        public abstract void                ClickAction();

    }

}
