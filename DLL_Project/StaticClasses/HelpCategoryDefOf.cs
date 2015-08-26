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

        public static readonly HelpCategoryDef  ApparelHelp = DefDatabase< HelpCategoryDef >.GetNamed( "ApparelHelp", true );
        public static readonly HelpCategoryDef  BuildingHelp = DefDatabase< HelpCategoryDef >.GetNamed( "BuildingHelp", true );
        public static readonly HelpCategoryDef  WeaponHelp = DefDatabase< HelpCategoryDef >.GetNamed( "WeaponHelp", true );

        public static readonly HelpCategoryDef  RecipeHelp = DefDatabase< HelpCategoryDef >.GetNamed( "RecipeHelp", true );

        public static readonly HelpCategoryDef  ResearchHelp = DefDatabase< HelpCategoryDef >.GetNamed( "ResearchHelp", true );
        public static readonly HelpCategoryDef  AdvancedResearchHelp = DefDatabase< HelpCategoryDef >.GetNamed( "AdvancedResearchHelp", true );

        public static readonly HelpCategoryDef  GizmoHelp = DefDatabase< HelpCategoryDef >.GetNamed( "GizmoHelp", true );

    }

}
