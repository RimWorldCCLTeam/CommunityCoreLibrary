using System;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public static class JobDriver_Extensions
    {

        #region General Targets by Index

        public static TargetInfo Target( this JobDriver obj, TargetIndex Ind )
        {
            return obj.pawn.CurJob.GetTarget( Ind );
        }

        public static Thing TargetThing( this JobDriver obj, TargetIndex Ind )
        {
            return obj.Target( Ind ).Thing;
        }

        public static IntVec3 TargetCell( this JobDriver obj, TargetIndex Ind )
        {
            return obj.Target( Ind ).Cell;
        }

        #endregion

        #region Indexed Target Things

        public static Thing TargetThingA( this JobDriver obj )
        {
            return obj.TargetThing( TargetIndex.A );
        }

        public static Thing TargetThingB( this JobDriver obj )
        {
            return obj.TargetThing( TargetIndex.B );
        }

        public static Thing TargetThingC( this JobDriver obj )
        {
            return obj.TargetThing( TargetIndex.C );
        }

        #endregion

        #region Indexed Target Cells

        public static IntVec3 TargetCellA( this JobDriver obj )
        {
            return obj.TargetCell( TargetIndex.A );
        }

        public static IntVec3 TargetCellB( this JobDriver obj )
        {
            return obj.TargetCell( TargetIndex.B );
        }

        public static IntVec3 TargetCellC( this JobDriver obj )
        {
            return obj.TargetCell( TargetIndex.C );
        }

        #endregion

    }

}
