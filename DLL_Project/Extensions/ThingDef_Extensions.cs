using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingDef_Extensions
    {

        #region Recipe Cache

        public static void                  RecacheRecipes( this ThingDef thingDef )
        {
            //Log.Message( building.LabelCap + " - Recaching" );
            typeof( ThingDef ).GetField( "allRecipesCached", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( thingDef, null );
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

        public static CommunityCoreLibrary.RestrictedPlacement_Properties RestrictedPlacement_Properties ( this ThingDef thingDef )
        {
            return thingDef.GetCompProperties( typeof( CommunityCoreLibrary.RestrictedPlacement_Comp ) ) as CommunityCoreLibrary.RestrictedPlacement_Properties;
        }

        #endregion

    }

}
