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

        // Make sure things stagger their checks to prevent lag spikes
        public override void                PostExposeData()
        {
            tickCount = parent.GetHashCode() % 250;
        }

        public override void                PostSpawnSetup()
        {
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
            if( ( PlaceWorkers.Exists( p => p.GetType() == typeof( PlaceWorker_OnlyUnderRoof ) ) )&&
                ( !Find.RoofGrid.Roofed( parent.Position ) ) )
            {
                DestroyParent();
                return;
            }

            // Check wall support
            if( PlaceWorkers.Exists( p => p.GetType() == typeof( PlaceWorker_WallAttachment ) ) ) {
                IntVec3 c = parent.Position - parent.Rotation.FacingCell;
                Building support = c.GetEdifice();
                if( ( support == null )||
                    ( ( support.def.graphicData.linkFlags & ( LinkFlags.Rock | LinkFlags.Wall ) ) == 0 ) )
                {
                    DestroyParent();
                    return;
                }
            }

            // Check surface
            if( PlaceWorkers.Exists( p => p.GetType() == typeof( PlaceWorker_OnlyOnSurface ) ) )
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

            var Restrictions = this.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                Log.Error( "Community Core Library :: RestrictedPlacement_Comp :: " + parent.def.defName + " requires RestrictedPlacement_Properties!" );
                return;
            }
#endif

            // Check on thing
            if( PlaceWorkers.Exists( p => p.GetType() == typeof( PlaceWorker_OnlyOnThing ) ) )
            {
                bool foundThing = false;
                foreach( Thing t in parent.Position.GetThingList() )
                {
                    if( ( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )&&
                        ( t.Position == parent.Position ) )
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
