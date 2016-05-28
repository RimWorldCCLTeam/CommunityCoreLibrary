using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class WorkGiver_FillHopper : WorkGiver_Scanner
    {

        static List< ThingDef >             HopperDefs;
        static int                          HopperIndex;

        public override ThingRequest        PotentialWorkThingRequest
        {
            get
            {
                if( HopperDefs.NullOrEmpty() )
                {
                    HopperDefs = DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                        ( t.IsHopper() )
                    ) ).ToList();
                    HopperIndex = 0;
                }
                else
                {
                    HopperIndex = ( ++HopperIndex ) % HopperDefs.Count;
                }
                return ThingRequest.ForDef( HopperDefs[ HopperIndex ] );
            }
        }

        public override PathEndMode         PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public WorkGiver_FillHopper()
        {
        }

        public override Job                 JobOnThing( Pawn pawn, Thing t )
        {
            if( !pawn.CanReserveAndReach( ( TargetInfo )t.Position, PathEndMode.Touch, DangerUtility.NormalMaxDanger( pawn ), 1 ) )
            {
                return (Job) null;
            }

            var hopperSgp = t as ISlotGroupParent;
            if( hopperSgp == null )
            {
                return (Job) null;
            }

            var resource = HopperGetCurrentResource( t.Position, hopperSgp );
            if(
                ( resource == null )||
                ( resource.stackCount <= ( resource.def.stackLimit / 2 ) )
            )
            {
                return WorkGiver_FillHopper.HopperFillJob( pawn, hopperSgp, resource );
            }

            JobFailReason.Is( "AlreadyFilledLower".Translate() );
            return (Job) null;
        }

        private static Thing                HopperGetCurrentResource( IntVec3 position, ISlotGroupParent hopperSgp )
        {
            var list = Find.ThingGrid.ThingsListAt( position ).Where( t => (
                ( !HopperDefs.Contains( t.def ) )&&
                ( hopperSgp.GetStoreSettings().AllowedToAccept( t ) )
            ) ).ToList();
            if( list.NullOrEmpty() )
            {
                return (Thing) null;
            }

            return list.First();
        }

        private static Job                  HopperFillJob( Pawn pawn, ISlotGroupParent hopperSgp, Thing resource )
        {
            Building building = hopperSgp as Building;

            // Get a sorted list (by distance) of matching resources
            List< Thing > resources = null;

            if( resource != null )
            {
                resources = Find.Map.listerThings.ThingsOfDef( resource.def )
                    .Where( t => (
                        ( HaulAIUtility.PawnCanAutomaticallyHaul( pawn, t ) )&&
                        ( hopperSgp.GetStoreSettings().AllowedToAccept( t ) )&&
                        ( HaulAIUtility.StoragePriorityAtFor( t.Position, t ) < hopperSgp.GetSlotGroup().Settings.Priority )
                    ) ).ToList();
            }
            else
            {
                resources = Find.Map.listerThings.AllThings
                    .Where( t => (
                        ( HaulAIUtility.PawnCanAutomaticallyHaul( pawn, t ) )&&
                        ( hopperSgp.GetStoreSettings().AllowedToAccept( t ) )&&
                        ( HaulAIUtility.StoragePriorityAtFor( t.Position, t ) < hopperSgp.GetSlotGroup().Settings.Priority )
                    ) ).ToList();
            }

            if( resources.NullOrEmpty() )
            {
                return (Job) null;
            }

            // Sort by distance (closest first)
            resources.Sort( ( Thing x, Thing y ) => ( Gen.ManhattanDistanceFlat( x.Position, building.Position ) < Gen.ManhattanDistanceFlat( y.Position, building.Position ) ) ? -1 : 1 );

            var grabResource = resources.First();
            if( grabResource != null )
            {
                // Try to haul the first (closest) resource found
                var job = HaulAIUtility.HaulMaxNumToCellJob( pawn, grabResource, building.Position, true );
                if( job != null )
                {
                    return job;
                }
            }
            return (Job) null;
        }

    }

}
