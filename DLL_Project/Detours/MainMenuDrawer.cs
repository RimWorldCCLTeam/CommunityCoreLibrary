using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;
using MainMenuDrawerExt = CommunityCoreLibrary.MainMenuDrawer_Extensions;

namespace CommunityCoreLibrary.Detour
{

    internal static class _MainMenuDrawer
    {

        // Don't bother reflecting this, just create your own, it's only used in one place
        internal static Vector2     _optionsScroll = new Vector2();


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
            |                          |                +----------+-----+ |
            |                          |                | Language |       |
            |                          |                | Flag     |       |
            |                          +----------------+----------+       |
            |                                                              |
        1.0 +--------------------------------------------------------------+
        */

        [DetourClassMethod( typeof( MainMenuDrawer ), "MainMenuOnGUI", InjectionSequence.DLLLoad )]
        internal static void _MainMenuOnGUI()
        {
            #region Version
            VersionControl.DrawInfoInCorner();
            #endregion

            #region Compute Base Title Vector
            var titleBaseVec = MainMenuDrawerExt.TitleSize;
            if( titleBaseVec.x > (float) Screen.width )
            {
                titleBaseVec *= (float) Screen.width / titleBaseVec.x;
            }
            var titleFinalVec = titleBaseVec * 0.7f;
            #endregion

            #region Compute Main Buttons, Links and Language Rects
            var currentMainMenuDefs = MainMenuDrawerExt.CurrentMainMenuDefs( MainMenuDrawerExt.AnyMapFiles );
            var currentMainMenuButtonCount = currentMainMenuDefs.Count;
            var currentMainMenuButtonHeight = MainMenuDrawerExt.OptionButtonSpacingFor( currentMainMenuButtonCount );

            var PaneWidth = MainMenuDrawerExt.GameRectWidth * 2 + MainMenuDrawerExt.OptionListSpacing * 3;

            var minPaneHeight = MainMenuDrawerExt.LinkOptionsHeight + MainMenuDrawerExt.LanguageOptionSpacing + MainMenuDrawerExt.LanguageOptionHeight;
            var maxPaneHeight = Screen.height - titleFinalVec.y - MainMenuDrawerExt.TitlePaneSpacing - MainMenuDrawerExt.CreditHeight - MainMenuDrawerExt.CreditTitleSpacing - MainMenuDrawerExt.LudeonEdgeSpacing - MainMenuDrawerExt.LudeonLogoSize.y;

            var PaneHeight = Mathf.Max( Mathf.Min( currentMainMenuButtonHeight, maxPaneHeight ), minPaneHeight ) + MainMenuDrawerExt.OptionListSpacing * 2;
            MainMenuDrawerExt.PaneSize = new Vector2( PaneWidth, PaneHeight );

            var menuOptionsRect = new Rect(
                ( (float) Screen.width  - MainMenuDrawerExt.PaneSize.x ) / 2f,
                ( (float) Screen.height - MainMenuDrawerExt.PaneSize.y ) / 2f,
                MainMenuDrawerExt.PaneSize.x,
                MainMenuDrawerExt.PaneSize.y );

            menuOptionsRect.y += MainMenuDrawerExt.TitleShift;

            menuOptionsRect.x = ( (float) Screen.width - menuOptionsRect.width - MainMenuDrawerExt.OptionsEdgeSpacing );
            #endregion

            #region Compute and Draw RimWorld Title
            var titleRect = new Rect(
                ( (float) Screen.width - titleFinalVec.x ) / 2f,
                ( menuOptionsRect.y - titleFinalVec.y - MainMenuDrawerExt.TitlePaneSpacing ),
                titleFinalVec.x,
                titleFinalVec.y );
            titleRect.x = ( (float) Screen.width - titleFinalVec.x - MainMenuDrawerExt.TitleShift );
            GUI.DrawTexture(
                titleRect,
                (Texture) MainMenuDrawerExt.TexTitle,
                ScaleMode.StretchToFill,
                true );
            #endregion

            #region Compute and Draw Credit to Tynan
            var mainCreditRect = titleRect;
            mainCreditRect.y += titleRect.height;
            mainCreditRect.xMax -= 55f;
            mainCreditRect.height = MainMenuDrawerExt.CreditHeight;
            mainCreditRect.y += MainMenuDrawerExt.CreditTitleSpacing;
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
            #endregion

            #region Compute and Draw Ludeon Logo
            GUI.color = new Color( 1f, 1f, 1f, 0.5f );
            GUI.DrawTexture(
                new Rect(
                    (float) Screen.width - MainMenuDrawerExt.LudeonLogoSize.x - MainMenuDrawerExt.LudeonEdgeSpacing,
                    MainMenuDrawerExt.LudeonEdgeSpacing,
                    MainMenuDrawerExt.LudeonLogoSize.x,
                    MainMenuDrawerExt.LudeonLogoSize.y ),
                (Texture) MainMenuDrawerExt.TexLudeonLogo,
                ScaleMode.StretchToFill,
                true );
            GUI.color = Color.white;
            #endregion

            #region Draw Main Buttons, Links and Language Option
            menuOptionsRect.y += MainMenuDrawerExt.OptionListSpacing;
            GUI.BeginGroup( menuOptionsRect );
            
            MainMenuDrawer.DoMainMenuControls(
                menuOptionsRect,
                MainMenuDrawerExt.AnyMapFiles );
            
            GUI.EndGroup();
            #endregion
        }

