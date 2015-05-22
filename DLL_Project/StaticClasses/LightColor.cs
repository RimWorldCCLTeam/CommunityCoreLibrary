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
	public static class Light
	{
        public static readonly List< colorName >    Color = new List< colorName >(){
            new colorName( "colorWhite".Translate() , new ColorInt( 217, 217, 217,   0 ) ),
            new colorName( "colorRed".Translate()   , new ColorInt( 217,   0,   0,   0 ) ),
            new colorName( "colorGreen".Translate() , new ColorInt(   0, 217,   0,   0 ) ),
            new colorName( "colorBlue".Translate()  , new ColorInt(   0,   0, 217,   0 ) ),
            new colorName( "colorYellow".Translate(), new ColorInt( 217, 217,  43,   0 ) ),
            new colorName( "colorOrange".Translate(), new ColorInt( 255, 132,   0,   0 ) ),
            new colorName( "colorPurple".Translate(), new ColorInt( 185,  61, 205,   0 ) ),
        };

	}
}
