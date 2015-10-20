using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class RestrictedPlacement_Properties : CompProperties
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

        public                              RestrictedPlacement_Properties()
        {
            compClass = typeof( RestrictedPlacement_Properties );
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
                    if( ( terrainDefs != null )&&
                        ( terrainDefs.Count > 0 ) )
                    {
                        restrictedTerrain.AddRange( terrainDefs );
                    }

                    // Now check for auto-adds
                    if( IncludeRoughStone )
                    {
                        var roughStone = DefDatabase<TerrainDef>.AllDefsListForReading
                            .FindAll( t =>
                                ( t.defName.Contains( "_Rough" ) )
                            );
                        if( ( roughStone != null )&&
                            ( roughStone.Count > 0 ) )
                        {
                            restrictedTerrain.AddRange( roughStone );
                        }
                    }

                    if( IncludeSmoothStone )
                    {
                        var smoothStone = DefDatabase<TerrainDef>.AllDefsListForReading
                            .FindAll( t =>
                                ( t.defName.Contains( "_Smooth" ) )
                            );
                        if( ( smoothStone != null )&&
                            ( smoothStone.Count > 0 ) )
                        {
                            restrictedTerrain.AddRange( smoothStone );
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
                    if( ( thingDefs != null )&&
                        ( thingDefs.Count > 0 ) )
                    {
                        restrictedThing.AddRange( thingDefs );
                    }
                }
                return restrictedThing;
            }
        }

        #endregion

    }

}
