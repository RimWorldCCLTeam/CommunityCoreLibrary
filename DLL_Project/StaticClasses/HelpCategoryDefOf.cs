using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class HelpCategoryDefOf
    {

        public static readonly string           ApparelHelp = "Apparel_HelpCategoryDef";
        public static readonly string           WeaponHelp = "Weapon_HelpCategoryDef";

        public static readonly string           RecipeHelp = "Recipe_HelpCategoryDef";

        public static readonly string           ResearchHelp = "Research_HelpCategoryDef";
        public static readonly string           AdvancedResearchHelp = "AdvancedResearch_HelpCategoryDef";

        public static readonly HelpCategoryDef  GizmoHelp = DefDatabase< HelpCategoryDef >.GetNamed( "GizmoHelp", true );

    }

}
