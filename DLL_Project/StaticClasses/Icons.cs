using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Icon
    {
        
        public static readonly Texture2D    GrowZone            = ContentFinder<Texture2D>.Get( "UI/Designators/ZoneCreate_Growing" );
        public static readonly Texture2D    ShareSowTag         = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/ShareSowTag" );
        public static readonly Texture2D    ShareLightColor     = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/ShareLightColor" );
        public static readonly Texture2D    SelectLightColor    = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/SelectLightColor" );

        public static readonly Texture2D    HelpMenuArrowUp     = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowDown" );
        public static readonly Texture2D    HelpMenuArrowDown   = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowUp" );

    }

}
