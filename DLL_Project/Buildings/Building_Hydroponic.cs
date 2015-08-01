using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class Building_Hydroponic : Building_PlantGrower
    {
        private CompPowerTrader compPower{
            get { return this.TryGetComp<CompPowerTrader>(); }
        }
        private List<ThingComp> comps{
            get { return this.AllComps; }
        }

        public override void TickRare()
        {
            // Building_PlantGrower does not call base.TickRare() so we have to do it here
            for (int index = 0; index < this.comps.Count; ++index)
                this.comps[index].CompTickRare();
            
            if (this.compPower == null || this.compPower.PowerOn)
                return;
            foreach (Thing thing in this.PlantsOnMe)
                thing.TakeDamage(new DamageInfo(DamageDefOf.Rotting, 4, (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null));
        }
    }
}

