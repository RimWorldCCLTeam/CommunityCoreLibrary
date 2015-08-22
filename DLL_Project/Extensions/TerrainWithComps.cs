using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
	public class TerrainWithComps : TerrainDef
	{

        #region XML Data

        public List< CompProperties >       comps = new List< CompProperties >();
		
        #endregion

        //[Unsaved]

        #region Instance Data

        #endregion

        #region Process State

#if DEBUG
        public override void                PostLoad()
        {
            base.PostLoad();

            // Validate place workers
            if( ( placeWorkers != null )&&
                ( placeWorkers.Count > 0 ) )
            {
                // Terrain with comps only supports a small set of place workers
                foreach( var placeWorker in placeWorkers )
                {
                    if(
                        ( placeWorker != typeof( PlaceWorker_NotOnTerrain ) )&&
                        ( placeWorker != typeof( PlaceWorker_OnlyOnTerrain ) )&&
                        ( placeWorker != typeof( PlaceWorker_NotUnderRoof ) )&&
                        ( placeWorker != typeof( PlaceWorker_OnlyUnderRoof ) )
                    )
                    {
                        Log.Error( "Community Core Library :: TerrainWithComps :: PlaceWorker( " + placeWorker.FullName + " ) is invalid for TerrainWithComps in " + defName );
                    }
                }
            }

            // Validate comps
            if( ( comps != null )&&
                ( comps.Count > 0 ) )
            {
                // Terrain with comps only supports a small set of comps
                for( int i = 0; i < comps.Count; ++i )
                {
                    var comp = comps[ i ];
                    var compClass = comp.compClass;
                    if(
                        ( compClass != typeof( RestrictedPlacement_Comp ) )
                    )
                    {
                        Log.Error( "Community Core Library :: TerrainWithComps :: comp( " + compClass.FullName + " ) is invalid for TerrainWithComps in " + defName );
                    }
                }
            }
        }
#endif

        #endregion

        #region Query State

		public CompProperties               GetCompProperties( Type compType )
		{
            for( int i = 0; i < comps.Count; i++ )
            {
                if( comps[ i ].compClass == compType )
                {
                    return comps[ i ];
				}
			}
            return (CompProperties) null;
		}

        #endregion

	}

}
