using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public class Loader : ITab
    {

        public                              Loader()
        {
#if DEVELOPER
            CCL_Log.OpenStream();
#endif

            if( Controller.Data.UnityObject == null )
            {
                Controller.Data.UnityObject = new GameObject( Controller.Data.UnityObjectName );
                Controller.Data.UnityObject.AddComponent< Controller.MainMonoBehaviour >();
                Object.DontDestroyOnLoad( Controller.Data.UnityObject );
                Controller.Data.cclMonoBehaviour = Controller.Data.UnityObject.GetComponent< Controller.MainMonoBehaviour >();
                Controller.Data.cclMonoBehaviour.Initialize();
            }
        }

        protected override void             FillTab()
        {
        }

    }

}
