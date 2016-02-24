using System;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public static class JobDriver_Extensions
    {

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

    }

}
