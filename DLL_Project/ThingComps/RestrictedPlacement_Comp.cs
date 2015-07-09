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

    public class RestrictedPlacement_Comp : ThingComp
    {
        private static int          tickCount;

        private RestrictedPlacement_Properties Restrictions
        {
            get
            {
                return (RestrictedPlacement_Properties)props;
            }
        }

        private List<PlaceWorker> PlaceWorkers
        {
            get
            {
                return parent.def.PlaceWorkers;
            }
        }

        // Make sure things stagger their checks to prevent lag spikes
        public override void PostExposeData() {
            tickCount = parent.GetHashCode() % 250;
        }
        public override void PostSpawnSetup() {
            tickCount = parent.GetHashCode() % 250;
        }

        // Tick for checks
        public override void CompTick() {
            DoChecks( 1 );
        }
        public override void CompTickRare() {
            DoChecks( 250 );
        }

        // Oops!  We shouldn't be allowed!
        public void DestroyParent()
        {
            PlaceWorker_Restriction_Alert_Data.Add( parent );
            parent.Destroy( DestroyMode.Kill );
        }

        public void DoChecks( int ticks )
        {   // This function maintains validation of the placement restrictions and
            // destroys the parent thing if the requirements have changed for certain things
            tickCount -= ticks;
            if( tickCount >= 0 )
                return;
            tickCount = 250;

            // Check for a roof
            if( ( PlaceWorkers.FindIndex( p => p.GetType() == typeof( PlaceWorker_OnlyUnderRoof ) ) >= 0 )&&
                ( Find.RoofGrid.Roofed( parent.Position ) == false ) ) {
                DestroyParent();
                return;
            }

            // Check wall support
            if( PlaceWorkers.FindIndex( p => p.GetType() == typeof( PlaceWorker_WallAttachment ) ) >= 0 ) {
                IntVec3 c = parent.Position + parent.Rotation.FacingCell * -1;
                Building support = c.GetEdifice();
                if( ( support == null )||
                    ( ( support.def.graphicData.linkFlags & ( LinkFlags.Rock | LinkFlags.Wall ) ) == 0 ) ) {
                    DestroyParent();
                    return;
                }
            }

            // Check surface
            if( PlaceWorkers.FindIndex( p => p.GetType() == typeof( PlaceWorker_OnlyOnSurface ) ) >= 0 ) {
                bool foundThing = false;
                foreach( Thing t in parent.Position.GetThingList() ) {
                    if( t.def.surfaceType != SurfaceType.None ) {
                        foundThing = true;
                        break;
                    }
                }
                if( foundThing == false ) {
                    DestroyParent();
                    return;
                }
            }

            // Check on thing
            if( PlaceWorkers.FindIndex( p => p.GetType() == typeof( PlaceWorker_OnlyOnThing ) ) >= 0 ) {
                bool foundThing = false;
                foreach( Thing t in parent.Position.GetThingList() ){
                    if( ( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )&&
                        ( t.Position == parent.Position ) ) {
                        foundThing = true;
                        break;
                    }
                }
                if( foundThing == false ) {
                    DestroyParent();
                    return;
                }
            }

        }
    }
}

