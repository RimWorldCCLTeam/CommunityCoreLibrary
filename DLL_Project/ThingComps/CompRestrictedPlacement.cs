using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class CompRestrictedPlacement : ThingComp
    {

        List< PlaceWorker >                 PlaceWorkers
        {
            get
            {
                return parent.def.PlaceWorkers;
            }
        }

        public bool                         IsTerrainRestriction
        {
            get
            {
                return(
                    ( this.parent.def.PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_OnlyOnTerrain ) ) ) )||
                    ( this.parent.def.PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_NotOnTerrain ) ) ) )
                );
            }
        }

        public bool                         IsThingRestriction
        {
            get
            {
                return(
                    ( this.parent.def.PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_OnlyOnThing ) ) ) )||
                    ( this.parent.def.PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_NotOnThing ) ) ) )
                );
            }
        }

        public bool                         RequiresProperties
        {
            get
            {
                return(
                    ( IsTerrainRestriction )||
                    ( IsThingRestriction )
                );
            }
        }

#if DEBUG
        public override void                PostSpawnSetup()
        {
            if( this.RequiresProperties )
            {
                var properties = this.RestrictedPlacement_Properties();
                if( properties == null )
                {
                    CCL_Log.TraceMod(
                        parent.def,
                        Verbosity.FatalErrors,
                        "Missing RestrictedPlacement_Properties"
                    );
                    return;
                }
                if(
                    ( IsTerrainRestriction )&&
                    ( properties.RestrictedTerrain.NullOrEmpty() )
                )
                {
                    CCL_Log.TraceMod(
                        parent.def,
                        Verbosity.FatalErrors,
                        "Missing terrainDefs"
                    );
                }
                if(
                    ( IsThingRestriction )&&
                    ( properties.RestrictedThing.NullOrEmpty() )
                )
                {
                    CCL_Log.TraceMod(
                        parent.def,
                        Verbosity.FatalErrors,
                        "Missing thingDefs"
                    );
                }
            }
        }
#endif

        // Tick for checks
        public override void                CompTick()
        {
            base.CompTick();
            if( !parent.IsHashIntervalTick( 250 ) )
            {
                return;
            }
            DoChecks();
        }

        public override void                CompTickRare()
        {
            base.CompTickRare();
            DoChecks();
        }

        // Oops!  We shouldn't be allowed!
        public void                         DestroyParent()
        {
            PlaceWorker_Restriction_Alert_Data.Add( parent );
            parent.Destroy( DestroyMode.Kill );
        }

        public bool                         HasPlaceWorker( Type placeWorker )
        {
            return PlaceWorkers.Exists( p => (
                ( p.GetType() == placeWorker )||
                ( p.GetType().IsSubclassOf( placeWorker ) )
            ) );
        }

        public void                         DoChecks()
        {   // This function maintains validation of the placement restrictions and
            // destroys the parent thing if the requirements have changed for certain things

            // Check for a roof
            if(
                ( HasPlaceWorker( typeof( PlaceWorker_OnlyUnderRoof ) ) )&&
                ( !Find.RoofGrid.Roofed( parent.Position ) )
            )
            {
                DestroyParent();
                return;
            }

            // Check wall support
            if( HasPlaceWorker( typeof( PlaceWorker_WallAttachment ) ) )
            {
                IntVec3 c = parent.Position - parent.Rotation.FacingCell;
                Building support = c.GetEdifice();
                if(
                    ( support == null )||
                    ( ( support.def.graphicData.linkFlags & ( LinkFlags.Rock | LinkFlags.Wall ) ) == 0 )
                )
                {
                    DestroyParent();
                    return;
                }
            }

            // Check surface
            if( HasPlaceWorker( typeof( PlaceWorker_OnlyOnSurface ) ) )
            {
                bool foundThing = false;
                foreach( Thing t in parent.Position.GetThingList() )
                {
                    if( t.def.surfaceType != SurfaceType.None )
                    {
                        foundThing = true;
                        break;
                    }
                }
                if( !foundThing )
                {
                    DestroyParent();
                    return;
                }
            }

            // Check on thing
            if( HasPlaceWorker( typeof( PlaceWorker_OnlyOnThing ) ) )
            {
                var Restrictions = this.RestrictedPlacement_Properties();
                bool foundThing = false;
                foreach( Thing t in parent.Position.GetThingList() )
                {
                    if(
                        ( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )&&
                        ( t.Position == parent.Position )
                    )
                    {
                        foundThing = true;
                        break;
                    }
                }
                if( !foundThing )
                {
                    DestroyParent();
                    return;
                }
            }

        }

    }

}
