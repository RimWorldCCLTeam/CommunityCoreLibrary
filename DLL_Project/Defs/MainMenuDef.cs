using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{
    
    public class MainMenuDef : Def
    {

        #region XML Data

        public string               labelKey;
        public int                  order;
        public Type                 menuClass;
        public bool                 closeMainTab = false;

        #endregion

        [Unsaved]

        #region Instance Data

        public IMainMenu            menuWorker;

        #endregion

        public override void PostLoad()
        {
            base.PostLoad();

            if( string.IsNullOrEmpty( labelKey ) )
            {
                if( string.IsNullOrEmpty( label ) )
                {
                    CCL_Log.TraceMod(
                        this,
                        Verbosity.FatalErrors,
                        "Def must have a valid label or labelKey for translation"
                    );
                    return;
                }
            }

            if( menuClass.GetInterface( "IMainMenu" ) == null )
            {
                CCL_Log.TraceMod(
                    this,
                    Verbosity.FatalErrors,
                    string.Format( "{0} is not a valid main menu class, does not implement IMainMenu", menuClass.ToString() )
                );
                return;
            }

            menuWorker = (IMainMenu) Activator.CreateInstance( menuClass );
            if( menuWorker == null )
            {
                CCL_Log.TraceMod(
                    this,
                    Verbosity.FatalErrors,
                    string.Format( "Unable to create instance of {0}", menuClass.ToString() ),
                    "MainMenuDef" );
                return;
            }

        }

    }

}
