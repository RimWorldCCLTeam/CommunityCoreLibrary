using System.Collections.Generic;
using Verse;
using RimWorld;

namespace CommunityCoreLibrary
{

	public static class ThingComp_Extensions
	{

        // TODO:  Is any of this really needed?  1000101

		#region Comp Properties

		public static CommunityCoreLibrary.CompProperties_ColoredLight CompProperties_ColoredLight( this ThingComp thingComp )
		{
			return thingComp.props as CommunityCoreLibrary.CompProperties_ColoredLight;
		}

		public static CommunityCoreLibrary.CompProperties_LowIdleDraw CompProperties_LowIdleDraw( this ThingComp thingComp )
		{
			return thingComp.props as CommunityCoreLibrary.CompProperties_LowIdleDraw;
		}

		public static CompProperties_Rottable CompProperties_Rottable( this ThingComp thingComp )
		{
			return thingComp.props as CompProperties_Rottable;
		}

		public static CommunityCoreLibrary.CompProperties_RestrictedPlacement RestrictedPlacement_Properties( this ThingComp thingComp )
		{
			return thingComp.props as CommunityCoreLibrary.CompProperties_RestrictedPlacement;
		}

		#endregion

	}

}
