using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class ModInit : ITab
    {

        public static GameObject            gameObject;

        public ModInit()
        {
            if( gameObject == null )
            {
                gameObject = new GameObject( "CCLController" );
                gameObject.AddComponent< ModController >();
                Object.DontDestroyOnLoad( gameObject );
            }
        }

        protected override void             FillTab()
        {
        }

    }

}
