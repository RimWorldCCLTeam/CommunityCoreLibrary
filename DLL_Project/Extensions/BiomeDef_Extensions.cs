using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public static class BiomeDef_Extensions
    {
        public static List<TerrainDef> AllTerrainDefs( this BiomeDef biome )
        {
            List<TerrainDef> ret = new List<TerrainDef>();

            // map terrain
            if ( !biome.terrainsByFertility.NullOrEmpty() )
            {
                ret.AddRange( biome.terrainsByFertility.Select( t => t.terrain ) );
            }

            // patch maker terrain
            if ( !biome.terrainPatchMakers.NullOrEmpty() )
            {
                foreach ( TerrainPatchMaker patchMaker in biome.terrainPatchMakers )
                {
                    ret.AddRange( patchMaker.thresholds.Select( t => t.terrain ) );
                }
            }

            return ret;
        } 
    }
}
