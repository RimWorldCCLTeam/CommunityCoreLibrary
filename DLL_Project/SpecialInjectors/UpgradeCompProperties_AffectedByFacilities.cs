using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    [SpecialInjectorSequencer]
    public class UpgradeCompProperties_AffectedByFacilities : SpecialInjector
    {

        public override bool                Inject()
        {
            // Change RimWorld.CompProperties_AffectedByFacilities into CommunityCoreLibrary.CompProperties_AffectedByFacilities
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => (
                ( def != null )&&
                ( def.comps != null )&&
                ( def.GetCompProperties<RimWorld.CompProperties_AffectedByFacilities>() != null )
            ) ) )
            {
                var propsAffectedBase = def.GetCompProperties<RimWorld.CompProperties_AffectedByFacilities>();
                var propsAffectedCCL = new CommunityCoreLibrary.CompProperties_AffectedByFacilities( propsAffectedBase );
                var baseIndex = def.comps.IndexOf( propsAffectedBase );
                def.comps.Remove( propsAffectedBase );
                def.comps.Insert( baseIndex, propsAffectedCCL );
            }

            return true;
        }

    }

}
