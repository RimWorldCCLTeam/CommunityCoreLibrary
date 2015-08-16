using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Icon
    {
        
        public static readonly Texture2D    GrowZone            = ContentFinder<Texture2D>.Get( "UI/Designators/ZoneCreate_Growing" );
        public static readonly Texture2D    NextButton          = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/NextButton" );
        public static readonly Texture2D    RoomButton          = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/RoomButton" );
        public static readonly Texture2D    LinkedButton        = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/LinkedButton" );
        public static readonly Texture2D    GroupButton         = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/GroupButton" );

        public static readonly Texture2D    HelpMenuArrowUp     = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowDown" );
        public static readonly Texture2D    HelpMenuArrowDown   = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowUp" );

    }

}
