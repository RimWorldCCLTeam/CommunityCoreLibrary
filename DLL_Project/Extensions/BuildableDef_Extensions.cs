using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    [StaticConstructorOnStartup]
	public static class BuildableDef_Extensions
	{

		#region Static Data

		static Dictionary<ushort, bool> isLockedOut;

        static BuildableDef_Extensions()
        {
            isLockedOut = new Dictionary<ushort, bool>();
        }

		#endregion

		#region Availability

		public static bool                  IsLockedOut( this BuildableDef buildableDef )
        {
            bool rVal;
            var foo = buildableDef.GetHashCode();
            if( !isLockedOut.TryGetValue( buildableDef.shortHash, out rVal ) )
            {
#if DEBUG
                CCL_Log.TraceMod(
                    buildableDef,
                    Verbosity.Stack,
                    "IsLockedOut()"
                );
#endif

                // Is it a frame or blueprint?
                if(
                    ( buildableDef.defName.EndsWith( "_Frame" ) )||
                    ( buildableDef.defName.EndsWith( "_Blueprint" ) )||
                    ( buildableDef.defName.EndsWith( "_Blueprint_Install" ) )
                )
                {
                    isLockedOut.Add( buildableDef.shortHash, true );
                    return true;
                }
                
                //  Logic is faulty ( Thingdefs inherit from buildable defs, checking for existence of blueprint eliminates all non-buildings )
                //  After correcting logic (Valid: blueprint == null [items], or blueprint != null and designation != null/None [buildings]),
                //  the check no longer makes sense, since only defs with a designation category ever get a blueprint assigned. -- Fluffy.
                //
                //// Is the designationCategory locked out?
                //// only applies if it is buildable (has a blueprint def), but no category.
                //if(
                //    buildableDef.blueprintDef != null &&
                //    ( buildableDef.designationCategory.NullOrEmpty()||
                //      buildableDef.designationCategory == "None" )
                //)
                //{
                //    isLockedOut.Add( buildableDef, true );
                //    return true;
                //}

                // If the research locks it's out, check for an ARDef unlock
                if(
                    ( buildableDef.researchPrerequisites != null )&&
                    ( buildableDef.researchPrerequisites.Any( def => def.IsLockedOut() ) )&&
                    ( !Controller.Data.AdvancedResearchDefs.Any( a => (
                        ( a.IsBuildingToggle )&&
                        ( !a.HideDefs )&&
                        ( a.thingDefs.Contains( buildableDef as ThingDef ) )
                ) ) ) )
                {
                    rVal = true;
                }

                // Cache the result
                isLockedOut.Add( buildableDef.shortHash, rVal );
            }
            return rVal;
        }

		public static bool HasResearchRequirement( this BuildableDef buildableDef )
		{
#if DEBUG
            CCL_Log.TraceMod(
                buildableDef,
                Verbosity.Stack,
                "HasResearchRequirement()"
            );
#endif
			// Can't entirely rely on this one check as it's state may change mid-game
			if(
                ( buildableDef.researchPrerequisites != null )&&
                ( buildableDef.researchPrerequisites.Any( def => def != null ) )
            )
            {
                // Easiest check, do it first
                return true;
            }

			// Check for an advanced research unlock
			return
				Controller.Data.AdvancedResearchDefs.Any( a => (
					( a.IsBuildingToggle ) &&
					( !a.HideDefs ) &&
					( a.thingDefs.Contains( buildableDef as ThingDef ) )
				) );
		}

		#endregion

		#region Lists of affected data

		public static List<Def> GetResearchRequirements( this BuildableDef buildableDef )
		{
#if DEBUG
            CCL_Log.TraceMod(
                buildableDef,
                Verbosity.Stack,
                "GetResearchRequirements()"
            );
#endif
			var researchDefs = new List<Def>();

			if( buildableDef.researchPrerequisites != null )
            {
                if( !buildableDef.researchPrerequisites.Contains( Research.Locker ) )
                {
                    researchDefs.AddRangeUnique( buildableDef.researchPrerequisites.ConvertAll<Def>( def => (Def)def ) );
                }
                else
                {
                    var advancedResearchDefs = Controller.Data.AdvancedResearchDefs.Where( a => (
                        ( a.IsBuildingToggle )&&
                        ( !a.HideDefs )&&
                        ( a.thingDefs.Contains( buildableDef as ThingDef ) )
                    ) ).ToList();
                    if( !advancedResearchDefs.NullOrEmpty() )
                    {
                        foreach( var advancedResearchDef in advancedResearchDefs )
                        {
                            researchDefs.AddRangeUnique( advancedResearchDef.researchDefs.ConvertAll<Def>( def => (Def)def ) );
                        }
                    }
                }
            }

			// Return the list of research required
			return researchDefs;
		}

		public static List<RecipeDef> GetRecipeDefs( this BuildableDef buildableDef )
		{
			return
				DefDatabase<RecipeDef>.AllDefsListForReading.Where(
					r => r.products.Any( tc => tc.thingDef == buildableDef as ThingDef ) ).ToList();
		}

		#endregion

		#region Comp Properties

		public static CommunityCoreLibrary.CompProperties_RestrictedPlacement RestrictedPlacement_Properties( this BuildableDef buildableDef )
		{
			if( buildableDef is TerrainWithComps )
			{
				// Terrain with comps
                return ( (TerrainWithComps)buildableDef ).GetCompProperties( typeof( CompRestrictedPlacement ) ) as CompProperties_RestrictedPlacement;
			}
			else if( buildableDef is ThingDef )
			{
				// Thing with comps
				return ( (ThingDef)buildableDef ).GetCompProperties<CompProperties_RestrictedPlacement>();
			}

			// Something else
			return (CompProperties_RestrictedPlacement)null;
		}

		#endregion

	}

}
