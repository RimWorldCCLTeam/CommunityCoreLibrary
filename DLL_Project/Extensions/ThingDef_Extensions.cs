using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingDef_Extensions
    {

        #region Recipe Cache

        public static void                  RecacheRecipes( this ThingDef thingDef, bool validateBills )
        {
            //Log.Message( building.LabelCap + " - Recaching" );
            typeof( ThingDef ).GetField( "allRecipesCached", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( thingDef, null );

            if( !validateBills )
            {
                return;
            }

            // Get the recached recipes
            var recipes = thingDef.AllRecipes;

            // Remove bill on any table of this def using invalid recipes
            var buildings = Find.ListerBuildings.AllBuildingsColonistOfDef( thingDef );
            foreach( var building in buildings )
            {
                var BillGiver = building as IBillGiver;
                for( int i = 0; i < BillGiver.BillStack.Count; ++ i )
                {
                    var bill = BillGiver.BillStack[ i ];
                    if( !recipes.Exists( r => bill.recipe == r ) )
                    {
                        BillGiver.BillStack.Delete( bill );
                        continue;
                    }
                }
            }

        }

        #endregion

        #region Lists of affected data

        public static void                  GetRecipesByResearchUnlocked( this ThingDef thingDef, ref List< RecipeDef > recipeDefs, ref List< Def > researchDefs )
        {
            // Things it is unlocked on with research
            recipeDefs = new List<RecipeDef>();
            researchDefs = new List<Def>();

            // Look at recipes
            var recipes = DefDatabase< RecipeDef >.AllDefsListForReading.Where( r => (
                ( r.researchPrerequisite != null )&&
                ( r.recipeUsers != null )&&
                ( !r.IsLockedOut() )&&
                ( r.recipeUsers.Contains( thingDef ) )
            ) ).ToList();

            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.thingDefs.Contains( thingDef ) )
            ) ).ToList();

            // Aggregate advanced research
            foreach( var a in advancedResearch )
            {
                recipeDefs.AddRange( a.recipeDefs );
                if( a.researchDefs.Count == 1 )
                {
                    // If it's a single research project, add that
                    researchDefs.Add( a.researchDefs[ 0 ] );
                }
                else
                {
                    // Add the advanced project instead
                    researchDefs.Add( a );
                }
            }
        }

        public static void                  GetRecipesByResearchLocked( this ThingDef thingDef, ref List< RecipeDef > recipeDefs, ref List< Def > researchDefs )
        {
            // Things it is locked on with research
            recipeDefs = new List<RecipeDef>();
            researchDefs = new List<Def>();

            // Look in advanced research
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( a.HideDefs )&&
                ( a.thingDefs.Contains( thingDef ) )
            ) ).ToList();

            // Aggregate advanced research
            foreach( var a in advancedResearch )
            {
                recipeDefs.AddRange( a.recipeDefs );
                if( a.researchDefs.Count == 1 )
                {
                    // If it's a single research project, add that
                    researchDefs.Add( a.researchDefs[ 0 ] );
                }
                else if( a.HelpConsolidator != null )
                {
                    // Add the advanced project instead
                    researchDefs.Add( a.HelpConsolidator );
                }
            }
        }

        #endregion

        #region Comp Properties

        public static CommunityCoreLibrary.CompProperties_ColoredLight CompProperties_ColoredLight ( this ThingDef thingDef )
        {
            return thingDef.GetCompProperties( typeof( CommunityCoreLibrary.CompColoredLight ) ) as CommunityCoreLibrary.CompProperties_ColoredLight;
        }

        public static CommunityCoreLibrary.CompProperties_LowIdleDraw CompProperties_LowIdleDraw ( this ThingDef thingDef )
        {
            return thingDef.GetCompProperties( typeof( CommunityCoreLibrary.CompPowerLowIdleDraw ) ) as CommunityCoreLibrary.CompProperties_LowIdleDraw;
        }

        public static Verse.CompProperties_Rottable CompProperties_Rottable ( this ThingDef thingDef )
        {
            return thingDef.GetCompProperties( typeof( RimWorld.CompRottable ) ) as Verse.CompProperties_Rottable;
        }

        #endregion

    }

}
