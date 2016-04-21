using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class CompProperties_RestrictedPlacement : CompProperties
    {

        #region XML Data

        // Maximum count of a thing which may exist at any given time
        public int                          MaxCount = -1;

        // These only apply to terrain restrictions, use them to auto-add
        // Rough of Smooth stone terrain to the list of Restricted Terrain
        // without having to list each one individually
        public bool                         IncludeRoughStone;
        public bool                         IncludeSmoothStone;

        // List of terrain to filter by
        public List< TerrainDef >           terrainDefs;

        // List of things to filter by
        public List< ThingDef >             thingDefs;

        #endregion

        [Unsaved]

        #region Instance Data

        List< TerrainDef >                  restrictedTerrain;
        List< ThingDef >                    restrictedThing;

        #endregion

        #region Process State

        public                              CompProperties_RestrictedPlacement()
        {
            compClass = typeof( CompRestrictedPlacement );
        }

        #endregion

        #region Query State

        public List< TerrainDef >           RestrictedTerrain
        {
            get
            {
                if( restrictedTerrain == null )
                {
                    // Create cache
                    restrictedTerrain = new List< TerrainDef >();

                    // Add xml defined terrain
                    if( !terrainDefs.NullOrEmpty() )
                    {
                        restrictedTerrain.AddRangeUnique( terrainDefs );
                    }

                    // Now check for auto-adds
                    if( IncludeRoughStone )
                    {
                        var roughStone = DefDatabase<TerrainDef>.AllDefsListForReading
                            .FindAll( t =>
                                ( t.defName.Contains( "_Rough" ) )
                            );
                        if( !roughStone.NullOrEmpty() )
                        {
                            restrictedTerrain.AddRangeUnique( roughStone );
                        }
                    }

                    if( IncludeSmoothStone )
                    {
                        var smoothStone = DefDatabase<TerrainDef>.AllDefsListForReading
                            .FindAll( t =>
                                ( t.defName.Contains( "_Smooth" ) )
                            );
                        if( !smoothStone.NullOrEmpty() )
                        {
                            restrictedTerrain.AddRangeUnique( smoothStone );
                        }
                    }

                }
                return restrictedTerrain;
            }
        }

        public List< ThingDef >             RestrictedThing
        {
            get
            {
                if( restrictedThing == null )
                {
                    // Create cache
                    restrictedThing = new List< ThingDef >();

                    // Add xml defined things
                    if( !thingDefs.NullOrEmpty() )
                    {
                        restrictedThing.AddRangeUnique( thingDefs );
                    }
                }
                return restrictedThing;
            }
        }

        #endregion

    }

}
