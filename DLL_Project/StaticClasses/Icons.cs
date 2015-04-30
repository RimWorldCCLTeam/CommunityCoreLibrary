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
		public static readonly Texture2D		GrowZone = ContentFinder<Texture2D>.Get( "UI/Designators/ZoneCreate_Growing", true);
		public static readonly Texture2D		NextButton = ContentFinder<Texture2D>.Get( "Ui/Icons/Commands/NextButton", true);
	}

}