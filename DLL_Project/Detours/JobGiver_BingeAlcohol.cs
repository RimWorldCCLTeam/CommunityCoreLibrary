using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _JobGiver_BingeAlcohol
    {

        internal static MethodInfo  _IgnoreForbid;

        #region Reflected Methods

        internal static bool IgnoreForbid( this JobGiver_Binge obj, Pawn pawn )
        {
            if( _IgnoreForbid == null )
            {
                _IgnoreForbid = typeof( JobGiver_Binge ).GetMethod( "IgnoreForbid", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool)_IgnoreForbid.Invoke( obj, new object[] { pawn } );
        }

        #endregion

        internal static Thing _BestConsumeTarget( this JobGiver_BingeAlcohol obj, Pawn pawn )
        {
            // Set flag to let FoodUtility.GetFoodDef() know we are looking for alcohol, not food
            _FoodUtility._GetFoodDefAlcohol = true;
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                ThingRequest.ForUndefined(),
                PathEndMode.OnCell,
                TraverseParms.For(
                    pawn,
                    pawn.NormalMaxDanger(),
                    TraverseMode.ByPawn,
                    false ),
                9999f,
                (thing) =>
            {
                if(
                    ( obj.IgnoreForbid( pawn ) )||
                    ( !thing.IsForbidden( pawn ) )
                )
                {
                    return pawn.CanReserve( thing, 1 );
                }
                return false;
            },
                Find.ListerThings.AllThings
                .Where( thing => thing.def.IsAlcohol() )
                .Concat(
                    (IEnumerable<Thing>)Find.ListerBuildings.AllBuildingsColonistOfClass<Building_AutomatedFactory>()
                    .Where( factory => (
                        ( factory.OutputToPawnsDirectly )&&
                        ( factory.BestProduct( FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol ) != null )
                    ) )
                ),
                -1,
                false
            );
        }

    }

}
