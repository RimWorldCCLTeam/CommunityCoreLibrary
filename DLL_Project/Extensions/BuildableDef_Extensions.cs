using System;
using System.Collections.Generic;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public static class BuildableDef_Extensions
    {

        #region Availability

        public static bool                  IsLockedOut( this BuildableDef b )
        {
            if( b.researchPrerequisite == null )
            {
                return false;
            }
            // Is the designationCategory locked out?
            if( ( string.IsNullOrEmpty( b.designationCategory ) )||
                ( b.designationCategory == "None" ) )
            {
                return true;
            }
            // Is the research parent locked out?
            return b.researchPrerequisite.IsLockedOut();
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
