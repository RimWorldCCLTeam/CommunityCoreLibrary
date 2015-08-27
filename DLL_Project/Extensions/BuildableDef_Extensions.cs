using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public static class BuildableDef_Extensions
    {

        #region Availability

        public static bool                  IsLockedOut( this BuildableDef buildableDef )
        {
            // Is the designationCategory locked out?
            if( ( buildableDef.blueprintDef != null )&&
                ( string.IsNullOrEmpty( buildableDef.designationCategory ) )||
                ( buildableDef.designationCategory == "None" ) )
            {
                return true;
            }
            // Advanced research unlocking it?
            if( ResearchController.AdvancedResearch.Exists( a => (
                ( a.IsBuildingToggle )&&
                ( !a.HideDefs )&&
                ( a.thingDefs.Contains( buildableDef as ThingDef ) )
            ) ) )
            {
                return false;
            }
            // Is the research parent locked out?
            return buildableDef.researchPrerequisite.IsLockedOut();
        }

        public static bool                  HasResearchRequirement( this BuildableDef buildableDef )
        {
            // Can't entirely rely on this one check as it's state may change mid-game
            if( buildableDef.researchPrerequisite != null )
            {
                // Easiest check, do it first
                return true;
            }

            // Check for an advanced research unlock
            return
                ResearchController.AdvancedResearch.Exists( a => (
                    ( a.IsBuildingToggle )&&
                    ( !a.HideDefs )||
                    ( a.thingDefs.Contains( buildableDef as ThingDef ) )
                ) );
        }

        #endregion

        #region Lists of affected data

        public static List< Def >           GetResearchRequirements( this BuildableDef buildableDef )
        {
            var researchDefs = new List< Def >();

            if( buildableDef.researchPrerequisite != null )
            {
                if( buildableDef.researchPrerequisite != Research.Locker )
                {
                    researchDefs.Add( buildableDef.researchPrerequisite );
                }
                else
                {
                    var advancedResearchDefs = ResearchController.AdvancedResearch.Where( a => (
                        ( a.IsBuildingToggle )&&
                        ( !a.HideDefs )||
                        ( a.thingDefs.Contains( buildableDef as ThingDef ) )
                    ) ).ToList();
                    foreach( var advancedResearchDef in advancedResearchDefs )
                    {
                        researchDefs.AddRange( advancedResearchDef.researchDefs.ConvertAll<Def>( def =>(Def)def ) );
                    }
                }
            }

            // Return the list of research required
            return researchDefs;
        }

        #endregion

        #region Comp Properties

        public static CommunityCoreLibrary.RestrictedPlacement_Properties RestrictedPlacement_Properties ( this BuildableDef buildableDef )
        {
            if( buildableDef.GetType() == typeof( TerrainWithComps ) )
            {
                // Terrain with comps
                return ( (TerrainWithComps)buildableDef ).GetCompProperties( typeof( RestrictedPlacement_Comp ) ) as RestrictedPlacement_Properties;
            }
            else if ( buildableDef.GetType() == typeof( ThingDef ) )
            {
                // Thing with comps
                return ( (ThingDef)buildableDef ).GetCompProperties( typeof( RestrictedPlacement_Comp ) ) as RestrictedPlacement_Properties;
            }

            // Something else
            return (RestrictedPlacement_Properties) null;
        }

        #endregion

    }

}
