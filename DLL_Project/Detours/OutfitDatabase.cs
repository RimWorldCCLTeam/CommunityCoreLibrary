using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    internal static class _OutfitDatabase
    {

        private const string            OutfitLabelAnything = "Anything";
        private const string            OutfitLabelNothing = "Nothing";
        private const string            OutfitLabelWorker = "Worker";
        private const string            OutfitLabelSoldier = "Soldier";
        private const string            OutfitLabelNaked = "Nudist";

        internal static List<OutfitDef> OutfitDefs = new List<OutfitDef>();

        [DetourClassMethod( typeof( OutfitDatabase ), "GenerateStartingOutfits" )]
        internal static void _GenerateStartingOutfits( this OutfitDatabase outfitDatabase )
        {
            outfitDatabase.MakeNewOutfit().label = OutfitLabelAnything;

            var outfitNothing = outfitDatabase.MakeNewOutfit();
            outfitNothing.label = OutfitLabelNothing;
            outfitNothing.filter.SetDisallowAll();

            var outfitWorker = outfitDatabase.MakeNewOutfit();
            outfitWorker.label = OutfitLabelWorker;
            outfitWorker.filter.SetDisallowAll();
            var workerApparel = DefDatabase<ThingDef>
                .AllDefs
                .Where( thingDef => (
                    ( thingDef.apparel != null )&&
                    ( !thingDef.apparel.defaultOutfitTags.NullOrEmpty() )&&
                    ( thingDef.apparel.defaultOutfitTags.Contains( OutfitLabelWorker ) )
                ) ).ToList();
            foreach( var apparelDef in workerApparel )
            {
                outfitWorker.filter.SetAllow( apparelDef, true );
            }

            var outfitSoldier = outfitDatabase.MakeNewOutfit();
            outfitSoldier.label = OutfitLabelSoldier;
            outfitSoldier.filter.SetDisallowAll();
            var soldierApparel = DefDatabase<ThingDef>
                .AllDefs
                .Where( thingDef => (
                    ( thingDef.apparel != null )&&
                    ( !thingDef.apparel.defaultOutfitTags.NullOrEmpty() )&&
                    ( thingDef.apparel.defaultOutfitTags.Contains( OutfitLabelSoldier ) )
                ) ).ToList();
            foreach( var apparelDef in soldierApparel )
            {
                outfitSoldier.filter.SetAllow( apparelDef, true );
            }

            var outfitNaked = outfitDatabase.MakeNewOutfit();
            outfitNaked.label = OutfitLabelNaked;
            outfitNaked.filter.SetDisallowAll();
            var nakedApparel = DefDatabase<ThingDef>
                .AllDefs
                .Where( thingDef => (
                    ( thingDef.apparel != null )&&
                    ( thingDef.apparel.bodyPartGroups.NullOrEmpty() )&&
                    ( !thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) )&&
                    ( !thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) )
                ) ).ToList();
            foreach( var apparelDef in nakedApparel )
            {
                outfitNaked.filter.SetAllow(apparelDef, true);
            }

            // Add outfits to database
            foreach( OutfitDef outfitDef in OutfitDefs )
            {
                var newOutfit = outfitDatabase.MakeNewOutfit();
                newOutfit.label = outfitDef.label;
                newOutfit.filter = outfitDef.filter;
            }
        }

    }

}