        [DetourClassMethod( typeof( MainMenuDrawer ), "DoMainMenuControls", InjectionSequence.DLLLoad ) ]
        internal static void _DoMainMenuControls( Rect rect, bool anyMapFiles )
        {
            #region Set Single Column Rect
            var optionColumnRect = new Rect( 0.0f, 0.0f, MainMenuDrawerExt.GameRectWidth, rect.height );
            Text.Font = GameFont.Small;
            #endregion

            #region Main Buttons

            #region Get Defs and Make Buttons

            var mainOptions = new List<ListableOption>();
            var currentMainMenuDefs = MainMenuDrawerExt.CurrentMainMenuDefs( anyMapFiles );

            foreach( var menu in currentMainMenuDefs )
            {
                mainOptions.Add( new ListableOption_MainMenu( menu ) );
            }

            #endregion

            #region Calculate Height for Buttons
            var currentMainMenuButtonCount = currentMainMenuDefs.Count;
            var currentMainMenuButtonHeight = MainMenuDrawerExt.OptionButtonSpacingFor( currentMainMenuButtonCount );
            #endregion

            #region Handle Scroll Region Prefix
            Rect mainOptionsViewRect;
            if( currentMainMenuButtonHeight > rect.y )
            {
                // More buttons than the area allows, begin a scroll area
                var scrollRect = new Rect(
                    0f,
                    MainMenuDrawerExt.OptionListSpacing,
                    MainMenuDrawerExt.GameRectWidth,
                    optionColumnRect.height - MainMenuDrawerExt.OptionListSpacing );
                optionColumnRect.width -= MainMenuDrawerExt.OptionListSpacing;
                optionColumnRect.height = currentMainMenuButtonHeight;
                _optionsScroll = GUI.BeginScrollView( scrollRect, _optionsScroll, optionColumnRect );
                mainOptionsViewRect = optionColumnRect;
            }
            else
            {
                mainOptionsViewRect = optionColumnRect.ContractedBy( MainMenuDrawerExt.OptionListSpacing );
            }
            #endregion

            #region Draw Buttons
            var mainOptionsHeight = OptionListingUtility.DrawOptionListing( mainOptionsViewRect, mainOptions );
            #endregion

            #region Handle Scroll Region Suffix
            if( currentMainMenuButtonHeight > rect.y )
            {
                // End the scroll area
                GUI.EndScrollView();
                optionColumnRect.xMax += MainMenuDrawerExt.OptionListSpacing;
            }
            #endregion

            #endregion

            #region Links and Language

            var linkOptionAreaRect = new Rect( optionColumnRect.xMax, 0.0f, -1f, rect.height );
            linkOptionAreaRect.xMax = rect.width;

            Text.Font = GameFont.Small;

            #region Draw Links
            var linkOptionRect = linkOptionAreaRect.ContractedBy( MainMenuDrawerExt.OptionListSpacing );
            var linkOptionHeight = OptionListingUtility.DrawOptionListing( linkOptionRect, MainMenuDrawerExt.LinkOptions );
            #endregion

            #region Draw Language Selection
            var languageRect = new Rect(
                linkOptionRect.x,
                linkOptionHeight + MainMenuDrawerExt.OptionSpacingDefault + MainMenuDrawerExt.LanguageOptionSpacing,
                MainMenuDrawerExt.LanguageOptionWidth,
                MainMenuDrawerExt.LanguageOptionHeight
            );

            MainMenuDrawerExt.DrawLanguageOption( languageRect );
            #endregion

            #endregion

        }

        #endregion

    }

}
