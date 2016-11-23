using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    [SpecialInjectorSequencer( InjectionSequence.MainLoad, InjectionTiming.SpecialInjectors )]
    public class UpgradeBuildingNutrientPasteDispenser : SpecialInjector
    {

        public override bool                Inject()
        {

            // Change Building_NutrientPasteDispenser into Building_AdvancedPasteDispenser
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => def.thingClass == typeof( Building_NutrientPasteDispenser ) ) )
            {
                def.thingClass = typeof( Building_AdvancedPasteDispenser );
            }

            return true;
        }

    }

}
