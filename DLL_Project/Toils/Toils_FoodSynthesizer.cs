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
                        Find.Reservations.Release( synthesizer, eater );
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
                        var meal = synthesizer.TryProduceThingDef( bestDef );
                        Find.Reservations.Release( synthesizer, eater );
                        if( meal == null )
                        {   // This should never happen, why is it?
                            Log.Error( eater.Label + " unable to take " + bestDef.label + " from " + synthesizer.ThingID );
                            eater.jobs.curDriver.EndJobWith( JobCondition.Incompletable );
                        }
                        else
                        {
                            eater.carrier.TryStartCarry( meal );
                            eater.jobs.curJob.SetTarget(TargetIndex.C, (TargetInfo) eater.carrier.CarriedThing);
                        }
                    }
                );
                takeFromSynthesizer.defaultDuration = synthesizer.ProductionTicks( bestDef );
            }
            return takeFromSynthesizer;
        }

        public static Toil TakeDrugFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            //would like to put this in the FoodSynthesis class but this would require changing the allowed function
            //signature to take pawn as well. And a full refactor of all of the functions.
            Func<ThingDef, bool> validator = FoodSynthesis.IsDrug;
            var synthesizer = (Building_AutomatedFactory) eater.jobs.curJob.GetTarget( ind ).Thing;
            if (eater.MentalState is MentalState_BingingDrug)
            {
                var bingingChemical = ((MentalState_BingingDrug) eater.MentalState).chemical;
                //Pawn is binging, will only take the drug that satisfies need.
                validator = thingDef =>
                {
                    if (thingDef.HasComp(typeof(CompProperties_Drug)))
                    {
                        var drugComp = (CompProperties_Drug) thingDef.GetCompProperty(typeof(CompProperties_Drug));
                        return drugComp.chemical == bingingChemical;
                    }
                    return false;
                };
            }
            else
            {
                ThingDef drug = synthesizer.GetRecipeForPawn(eater);
                if (drug != null)
                {
                    //RecipeForPawn was set during the social relax search. This is the drug the pawn ended up with.
                    //Already checked for drug policy.
                    validator = thingDef =>
                    {
                        return thingDef == drug;
                    };
                    synthesizer.ClearRecipeForPawn(eater);
                }
                else
                {
                    if (eater.drugs != null)
                    {
                        var drugPolicy = eater.drugs;
                        //Pawn will only take allowed drugs.
                        validator = thingDef =>
                        {
                            return eater.drugs.AllowedToTakeScheduledNow(thingDef);
                        };
                    }
                }
            }
            return TakeFromSynthesier( ind, eater, validator, FoodSynthesis.SortDrug );
        }

        public static Toil TakeMealFromSynthesizer( TargetIndex ind, Pawn eater )
        {
            return TakeFromSynthesier( ind, eater, FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
        }

    }

}
