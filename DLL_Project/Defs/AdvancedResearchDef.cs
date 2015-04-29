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

	public class ResearchCompletePair
	{
		// Research projects and last completion flag
		public ResearchProjectDef researchProject;
		public bool wasComplete;

		public ResearchCompletePair( ResearchProjectDef r )
		{
			researchProject = r;
			wasComplete = false;
		}

	}

	public class BuildingDesignationPair
	{
		// building designation pair for research unlock
		public ThingDef building;
		public string designationCategory;

		public BuildingDesignationPair( ThingDef b, string d )
		{
			building = b;
			designationCategory = d;
		}

	}

	public class AdvancedResearchDef : Def
	{
		// These are used in the xml def
		public List< RecipeDef > recipeDefs;
		public List< ResearchProjectDef >	researchDefs;
		public List< ThingDef > buildingDefs;

		// This is used by the DLL for optimization
		public bool isEnabled = false;

		// This is to allow buildings to be unlocked by multiple research
		public List< BuildingDesignationPair >	originalBuildingDesignations = null;
	}
}
