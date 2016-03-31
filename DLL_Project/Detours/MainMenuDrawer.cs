using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _MainMenuDrawer
    {

        internal const float        GameRectWidth = 200f;
        internal const float        NewsRectWidth = 350f;
        internal const int          ButCount = 3;
        internal const float        TitleShift = 50f;
        internal const float        TitlePaneSpacing = 10f;
        internal const float        OptionsEdgeSpacing = 30f;
        internal const float        CreditHeight = 30f;
        internal const float        CreditTitleSpacing = 3f;
        internal const float        LudeonEdgeSpacing = 8f;
        internal const float        OptionListSpacing = 17f;
        internal const float        LinkOptionMinHeight = 24f;
        internal const float        LanguageOptionWidth = 64f;
        internal const float        LanguageOptionHeight = 32f;
        internal const float        LanguageOptionSpacing = 10f;

        internal const float        OptionSpacingDefault = 7f;
        internal const float        OptionButtonHeightDefault = 45f;


        private static MethodInfo   _CloseMainTab;

        private static FieldInfo    _anyWorldFiles;
        private static FieldInfo    _anyMapFiles;

        private static FieldInfo    _PaneSize;
        private static FieldInfo    _TitleSize;

        private static FieldInfo    _IconBlog;
        private static FieldInfo    _IconForums;
        private static FieldInfo    _IconTwitter;
        private static FieldInfo    _IconBook;

        private static FieldInfo    _TexTitle;

        private static FieldInfo    _TexLudeonLogo;
        private static FieldInfo    _LudeonLogoSize;

        private static Vector2      _optionsScroll = new Vector2();

        private static List<MainMenuDef>  _MainMenuDefs;

        private static List<ListableOption> _linkOptions;

        #region Constructor

        static _MainMenuDrawer()
        {
            _CloseMainTab   = typeof( MainMenuDrawer ).GetMethod( "CloseMainTab", BindingFlags.Static | BindingFlags.NonPublic );

            _anyWorldFiles  = typeof( MainMenuDrawer ).GetField( "anyWorldFiles", BindingFlags.Static | BindingFlags.NonPublic );
            _anyMapFiles    = typeof( MainMenuDrawer ).GetField( "anyMapFiles", BindingFlags.Static | BindingFlags.NonPublic );

            _PaneSize       = typeof( MainMenuDrawer ).GetField( "PaneSize", BindingFlags.Static | BindingFlags.NonPublic );
            _TitleSize      = typeof( MainMenuDrawer ).GetField( "TitleSize", BindingFlags.Static | BindingFlags.NonPublic );

            _IconBlog       = typeof( MainMenuDrawer ).GetField( "IconBlog", BindingFlags.Static | BindingFlags.NonPublic );
            _IconForums     = typeof( MainMenuDrawer ).GetField( "IconForums", BindingFlags.Static | BindingFlags.NonPublic );
            _IconTwitter    = typeof( MainMenuDrawer ).GetField( "IconTwitter", BindingFlags.Static | BindingFlags.NonPublic );
            _IconBook       = typeof( MainMenuDrawer ).GetField( "IconBook", BindingFlags.Static | BindingFlags.NonPublic );

            _TexTitle       = typeof( MainMenuDrawer ).GetField( "TexTitle", BindingFlags.Static | BindingFlags.NonPublic );

            _TexLudeonLogo  = typeof( MainMenuDrawer ).GetField( "TexLudeonLogo", BindingFlags.Static | BindingFlags.NonPublic );
            _LudeonLogoSize = typeof( MainMenuDrawer ).GetField( "LudeonLogoSize", BindingFlags.Static | BindingFlags.NonPublic );

        }

        #endregion

        #region Properties Reflecting Original Fields

        internal static Vector2 ScreenCentre
        {
            get
            {
                return new Vector2( (float) Screen.width / 2f, (float) Screen.height / 2f );
            }
        }

        internal static bool AnyWorldFiles
        {
            get
            {
                return (bool) _anyWorldFiles.GetValue( null );
            }
        }

        internal static bool AnyMapFiles
        {
            get
            {
                return (bool) _anyMapFiles.GetValue( null );
            }
        }

        internal static Vector2 PaneSize
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

        internal static Vector2 TitleSize
        {
            get
            {
                return (Vector2) _TitleSize.GetValue( null );
            }
        }

        internal static Vector2 LudeonLogoSize
        {
            get
            {
                return (Vector2) _LudeonLogoSize.GetValue( null );
            }
        }

        internal static Texture2D TexTitle
        {
            get
            {
                return (Texture2D) _TexTitle.GetValue( null );
            }
        }

        internal static Texture2D TexLudeonLogo
        {
            get
            {
                return (Texture2D) _TexLudeonLogo.GetValue( null );
            }
        }

        internal static Texture2D IconBlog
        {
            get
            {
                return (Texture2D) _IconBlog.GetValue( null );
            }
        }

        internal static Texture2D IconForums
        {
            get
            {
                return (Texture2D) _IconForums.GetValue( null );
            }
        }

        internal static Texture2D IconTwitter
        {
            get
            {
                return (Texture2D) _IconTwitter.GetValue( null );
            }
        }

        internal static Texture2D IconBook
        {
            get
            {
                return (Texture2D) _IconBook.GetValue( null );
            }
        }
                             
        #endregion

        #region Sorted, Translated Main Menu Defs

        internal static List<MainMenuDef> AllMainMenuDefs
        {
            get
            {
                if( _MainMenuDefs == null )
                {
                    // Get all defs which have a valid label and menu worker
                    _MainMenuDefs = DefDatabase< MainMenuDef >.AllDefsListForReading.Where( def => (
                        (
                            ( !string.IsNullOrEmpty( def.label ) )||
                            (
                                ( !string.IsNullOrEmpty( def.labelKey ) )&&
                                ( def.labelKey.CanTranslate() )
                            )
                        )&&
                        ( def.menuWorker != null )
                    ) ).ToList();

                    // Sort defs by order
                    _MainMenuDefs.Sort( (x, y) => x.order > y.order ? -1 : 1 );

                    // Translate label keys
                    foreach( var menu in _MainMenuDefs )
                    {
                        if(
                            ( !string.IsNullOrEmpty( menu.labelKey ) )&&
                            ( menu.labelKey.CanTranslate() )
                        )
                        {
                            menu.label = menu.labelKey.Translate();
                        }
                    }
                }
                return _MainMenuDefs;
            }
        }

        internal static List<MainMenuDef> CurrentMainMenuDefs( bool anyWorldFiles, bool anyMapFiles )
        {
            return AllMainMenuDefs.Where( def => def.menuWorker.RenderNow( anyWorldFiles, anyMapFiles ) ).ToList();
        }

        internal static float CurrentMainMenuDefHeight( int count )
        {
            return 
                count * OptionButtonHeightDefault +
                ( count - 1 ) * OptionSpacingDefault;
        }

        #endregion

        #region Link Options

        internal static List<ListableOption> LinkOptions
        {
            get
            {
                if( _linkOptions == null )
                {
                    _linkOptions = new List<ListableOption>()
                    {
                        _FictionPrimerOption(),
                        _BlogOption(),
                        _ForumsOption(),
                        _WikiOption(),
                        _TwitterOption(),
                        _DesignBookOption(),
                        _TranslateOption()
                    };
                }
                return _linkOptions;
            }
        }

        internal static float LinkOptionsHeight
        {
            get
            {
                float y = 0f;
                foreach( var option in LinkOptions )
                {
                    var link = option as ListableOption_WebLink;
                    float width1 = (float) ( GameRectWidth - (float) link.image.width - 3.0f );
                    float num = Text.CalcHeight( link.label, width1 );
                    float height = Mathf.Max( LinkOptionMinHeight, num );
                    y += height;
                }
                return y;
            }
        }

        #endregion

        #region Detoured Methods

        /*
           0.0                            0.5                             1.0
        0.0 +--------------------------------------------------+-----------+
            |Version |                                         |  Ludeon   |
            +--+-----+-----------------------------------------+-----------+
            |  |                  RimWorld Title Tex                       |
            |  |                      Texture                              |
            |  +-------------------------+-------------------------------+-+
            |                            |        Credit to Tynan        | |
            |                          +-+--------------+----------------+ |
            |                          |                |                | |
            |                          |    Main        |    Web Links   | |
        0.5 |                          |    Option      |                | |
            |                          |    Buttons     |                | |
            |                          |                |                | |
            |                          |                |                | |
            |                          |                |                | |
            |                          |                |                | |
            |                          |                |                | |
            |                          |                |                | |
            |                          +----------------+----------------+ |
            |                                                              |
        1.0 +--------------------------------------------------------------+
        */

        internal static void _MainMenuOnGUI()
        {
            VersionControl.DrawInfoInCorner();

            var titleBaseVec = TitleSize;
            if( titleBaseVec.x > (float) Screen.width )
            {
                titleBaseVec *= (float) Screen.width / titleBaseVec.x;
            }
            var titleFinalVec = titleBaseVec * 0.7f;

            var currentMainMenuDefs = CurrentMainMenuDefs( AnyWorldFiles, AnyMapFiles );
            var currentMainMenuButtonCount = currentMainMenuDefs.Count;
            var currentMainMenuButtonHeight = CurrentMainMenuDefHeight( currentMainMenuButtonCount );

            var PaneWidth = GameRectWidth * 2 + OptionListSpacing * 3;

            var minPaneHeight = LinkOptionsHeight + LanguageOptionSpacing + LanguageOptionHeight;
            var maxPaneHeight = Screen.height - titleFinalVec.y - TitlePaneSpacing - CreditHeight - CreditTitleSpacing - LudeonEdgeSpacing - LudeonLogoSize.y;

            var PaneHeight = Mathf.Max( Mathf.Min( currentMainMenuButtonHeight, maxPaneHeight ), minPaneHeight ) + OptionListSpacing * 2;
            PaneSize = new Vector2( PaneWidth, PaneHeight );

            var menuOptionsRect = new Rect(
                ( (float) Screen.width  - PaneSize.x ) / 2f,
                ( (float) Screen.height - PaneSize.y ) / 2f,
                PaneSize.x,
                PaneSize.y );

            menuOptionsRect.y += TitleShift;

            menuOptionsRect.x = ( (float) Screen.width - menuOptionsRect.width - OptionsEdgeSpacing );

            var titleRect = new Rect(
                ( (float) Screen.width - titleFinalVec.x ) / 2f,
                ( menuOptionsRect.y - titleFinalVec.y - TitlePaneSpacing ),
                titleFinalVec.x,
                titleFinalVec.y );
            titleRect.x = ( (float) Screen.width - titleFinalVec.x - TitleShift );
            GUI.DrawTexture(
                titleRect,
                (Texture) TexTitle,
                ScaleMode.StretchToFill,
                true );

            var mainCreditRect = titleRect;
            mainCreditRect.y += titleRect.height;
            mainCreditRect.xMax -= 55f;
            mainCreditRect.height = CreditHeight;
            mainCreditRect.y += CreditTitleSpacing;
            var mainCreditText = "MainPageCredit".Translate();
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperRight;
            if( Screen.width < 990 )
            {
                var mainCreditBackRect = mainCreditRect;
                mainCreditBackRect.xMin = mainCreditBackRect.xMax - Text.CalcSize( mainCreditText ).x;
                mainCreditBackRect.xMin -= 4f;
                mainCreditBackRect.xMax += 4f;
                GUI.color = new Color( 0.2f, 0.2f, 0.2f, 0.5f );
                GUI.DrawTexture(
                    mainCreditBackRect,
                    (Texture) BaseContent.WhiteTex );
                GUI.color = Color.white;
            }
            Widgets.Label( mainCreditRect, mainCreditText );
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            GUI.color = new Color( 1f, 1f, 1f, 0.5f );
            GUI.DrawTexture(
                new Rect(
                    (float) Screen.width - LudeonLogoSize.x - LudeonEdgeSpacing,
                    LudeonEdgeSpacing,
                    LudeonLogoSize.x,
                    LudeonLogoSize.y ),
                (Texture) TexLudeonLogo,
                ScaleMode.StretchToFill,
                true );
            GUI.color = Color.white;

            menuOptionsRect.y += OptionListSpacing;
            GUI.BeginGroup( menuOptionsRect );

            MainMenuDrawer.DoMainMenuButtons(
                menuOptionsRect,
                AnyWorldFiles,
                AnyMapFiles );
            
            GUI.EndGroup();

        }

        internal static void _DoMainMenuButtons( Rect rect, bool anyWorldFiles, bool anyMapFiles, Action backToGameButtonAction = null )
        {
            var mainOptionRect = new Rect( 0.0f, 0.0f, GameRectWidth, rect.height );
            Text.Font = GameFont.Small;

            var mainOptions = new List<ListableOption>();
            var currentMainMenuDefs = CurrentMainMenuDefs( anyWorldFiles, anyMapFiles );

            foreach( var menu in currentMainMenuDefs )
            {
                mainOptions.Add( new ListableOption_MainMenu( menu ) );
            }

            var currentMainMenuButtonCount = currentMainMenuDefs.Count;
            var currentMainMenuButtonHeight = CurrentMainMenuDefHeight( currentMainMenuButtonCount );

            Rect mainOptionsViewRect;

            if( currentMainMenuButtonHeight > rect.y )
            {
                // More buttons than the area allows, begin a scroll area
                var scrollRect = new Rect(
                    0f,
                    OptionListSpacing,
                    GameRectWidth,
                    mainOptionRect.height - OptionListSpacing );
                mainOptionRect.width -= OptionListSpacing;
                mainOptionRect.height = currentMainMenuButtonHeight;
                _optionsScroll = GUI.BeginScrollView( scrollRect, _optionsScroll, mainOptionRect );
                mainOptionsViewRect = mainOptionRect;
            }
            else
            {
                mainOptionsViewRect = mainOptionRect.ContractedBy( OptionListSpacing );
            }

            var mainOptionsHeight = OptionListingUtility.DrawOptionListing( mainOptionsViewRect, mainOptions );

            if( currentMainMenuButtonHeight > rect.y )
            {
                // End the scroll area
                GUI.EndScrollView();
                mainOptionRect.xMax += OptionListSpacing;
            }

            var linkOptionAreaRect = new Rect( mainOptionRect.xMax, 0.0f, -1f, rect.height );
            linkOptionAreaRect.xMax = rect.width;

            Text.Font = GameFont.Small;

            var linkOptionRect = linkOptionAreaRect.ContractedBy( OptionListSpacing );
            var linkOptionHeight = OptionListingUtility.DrawOptionListing( linkOptionRect, LinkOptions );

            if( Game.Mode == GameMode.Entry )
            {
                GUI.BeginGroup( linkOptionRect );
                if(
                    Widgets.ImageButton(
                        new Rect(
                            0.0f,
                            linkOptionHeight + LanguageOptionSpacing,
                            LanguageOptionWidth,
                            LanguageOptionHeight ),
                        LanguageDatabase.activeLanguage.icon )
                )
                {
                    var languageOptions = new List<FloatMenuOption>();
                    foreach( LoadedLanguage loadedLanguage in LanguageDatabase.AllLoadedLanguages )
                    {
                        var switcher = new SwitchLang( loadedLanguage );
                        languageOptions.Add(
                            new FloatMenuOption(
                                switcher.localLang.FriendlyNameNative,
                                switcher.SwitchTo,
                                MenuOptionPriority.Medium )
                        );
                    }
                    Find.WindowStack.Add( (Window) new FloatMenu( languageOptions, false ) );
                }
                GUI.EndGroup();
            }
        }

        #endregion

        #region Language Picked Helper Class

        internal class SwitchLang
        {

            public LoadedLanguage localLang;

            public SwitchLang( LoadedLanguage lang )
            {
                localLang = lang;
            }

            public void SwitchTo()
            {
                LanguageDatabase.SelectLanguage( localLang );
                Prefs.Save();
            }

        }

        #endregion

        internal static void            CloseMainTab()
        {
            _CloseMainTab.Invoke( null, null );
        }

        #region Main Menu Listable Options

        internal class ListableOption_MainMenu : ListableOption
        {
            MainMenuDef     menuDef;

            public ListableOption_MainMenu( MainMenuDef def ) : base( def.label, def.menuWorker.ClickAction )
            {
                menuDef = def;
            }

            public override float DrawOption( Vector2 pos, float width )
            {
                float height = Mathf.Max( minHeight, Text.CalcHeight( label, width ) );
                if( Widgets.TextButton( new Rect( pos.x, pos.y, width, height ), label, true, true ) )
                {
                    if( menuDef.closeMainTab )
                    {
                        CloseMainTab();
                    }
                    this.action();
                }
                return height;
            }

        }

        #endregion

        #region Link Buttons

        internal static ListableOption  _FictionPrimerOption()
        {
            return new ListableOption_WebLink(
                "FictionPrimer".Translate(),
                "https://docs.google.com/document/d/1pIZyKif0bFbBWten4drrm7kfSSfvBoJPgG9-ywfN8j8/pub",
                IconBlog
            );
        }

        internal static ListableOption  _BlogOption()
        {
            return new ListableOption_WebLink(
                "LudeonBlog".Translate(),
                "http://ludeon.com/blog",
                IconBlog
            );
        }

        internal static ListableOption  _ForumsOption()
        {
            return new ListableOption_WebLink(
                "Forums".Translate(),
                "http://ludeon.com/forums",
                IconForums
            );
        }

        internal static ListableOption  _WikiOption()
        {
            return new ListableOption_WebLink(
                "OfficialWiki".Translate(),
                "http://rimworldwiki.com",
                IconBlog
            );
        }

        internal static ListableOption  _TwitterOption()
        {
            return new ListableOption_WebLink(
                "TynansTwitter".Translate(),
                "https://twitter.com/TynanSylvester",
                IconTwitter
            );
        }

        internal static ListableOption  _DesignBookOption()
        {
            return new ListableOption_WebLink(
                "TynansDesignBook".Translate(),
                "http://tynansylvester.com/book",
                IconBook
            );
        }

        internal static ListableOption  _TranslateOption()
        {
            return new ListableOption_WebLink(
                "HelpTranslate".Translate(),
                "http://ludeon.com/forums/index.php?topic=2933.0",
                IconForums
            );
        }

        /*
        internal static ListableOption  _Option()
        {
            return new ListableOption_WebLink(
                "".Translate(),
                "",
                Icon
            );
        }
        */

        #endregion

    }

}
