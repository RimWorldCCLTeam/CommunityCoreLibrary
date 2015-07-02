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
        // Processing priority so everything happens in the order it should
        // Lower value is higher priority
        public int                          Priority;

        // Research requirement
        public List< ResearchProjectDef >   researchDefs;

        // Flag this as true to remove recipes and hide buildings
        public bool                         HideDefs = false;

        // These are optionally defined in xml
        public List< RecipeDef >            recipeDefs;
        public List< ThingDef >             buildingDefs;
        public List< ResearchMod >          researchMods;

        // These are used internally by the library, do not set them in xml!
        public bool                         isEnabled = false;

        public bool                         toggleRecipes;
        public bool                         toggleBuildings;
        public bool                         doCallbacks;

    }
}
