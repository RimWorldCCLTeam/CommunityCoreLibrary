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
        // TODO:see other todos for this whole file
        /*private static Toil TakeFromSynthesier( TargetIndex ind, Pawn eater, Func<ThingDef,bool> validator, Func<ThingDef,ThingDef,int> sorter )
        {
            var synthesizer = (Building_AutomatedFactory) eater.jobs.curJob.GetTarget( ind ).Thing;
            var bestDef = synthesizer.BestProduct( validator, sorter );
            var takeFromSynthesizer = new Toil();
            takeFromSynthesizer.defaultCompleteMode = ToilCompleteMode.Delay;
            takeFromSynthesizer.AddFinishAction( () =>
                {
                    var def = synthesizer.BestProduct( validator, sorter );
                    Thing thing = synthesizer.TryProduceThingDef( def );
                    if( thing == null )
                    {
                        Log.Error( eater.Label + " unable to take " + def.label + " from " + synthesizer.ThingID );
                        eater.jobs.curDriver.EndJobWith( JobCondition.Incompletable );
                    }
                    else
                    {
                        eater.carrier.TryStartCarry( thing );
                        eater.jobs.curJob.targetA = (TargetInfo) eater.carrier.CarriedThing;
                    }
                }
            );
            takeFromSynthesizer.defaultDuration = synthesizer.ProductionTicks( bestDef );
            return takeFromSynthesizer;
        }*/

        /*public static Toil TakeAlcoholFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            return TakeFromSynthesier( ind, eater, FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol );
        }*/

        /*public static Toil TakeMealFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            return TakeFromSynthesier( ind, eater, FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
        }*/

    }

}
