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

	public class AdvancedResearchDef : Def
	{
		// These are defined in xml
		public List< RecipeDef > recipeDefs;
		public List< ResearchProjectDef >	researchDefs;
		public List< ThingDef > buildingDefs;


        // These are used internally by the library, do not set them in xml!
		public bool isEnabled = false;

	}
}
