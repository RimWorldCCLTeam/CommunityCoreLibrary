using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static class MainMenuDrawer_Extensions
    {

        #region Public Constants

        public const float          GameRectWidth = 200f;
        public const float          NewsRectWidth = 350f;

        public const int            MinButCount = 3;

        public const float          TitleShift = 50f;
        public const float          TitlePaneSpacing = 10f;

        public const float          CreditHeight = 30f;
        public const float          CreditTitleSpacing = 3f;

        public const float          LudeonEdgeSpacing = 8f;

        public const float          OptionsEdgeSpacing = 30f;
        public const float          OptionListSpacing = 17f;
        public const float          OptionSpacingDefault = 7f;
        public const float          OptionButtonHeightDefault = 45f;
        public const float          LinkOptionMinHeight = 24f;

        public const float          LanguageOptionWidth = 64f;
        public const float          LanguageOptionHeight = 32f;
        public const float          LanguageOptionSpacing = 10f;

        #endregion

        #region Internal Reflection Data

        private static Type         _Verse_TexButton;

        private static MethodInfo   _CloseMainTab;

        private static FieldInfo    _anyMapFiles;

        private static FieldInfo    _PaneSize;
        private static FieldInfo    _TitleSize;

        private static FieldInfo    _IconBlog;
        private static FieldInfo    _IconForums;
        private static FieldInfo    _IconTwitter;
        private static FieldInfo    _IconBook;
        private static FieldInfo    _IconSoundtrack;

        private static FieldInfo    _TexTitle;

        private static FieldInfo    _TexLudeonLogo;
        private static FieldInfo    _LudeonLogoSize;

        private static List<MainMenuDef>  _MainMenuDefs;

        private static List<ListableOption> _linkOptions;

        #endregion

        #region Constructor

        static                      MainMenuDrawer_Extensions()
        {
            // Fetch internal Verse.TexButton class
            _Verse_TexButton = Controller.Data.Assembly_CSharp.GetType( "Verse.TexButton" );
            if( _Verse_TexButton == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get class 'Verse.TexButton' in 'Assembly_CSharp'",
                    "MainMenuDrawer_Extensions" );
            }

            _CloseMainTab   = typeof( MainMenuDrawer ).GetMethod( "CloseMainTab", Controller.Data.UniversalBindingFlags );
            if( _CloseMainTab == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'CloseMainTab' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }

            _anyMapFiles    = typeof( MainMenuDrawer ).GetField( "anyMapFiles", Controller.Data.UniversalBindingFlags );
            if( _anyMapFiles == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'anyMapFiles' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }

            _PaneSize       = typeof( MainMenuDrawer ).GetField( "PaneSize", Controller.Data.UniversalBindingFlags );
            if( _PaneSize == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'PaneSize' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }
            _TitleSize      = typeof( MainMenuDrawer ).GetField( "TitleSize", Controller.Data.UniversalBindingFlags );
            if( _TitleSize == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'TitleSize' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }

            _IconBlog       = _Verse_TexButton.GetField( "IconBlog", Controller.Data.UniversalBindingFlags );
            if( _IconBlog == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'IconBlog' in 'TexButton'",
                    "MainMenuDrawer_Extensions" );
            }
            _IconForums     = _Verse_TexButton.GetField( "IconForums", Controller.Data.UniversalBindingFlags );
            if( _IconForums == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'IconForums' in 'TexButton'",
                    "MainMenuDrawer_Extensions" );
            }
            _IconTwitter    = _Verse_TexButton.GetField( "IconTwitter", Controller.Data.UniversalBindingFlags );
            if( _IconTwitter == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'IconTwitter' in 'TexButton'",
                    "MainMenuDrawer_Extensions" );
            }
            _IconBook       = _Verse_TexButton.GetField( "IconBook", Controller.Data.UniversalBindingFlags );
            if( _IconBook == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'IconBook' in 'TexButton'",
                    "MainMenuDrawer_Extensions" );
            }
            _IconSoundtrack = _Verse_TexButton.GetField( "IconSoundtrack", Controller.Data.UniversalBindingFlags );
            if( _IconSoundtrack == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'IconSoundtrack' in 'TexButton'",
                    "MainMenuDrawer_Extensions" );
            }

            _TexTitle       = typeof( MainMenuDrawer ).GetField( "TexTitle", Controller.Data.UniversalBindingFlags );
            if( _TexTitle == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'TexTitle' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }

            _TexLudeonLogo  = typeof( MainMenuDrawer ).GetField( "TexLudeonLogo", Controller.Data.UniversalBindingFlags );
            if( _TexLudeonLogo == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'TexLudeonLogo' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }
            _LudeonLogoSize = typeof( MainMenuDrawer ).GetField( "LudeonLogoSize", Controller.Data.UniversalBindingFlags );
            if( _LudeonLogoSize == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'LudeonLogoSize' in 'MainMenuDrawer'",
                    "MainMenuDrawer_Extensions" );
            }

        }

        #endregion

        #region Properties Reflecting Original Fields

        public static Vector2       ScreenCentre
        {
            get
            {
                return new Vector2( (float) Screen.width / 2f, (float) Screen.height / 2f );
            }
        }

        public static bool          AnyMapFiles
        {
            get
            {
                return (bool) _anyMapFiles.GetValue( null );
            }
        }

        public static Vector2       PaneSize
        {
            get
            {
                return (Vector2) _PaneSize.GetValue( null );
            }
            set
            {
                _PaneSize.SetValue( null, value );
            }
        }

        public static Vector2       TitleSize
        {
            get
            {
                return (Vector2) _TitleSize.GetValue( null );
            }
        }

        public static Vector2       LudeonLogoSize
        {
            get
            {
                return (Vector2) _LudeonLogoSize.GetValue( null );
            }
        }

        public static Texture2D     TexTitle
        {
            get
            {
                return (Texture2D) _TexTitle.GetValue( null );
            }
        }

        public static Texture2D     TexLudeonLogo
        {
            get
            {
                return (Texture2D) _TexLudeonLogo.GetValue( null );
            }
        }

        public static Texture2D     IconBlog
        {
            get
            {
                return (Texture2D) _IconBlog.GetValue( null );
            }
        }

        public static Texture2D     IconForums
        {
            get
            {
                return (Texture2D) _IconForums.GetValue( null );
            }
        }

        public static Texture2D     IconTwitter
        {
            get
            {
                return (Texture2D) _IconTwitter.GetValue( null );
            }
        }

        public static Texture2D     IconBook
        {
            get
            {
                return (Texture2D) _IconBook.GetValue( null );
            }
        }

        public static Texture2D     IconSoundtrack
        {
            get
            {
                return (Texture2D) _IconSoundtrack.GetValue( null );
            }
        }

        #endregion

        #region Close In-Game Main Tab

        public static void              CloseMainTab()
        {
            _CloseMainTab.Invoke( null, null );
        }

        #endregion

        #region Sorted, Translated Main Menu Defs

        public static List<MainMenuDef> ValidMainMenuDefs
        {
            get
            {
                if( _MainMenuDefs == null )
                {
                    // Get all defs which have a valid label and menu worker
                    _MainMenuDefs = DefDatabase< MainMenuDef >.AllDefsListForReading.Where( def => (
                        /*(  Don't worry about labels here in case language keys failed to load
                            ( !string.IsNullOrEmpty( def.label ) )||
                            (
                                ( !string.IsNullOrEmpty( def.labelKey ) )&&
                                ( def.labelKey.CanTranslate() )
                            )
                        )&&*/
                        ( def.menuWorker != null )
                    ) ).ToList();

                    // Sort defs by order
                    _MainMenuDefs.Sort( (x, y) => x.order > y.order ? -1 : 1 );

                }
                return _MainMenuDefs;
            }
        }

        public static List<MainMenuDef> CurrentMainMenuDefs( bool anyMapFiles )
        {
            return ValidMainMenuDefs.Where( def => (
                (
                    ( !Controller.Data.RequireRestart )||
                    (
                        ( Controller.Data.PlayWithoutRestart )||
                        ( def.showIfRestartRequired )
                    )
                )&&
                ( def.menuWorker.RenderNow( anyMapFiles ) )
            ) ).ToList();
        }

        #endregion

        #region Link Options

        public static List<ListableOption> LinkOptions
        {
            get
            {
                if( _linkOptions == null )
                {
                    _linkOptions = new List<ListableOption>()
                    {
                        MainMenuDrawer_LinkOptions.FictionPrimerOption,
                        MainMenuDrawer_LinkOptions.BlogOption,
                        MainMenuDrawer_LinkOptions.ForumsOption,
                        MainMenuDrawer_LinkOptions.WikiOption,
                        MainMenuDrawer_LinkOptions.TwitterOption,
                        MainMenuDrawer_LinkOptions.DesignBookOption,
                        MainMenuDrawer_LinkOptions.TranslateOption,
                        MainMenuDrawer_LinkOptions.BuySoundTrack
                    };
                }
                return _linkOptions;
            }
        }

        #endregion

        #region Spacing Helpers

        public static float OptionButtonSpacingFor( int count, float buttonHeight = OptionButtonHeightDefault, float spacing = OptionSpacingDefault )
        {
            return 
                count * buttonHeight +
                ( count - 1 ) * spacing;
        }

        public static float LinkOptionsHeight
        {
            get
            {
                float y = 0f;
                foreach( var option in LinkOptions )
                {
                    var link = option as ListableOption_WebLink;
                    float width1 = (float) ( GameRectWidth - (float) link.image.width - 3.0f );
                    float num = Text.CalcHeight( link.label, width1 );
                    float height = Mathf.Max( LinkOptionMinHeight, num ) + OptionSpacingDefault;
                    y += height;
                }
                return y;
            }
        }

        #endregion

        #region Small Area Renders

        public static void      DrawLanguageOption( Rect rect )
        {
            if( Current.ProgramState == ProgramState.Entry )
            {
                var lang = LanguageDatabase.activeLanguage;
                if( lang.icon.NullOrBad() )
                {   // Try fix bas flag texture
                    var fileInfo = new FileInfo( Path.Combine( "Mods", Path.Combine( "Core", Path.Combine( "Languages", Path.Combine( lang.folderName.ToString(), "LangIcon.png" ) ) ) ) );
                    if( fileInfo.Exists )
                    {
                        LanguageDatabase.activeLanguage.icon = ModContentLoader<Texture2D>.LoadItem( fileInfo.FullName ).contentItem;
                    }
                }
                if(
                    Widgets.ButtonImage(
                        rect,
                        LanguageDatabase.activeLanguage.icon )
                )
                {
                    var languageOptions = new List<FloatMenuOption>();
                    foreach( LoadedLanguage selectedLanguage in LanguageDatabase.AllLoadedLanguages )
                    {
                        languageOptions.Add(
                            new FloatMenuOption(
                                selectedLanguage.FriendlyNameNative,
                                () =>
                        {
                            if( selectedLanguage != Verse.LanguageDatabase.activeLanguage )
                            {
                                // Only reload if it changed
                                LanguageDatabase.SelectLanguage( selectedLanguage );
                                Prefs.Save();
                            }
                        },
                            MenuOptionPriority.Medium )
                        );
                    }
                    Find.WindowStack.Add( new FloatMenu( languageOptions ) );
                }
            }
        }

        #endregion

    }

}
