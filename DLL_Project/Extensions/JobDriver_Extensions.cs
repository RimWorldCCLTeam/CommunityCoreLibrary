using System;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public static class JobDriver_Extensions
    {

        #region Target Things

        public static Thing TargetThingA( this JobDriver obj )
        {
            return obj.pawn.jobs.curJob.targetA.Thing;
        }

        public static Thing TargetThingB( this JobDriver obj )
        {
            return obj.pawn.jobs.curJob.targetB.Thing;
        }

        public static Thing TargetThingC( this JobDriver obj )
        {
            return obj.pawn.jobs.curJob.targetC.Thing;
        }

        #endregion

        #region Target Locations

        public static IntVec3 TargetLocA( this JobDriver obj )
        {
            return obj.pawn.jobs.curJob.targetA.Cell;
        }

        public static IntVec3 TargetLocB( this JobDriver obj )
        {
            return obj.pawn.jobs.curJob.targetB.Cell;
        }

        public static IntVec3 TargetCellC( this JobDriver obj )
        {
            return obj.pawn.jobs.curJob.targetC.Cell;
        }

        #endregion

    }

}
