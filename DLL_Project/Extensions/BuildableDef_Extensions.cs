using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class BuildableDef_Extensions
    {
        
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
