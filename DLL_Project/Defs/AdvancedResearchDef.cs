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

	public struct ResearchCompletePair
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

	public struct BuildingDesignationPair
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
		// These are defined in xml
		public List< RecipeDef > recipeDefs;
		public List< ResearchProjectDef >	researchDefs;
		public List< ThingDef > buildingDefs;


        // These are used internally by the library, do not set them in xml!
		public bool isEnabled = false;

		// This stores buildings designation categories for those that are unlocked by multiple trees
		public List< BuildingDesignationPair >	originalBuildingDesignations = null;
	}
}
