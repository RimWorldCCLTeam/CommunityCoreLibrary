using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

    public static class Icon
    {
        public static readonly Texture2D        GrowZone = ContentFinder<Texture2D>.Get( "UI/Designators/ZoneCreate_Growing", true);
        public static readonly Texture2D        NextButton = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/NextButton", true);
        public static readonly Texture2D        RoomButton = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/RoomButton", true);
        public static readonly Texture2D        LinkedButton = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/LinkedButton", true);
        public static readonly Texture2D        GroupButton = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/GroupButton", true);

        public static readonly Texture2D        ArrowUp = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowDown", true );
        public static readonly Texture2D        ArrowDown = ContentFinder<Texture2D>.Get( "UI/HelpMenu/ArrowUp", true );

    }

}