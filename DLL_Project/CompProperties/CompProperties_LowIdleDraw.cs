using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
	public enum LowIdleDrawMode
	{
		InUse = 0,
		WhenNear,
		GroupUser
	}
	public class CompProperties_LowIdleDraw : CompProperties
	{
		public float					idlePowerFactor = 0.0f;
		public LowIdleDrawMode			operationalMode = LowIdleDrawMode.InUse;

		public CompProperties_LowIdleDraw ()
		{
			this.compClass = typeof( CompProperties_LowIdleDraw );
		}
	}
}

