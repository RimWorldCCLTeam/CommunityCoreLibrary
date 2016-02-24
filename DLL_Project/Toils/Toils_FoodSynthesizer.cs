using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public static class Toils_FoodSynthesizer
    {

        public static Toil TakeAlcoholFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            var synthesizer = (Building_FoodSynthesizer) eater.jobs.curJob.GetTarget( ind ).Thing;
            var alcoholDef = synthesizer.BestAlcoholFrom();
            var takeFromSynthesizer = new Toil();
            takeFromSynthesizer.defaultCompleteMode = ToilCompleteMode.Delay;
            takeFromSynthesizer.AddFinishAction( () =>
                {
                    Thing thing = synthesizer.TryDispenseAlcohol( alcoholDef );
                    if( thing == null )
                    {
                        eater.jobs.curDriver.EndJobWith( JobCondition.Incompletable );
                    }
                    else
                    {
                        eater.carrier.TryStartCarry( thing );
                        eater.jobs.curJob.targetA = (TargetInfo) eater.carrier.CarriedThing;
                    }
                }
            );
            takeFromSynthesizer.defaultDuration = synthesizer.CollectDuration( alcoholDef );
            return takeFromSynthesizer;
        }

        public static Toil TakeMealFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            Pawn pawn = eater;
            var synthesizer = (Building_FoodSynthesizer) pawn.jobs.curJob.GetTarget( ind ).Thing;
            var mealDef = synthesizer.BestMealFrom();
            var takeFromSynthesizer = new Toil();
            takeFromSynthesizer.defaultCompleteMode = ToilCompleteMode.Delay;
            takeFromSynthesizer.AddFinishAction( () =>
                {
                    Thing thing = synthesizer.TryDispenseMeal( mealDef );
                    if( thing == null )
                    {
                        eater.jobs.curDriver.EndJobWith( JobCondition.Incompletable );
                    }
                    else
                    {
                        eater.carrier.TryStartCarry( thing );
                        eater.jobs.curJob.targetA = (TargetInfo) eater.carrier.CarriedThing;
                    }
                }
            );
            takeFromSynthesizer.defaultDuration = synthesizer.CollectDuration( mealDef );
            return takeFromSynthesizer;
        }

    }

}
