using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class RestrictedPlacement_Properties : CompProperties
    {

        #region XML Data

        public int                          MaxCount = -1;
        public List< TerrainDef >           RestrictedTerrain;
        public List< ThingDef >             RestrictedThing;

        #endregion

        public                              RestrictedPlacement_Properties()
        {
            compClass = typeof( RestrictedPlacement_Properties );
        }

    }

}
