using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class RestrictedPlacement_Comp : ThingComp
    {

        static int                          tickCount;

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

        // Make sure things stagger their checks to prevent lag spikes
        public override void                PostExposeData()
        {
            tickCount = parent.GetHashCode() % 250;
        }

        public override void                PostSpawnSetup()
        {
#if DEBUG
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
#endif
            tickCount = parent.GetHashCode() % 250;
        }

        // Tick for checks
        public override void                CompTick()
        {
            DoChecks( 1 );
        }

        public override void                CompTickRare()
        {
            DoChecks( 250 );
        }

        // Oops!  We shouldn't be allowed!
        public void                         DestroyParent()
        {
            PlaceWorker_Restriction_Alert_Data.Add( parent );
            parent.Destroy( DestroyMode.Kill );
        }

        public void                         DoChecks( int ticks )
        {   // This function maintains validation of the placement restrictions and
            // destroys the parent thing if the requirements have changed for certain things
            tickCount -= ticks;
            if( tickCount >= 0 )
            {
                return;
            }
            tickCount = 250;

            // Check for a roof
            if(
                ( PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_OnlyUnderRoof ) ) ) )&&
                ( !Find.RoofGrid.Roofed( parent.Position ) )
            )
            {
                DestroyParent();
                return;
            }

            // Check wall support
            if( PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_WallAttachment ) ) ) )
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
            if( PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_OnlyOnSurface ) ) ) )
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
            if( PlaceWorkers.Exists( p => p.GetType().IsSubclassOf( typeof( PlaceWorker_OnlyOnThing ) ) ) )
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
