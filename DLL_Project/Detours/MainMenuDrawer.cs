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
        internal static Vector2             _optionsScroll = new Vector2();


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
        
        [DetourMember( typeof( MainMenuDrawer ), InjectionSequence.DLLLoad )]
        internal static void                _MainMenuOnGUI()
        {
            VersionControl.DrawInfoInCorner();

            #region Compute Base Title Vector
            var titleVec = MainMenuDrawerExt.TitleSize;
            if (titleVec.x > (float) Verse.UI.screenWidth )
            {
                titleVec *= (float) Verse.UI.screenWidth / titleVec.x;
            }
            titleVec *= 0.7f;
            #endregion

            #region Compute Main Buttons, Links and Language Rects
            var menuButtonHeight = MainMenuDrawerExt.OptionButtonSpacingFor(
                 MainMenuDrawerExt.CurrentMainMenuDefs( MainMenuDrawerExt.AnyMapFiles ).Count );

            var paneWidth = MainMenuDrawerExt.GameRectWidth * 2 + MainMenuDrawerExt.OptionListSpacing * 3;

            var minPaneHeight = (
                MainMenuDrawerExt.LinkOptionsHeight +
                MainMenuDrawerExt.LanguageOptionSpacing +
                MainMenuDrawerExt.LanguageOptionHeight
            );
            var maxPaneHeight = (
                Verse.UI.screenHeight -
                titleVec.y -
                MainMenuDrawerExt.TitlePaneSpacing -
                MainMenuDrawerExt.CreditHeight -
                MainMenuDrawerExt.CreditTitleSpacing -
                MainMenuDrawerExt.LudeonEdgeSpacing -
                MainMenuDrawerExt.LudeonLogoSize.y
            );

            var paneHeight = Mathf.Max(
                Mathf.Min( menuButtonHeight, maxPaneHeight ),
                minPaneHeight
            ) + MainMenuDrawerExt.OptionListSpacing * 2;

            MainMenuDrawerExt.PaneSize = new Vector2( paneWidth, paneHeight );

            var menuOptionsRect = new Rect(
                ( (float) Verse.UI.screenWidth -  MainMenuDrawerExt.PaneSize.x ) / 2f + MainMenuDrawerExt.TitleShift,
                ( (float) Verse.UI.screenHeight - MainMenuDrawerExt.PaneSize.y ) / 2f,
                MainMenuDrawerExt.PaneSize.x,
                MainMenuDrawerExt.PaneSize.y
            );
            
            menuOptionsRect.x = ( Verse.UI.screenWidth - menuOptionsRect.width - MainMenuDrawerExt.OptionsEdgeSpacing );
            #endregion

            #region Compute and Draw RimWorld Title
            var titleRect = new Rect(
                Verse.UI.screenWidth - titleVec.x - MainMenuDrawerExt.TitleShift,
                menuOptionsRect.y -    titleVec.y - MainMenuDrawerExt.TitlePaneSpacing,
                titleVec.x,
                titleVec.y );
            GUI.DrawTexture(
                titleRect,
                MainMenuDrawerExt.TexTitle,
                ScaleMode.StretchToFill,
                true );
            #endregion

            #region Compute and Draw Credit to Tynan

            Rect mainCreditRect = new Rect(
                0f,
                menuOptionsRect.y - MainMenuDrawerExt.CreditHeight,
                (float)Verse.UI.screenWidth - 85f,
                MainMenuDrawerExt.CreditHeight
            );

            var mainCreditText = "MainPageCredit".Translate();
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperRight;

            if( Verse.UI.screenWidth < 990 )
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
                    (float) Verse.UI.screenWidth - MainMenuDrawerExt.LudeonLogoSize.x - MainMenuDrawerExt.LudeonEdgeSpacing,
                    MainMenuDrawerExt.LudeonEdgeSpacing,
                    MainMenuDrawerExt.LudeonLogoSize.x,
                    MainMenuDrawerExt.LudeonLogoSize.y ),
                (Texture) MainMenuDrawerExt.TexLudeonLogo,
                ScaleMode.StretchToFill,
                true );
            GUI.color = Color.white;
            #endregion

            #region Draw Main Buttons, Links and Language Option
            menuOptionsRect.yMin += MainMenuDrawerExt.OptionListSpacing;
            GUI.BeginGroup(menuOptionsRect);

            MainMenuDrawer.DoMainMenuControls(
                menuOptionsRect,
                MainMenuDrawerExt.AnyMapFiles
            );

            GUI.EndGroup();
            #endregion

        }

        [DetourMember( typeof( MainMenuDrawer ), InjectionSequence.DLLLoad ) ]
        internal static void                _DoMainMenuControls( Rect rect, bool anyMapFiles )
        {
            #region Set Single Column Rect

            var optionColumnRect = new Rect( 0.0f, 0.0f, MainMenuDrawerExt.GameRectWidth, rect.height );
            Text.Font = GameFont.Small;

            #endregion

            #region Main Buttons

            #region Get Defs and Make Buttons

            var mainOptions = (
                from def in MainMenuDrawerExt.CurrentMainMenuDefs( anyMapFiles )
                select new ListableOption_MainMenu( def ) as ListableOption
            ).ToList();

            var buttonHeight = MainMenuDrawerExt.OptionButtonSpacingFor( mainOptions.Count );

            #endregion

            #region Handle Scroll Region Prefix

            Rect mainOptionsViewRect;
            if( buttonHeight > rect.y )
            {
                // More buttons than the area allows, begin a scroll area
                var scrollRect = new Rect(
                    0f,
                    MainMenuDrawerExt.OptionListSpacing,
                    MainMenuDrawerExt.GameRectWidth,
                    optionColumnRect.height - MainMenuDrawerExt.OptionListSpacing );
                optionColumnRect.width -= MainMenuDrawerExt.OptionListSpacing;
                optionColumnRect.height = buttonHeight;

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

            if( buttonHeight > rect.y )
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
