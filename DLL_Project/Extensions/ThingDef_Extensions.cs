using System.Reflection;
using System.Collections.Generic;

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
