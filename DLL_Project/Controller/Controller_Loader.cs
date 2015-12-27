using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public class Loader : ITab
    {

        public                              Loader()
        {
            if( Controller.Data.UnityObject == null )
            {
                Controller.Data.UnityObject = new GameObject( Controller.Data.UnityObjectName );
                Controller.Data.UnityObject.AddComponent< Controller.MainMonoBehaviour >();
                Object.DontDestroyOnLoad( Controller.Data.UnityObject );
            }
        }

        protected override void             FillTab()
        {
        }

    }

}
