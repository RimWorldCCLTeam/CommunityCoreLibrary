using UnityEngine;

namespace CommunityCoreLibrary
{

    public interface                        IMainMenu
    {

        bool                                RenderNow( bool anyWorldFiles, bool anyMapFiles );

        void                                ClickAcion();

    }

}
