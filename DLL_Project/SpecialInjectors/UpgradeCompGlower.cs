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
    public class UpgradeCompGlower : SpecialInjector
    {

        public override bool                Inject()
        {
            // Change CompGlower into CompGlowerToggleable
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => (
                ( def != null )&&
                ( def.comps != null )&&
                ( def.HasComp( typeof( CompGlower ) ) )
            ) ) )
            {
                var compGlower = def.GetCompProperties<CompProperties_Glower>();
                compGlower.compClass = typeof( CompGlowerToggleable );
            }

            return true;
        }

    }

}
