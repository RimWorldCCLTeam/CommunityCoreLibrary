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
        private static Toil TakeFromSynthesier( TargetIndex ind, Pawn eater, Func<ThingDef,bool> validator, Func<ThingDef,ThingDef,int> sorter )
        {
            var synthesizer = (Building_AutomatedFactory) eater.jobs.curJob.GetTarget( ind ).Thing;
            var bestDef = synthesizer.BestProduct( validator, sorter );
            var takeFromSynthesizer = new Toil();
            //Log.Message( string.Format( "{0}.TakeMealFromSynthesizier( {1}, {2} )", eater == null ? "null" : eater.NameStringShort, synthesizer == null ? "null" : synthesizer.ThingID, bestDef == null ? "null" : bestDef.defName ) );
            if( bestDef == null )
            {
                takeFromSynthesizer.defaultCompleteMode = ToilCompleteMode.Delay;
                takeFromSynthesizer.AddEndCondition( () =>
                    {
                        return JobCondition.Incompletable;
                    }
                );
                takeFromSynthesizer.defaultDuration = 999;
            }
            else
            {
                takeFromSynthesizer.defaultCompleteMode = ToilCompleteMode.Delay;
                takeFromSynthesizer.AddFinishAction( () =>
                    {
                        Thing thing = synthesizer.TryProduceThingDef( bestDef );
                        if( thing == null )
                        {
                            Log.Error( eater.Label + " unable to take " + bestDef.label + " from " + synthesizer.ThingID );
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
            }
            return takeFromSynthesizer;
        }

        public static Toil TakeAlcoholFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            return TakeFromSynthesier( ind, eater, FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol );
        }

        public static Toil TakeMealFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            return TakeFromSynthesier( ind, eater, FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
        }

    }

}
