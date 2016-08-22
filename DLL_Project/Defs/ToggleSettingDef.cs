using System;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{
    
    public class ToggleSettingDef : Def
    {

        #region XML Data

        public bool                 enableButton = true;
        public bool                 exposeValue = true;

        public string               labelKey;
        public string               saveKey;

        public string               tutorTag;
        public string               iconTexture;
        public SoundDef             soundDef;

        public int                  order;

        public Type                 toggleClass;

        #endregion

        [Unsaved]

        #region Instance Data

        public Texture2D            icon;
        public ToggleSetting        toggleWorker;

        #endregion

        public virtual string       Label
        {
            get
            {
                if( string.IsNullOrEmpty( labelKey ) )
                {
                    return label;
                }
                return labelKey.Translate();
            }
        }

        public override void        PostLoad()
        {
            base.PostLoad();

            if( enableButton )
            {
                if(
                    ( string.IsNullOrEmpty( labelKey ) )&&
                    ( string.IsNullOrEmpty( label ) )
                )
                {
                    CCL_Log.TraceMod(
                        this,
                        Verbosity.NonFatalErrors,
                        "Def must have a valid label or labelKey for translation"
                    );
                    label = defName;
                }
                if( string.IsNullOrEmpty( iconTexture ) )
                {
                    CCL_Log.TraceMod(
                        this,
                        Verbosity.FatalErrors,
                        "Def must have a valid icon texture if enableButton is true"
                    );
                }
            }

            if(
                ( exposeValue )&&
                ( string.IsNullOrEmpty( saveKey ) )
            )
            {
                CCL_Log.TraceMod(
                    this,
                    Verbosity.FatalErrors,
                    "Def must have a valid saveKey if exposeValue is true, must not be null"
                );
            }

            if( !toggleClass.IsSubclassOf( typeof( ToggleSetting ) ) )
            {
                CCL_Log.TraceMod(
                    this,
                    Verbosity.FatalErrors,
                    string.Format( "{0} is not a valid toggle class, does not inherit from ToggleSetting", toggleClass.ToString() )
                );
            }
            else
            {
                toggleWorker = (ToggleSetting) Activator.CreateInstance( toggleClass );
                if( toggleWorker == null )
                {
                    CCL_Log.TraceMod(
                        this,
                        Verbosity.FatalErrors,
                        string.Format( "Unable to create instance of {0}", toggleClass.ToString() ),
                        "ToggleSettingDef" );
                }
            }

            if(
                ( enableButton )&&
                ( !string.IsNullOrEmpty( iconTexture ) )
            )
            {
                LongEventHandler.ExecuteWhenFinished( () =>
                {
                    icon = ContentFinder<Texture2D>.Get( iconTexture );
                } );
            }

        }

    }

}
