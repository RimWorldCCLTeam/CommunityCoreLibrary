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

    public class CompLifespanPowered : ThingComp
    {
        public int          remainingTicks = -1;

        public CompPowerTrader CompPower
        {
            get{
                return parent.TryGetComp<CompPowerTrader>();
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            if( remainingTicks < 0 )
                remainingTicks = this.props.lifespanTicks;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<int>(ref remainingTicks, "remainingTicks", this.props.lifespanTicks, true);
        }

        public override void CompTick()
        {
            base.CompTick();
            TickDown( 1 );
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            TickDown( 250 );
        }

        public void TickDown( int down )
        {
            if( !CompPower.PowerOn )
                return;
            
            remainingTicks -= down;
            if( remainingTicks > 0 )
                return;

            this.parent.Destroy(DestroyMode.Vanish);
        }

        public override string CompInspectStringExtra()
        {
            if (remainingTicks > 0)
            {
                return Translator.Translate("LifespanExpiry") + " " +
                    GenTime.TickstoDaysAndHoursString(remainingTicks) + "\n" +
                    base.CompInspectStringExtra();
            }
            return base.CompInspectStringExtra();
        }
    }
}

