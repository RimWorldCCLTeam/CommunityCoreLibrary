using System;
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
	public static class LightColour
	{
		public static readonly ColorInt[]		value = {
			new ColorInt( 217, 217, 217,   0 ),
			new ColorInt( 217,   0,   0,   0 ),
			new ColorInt(   0, 217,   0,   0 ),
			new ColorInt(   0,   0, 217,   0 ),
			new ColorInt( 217, 217,  43,   0 ),
			new ColorInt( 255, 132,   0,   0 ),
			new ColorInt( 185,  61, 205,   0 )
		};

		public static readonly string[]			name = {
			"colourWhite".Translate(),
			"colourRed".Translate(),
			"colourGreen".Translate(),
			"colourBlue".Translate(),
			"colourYellow".Translate(),
			"colourOrange".Translate(),
			"colourPurple".Translate()
		};

		public static readonly int				count = value.Count();

		public static string					NextColourName( int index )
		{
			int nextIndex = index + 1;
			if( nextIndex >= count )
			{
				nextIndex = 0;
			}
			return name[ nextIndex ];
		}

	}

	public static class Icon
	{
		public static readonly Texture2D		GrowZone = ContentFinder<Texture2D>.Get( "UI/Designators/ZoneCreate_Growing", true);
		public static readonly Texture2D		NextButton = ContentFinder<Texture2D>.Get( "Ui/Icons/Commands/NextButton", true);
	}
}