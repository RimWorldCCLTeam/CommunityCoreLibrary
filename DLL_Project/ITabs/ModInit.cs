using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class ModInit : ITab
    {
        
        protected GameObject                gameObject;

        public ModInit()
        {
            if( gameObject == null )
            {
                gameObject = new GameObject( "CCLController" );
                gameObject.AddComponent< Controller >();
                Object.DontDestroyOnLoad( gameObject );
            }
        }

        protected override void             FillTab()
        {
        }

    }

}
