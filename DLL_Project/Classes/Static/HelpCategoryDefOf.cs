using Verse;

namespace CommunityCoreLibrary
{

    public static class HelpCategoryDefOf
    {
        public static readonly string           HelpPostFix          = "_HelpCategoryDef",

                                                // items
                                                ApparelHelp          = "Apparel" + HelpPostFix,
                                                BodyPartHelp         = "BodyPart" + HelpPostFix,
                                                DrugHelp             = "Drug" + HelpPostFix,
                                                MealHelp             = "Meal" + HelpPostFix,
                                                WeaponHelp           = "Weapon" + HelpPostFix,
                                                
                                                // flora and fauna
                                                TerrainHelp          = "Terrain" + HelpPostFix,
                                                Plants               = "Plants" + HelpPostFix,
                                                Animals              = "Animals" + HelpPostFix,
                                                Humanoids            = "Humanoids" + HelpPostFix,
                                                Mechanoids           = "Mechanoids" + HelpPostFix,
                                                Biomes               = "Biomes" + HelpPostFix,

                                                // recipes and research
                                                RecipeHelp           = "Recipe" + HelpPostFix,
                                                ResearchHelp         = "Research" + HelpPostFix,
                                                AdvancedResearchHelp = "AdvancedResearch" + HelpPostFix;
        
        public static readonly HelpCategoryDef  GizmoHelp = DefDatabase< HelpCategoryDef >.GetNamed( "GizmoHelp", true );
    }

}
