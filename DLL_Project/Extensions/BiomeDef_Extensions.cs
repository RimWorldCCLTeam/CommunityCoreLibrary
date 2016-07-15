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
                ret.AddRangeUnique( biome.terrainsByFertility.Select( t => t.terrain ) );
            }

            // patch maker terrain
            if ( !biome.terrainPatchMakers.NullOrEmpty() )
            {
                foreach ( TerrainPatchMaker patchMaker in biome.terrainPatchMakers )
                {
                    ret.AddRangeUnique( patchMaker.thresholds.Select( t => t.terrain ) );
                }
            }

            return ret;
        }

        public static List<IncidentDef> AllDiseases( this BiomeDef biome )
        {
            var list = new List<IncidentDef>();
            var incidents = DefDatabase<IncidentDef>.AllDefsListForReading;
            if( !incidents.NullOrEmpty() )
            {
                foreach( var incident in incidents )
                {
                    var commonality = biome.CommonalityOfDisease( incident );
                    if( commonality > 0.0f )
                    {
                        list.Add( incident );
                    }
                }
            }
            return list;
        }

    }

}
