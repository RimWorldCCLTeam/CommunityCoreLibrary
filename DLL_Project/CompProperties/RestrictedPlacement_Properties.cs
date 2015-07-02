using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class RestrictedPlacement_Properties : CompProperties
    {
        // These may be defined in xml or left as the default
        public int                  MaxCount = -1;
        public List< TerrainDef >   RestrictedTerrain = null;

        public RestrictedPlacement_Properties()
        {
            this.compClass = typeof( RestrictedPlacement_Properties );
        }
    }
}

