using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public interface IHopperUser
	{
		// This property tells is the list of things to program the hopper with
		ThingFilter						ResourceFilter
		{
			get;
		}

	}

}

