using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    internal static class _OutfitDatabase
    {
        public static List<OutfitDef> OutfitDefs = new List<OutfitDef>();

        internal static void _GenerateStartingOutfits(this OutfitDatabase outfitDatabase)
        {
            outfitDatabase.MakeNewOutfit().label = "Anything";

            Outfit outfit = outfitDatabase.MakeNewOutfit();
            outfit.label = "Nothing";
            outfit.filter.SetDisallowAll();

            Outfit outfit1 = outfitDatabase.MakeNewOutfit();
            outfit1.label = "Worker";
            outfit1.filter.SetDisallowAll();
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.apparel != null && allDef.apparel.defaultOutfitTags != null && allDef.apparel.defaultOutfitTags.Contains("Worker"))
                    outfit1.filter.SetAllow(allDef, true);
            }

            Outfit outfit2 = outfitDatabase.MakeNewOutfit();
            outfit2.label = "Soldier";
            outfit2.filter.SetDisallowAll();
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.apparel != null && allDef.apparel.defaultOutfitTags != null && allDef.apparel.defaultOutfitTags.Contains("Soldier"))
                    outfit2.filter.SetAllow(allDef, true);
            }

            Outfit outfit3 = outfitDatabase.MakeNewOutfit();
            outfit3.label = "Nudist";
            outfit3.filter.SetDisallowAll();
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.apparel != null && !allDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !allDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                    outfit3.filter.SetAllow(allDef, true);
            }

            // do my stuff here
            foreach (OutfitDef outfitDef in OutfitDefs)
            {
                Outfit newOutfit = outfitDatabase.MakeNewOutfit();
                newOutfit.label = outfitDef.label;
                newOutfit.filter = outfitDef.filter;
            }
        }
    }
}
