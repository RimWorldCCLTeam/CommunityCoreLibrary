using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    [StaticConstructorOnStartup]
    public static class Icon
    {

        public static readonly Texture2D    GrowZone;
        public static readonly Texture2D    ShareSowTag;
        public static readonly Texture2D    ShareLightColor;
        public static readonly Texture2D    SelectLightColor;

        // Help tab
        public static readonly Texture2D    HelpMenuArrowUp;
        public static readonly Texture2D    HelpMenuArrowDown;
        public static readonly Texture2D    HelpMenuArrowRight;

        static Icon()
        {
            GrowZone            = ContentFinder<Texture2D>.Get( "UI/Designators/ZoneCreate_Growing" );
            ShareSowTag         = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/ShareSowTag" );
            ShareLightColor     = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/ShareLightColor" );
            SelectLightColor    = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/SelectLightColor" );
            HelpMenuArrowUp     = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowUp" );
            HelpMenuArrowDown   = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowDown" );
            HelpMenuArrowRight  = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowRight" );
        }

    }

}
