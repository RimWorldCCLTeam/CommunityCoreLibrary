using System.Linq;
using System.Text;
using UnityEngine;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompRottableRefrigerated_Injector : SpecialInjector
    {
        
        public override void Inject()
        {
            // Replace CompRottable on ThingDefs
            var thingDefs = DefDatabase< ThingDef >.AllDefsListForReading;
            if( !thingDefs.NullOrEmpty() )
            {
                foreach( var thingDef in thingDefs )
                {
                    if( !thingDef.comps.NullOrEmpty() )
                    {
                        foreach( var prop in thingDef.comps )
                        {
                            if( prop.GetType() == typeof( CompProperties_Rottable ) )
                            {
                                prop.compClass = typeof( CompRottableRefrigerated );
                            }
                        }
                    }
                }
            }

        }

    }

    public class CompRottableRefrigerated : CompRottable
    {
        private CompProperties_Rottable PropsRot
        {
            get
            {
                return (CompProperties_Rottable) this.props;
            }
        }

        private CompRefrigerated CompRefigerated
        {
            get
            {
                var foo = parent.Position.GetThingList().Where( t => (
                    ( t.TryGetComp<CompRefrigerated>() != null )
                ) ).ToList();
                if( !foo.NullOrEmpty() )
                {
                    return foo.First().TryGetComp<CompRefrigerated>();
                }
                return null;
            }
        }

        private CompPowerTrader CompPowerTrader
        {
            get
            {
                var foo = CompRefigerated;
                if( foo != null )
                {
                    return foo.parent.TryGetComp<CompPowerTrader>();
                }
                return null;
            }
        }

        private bool InRefrigerator
        {
            get
            {
                return (
                    ( CompPowerTrader != null )&&
                    ( CompPowerTrader.PowerOn )
                );
            }
        }

        public override void CompTickRare()
        {
            if( this.InRefrigerator )
            {
                return;
            }
            float num = this.rotProgress;
            this.rotProgress += (float) Mathf.RoundToInt(1f * GenTemperature.RotRateAtTemperature(GenTemperature.GetTemperatureForCell(this.parent.Position)) * 250f);
            if (this.Stage == RotStage.Rotting && this.PropsRot.rotDestroys)
            {
                this.parent.Destroy(DestroyMode.Vanish);
            }
            else
            {
                if (this.Stage != RotStage.Rotting || this.PropsRot.rotDamagePerDay <= 0 || Mathf.FloorToInt(num / 30000f) == Mathf.FloorToInt(this.rotProgress / 30000f))
                    return;
                this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, this.PropsRot.rotDamagePerDay, (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null));
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            switch (this.Stage)
            {
            case RotStage.Fresh:
                stringBuilder1.AppendLine(Translator.Translate("RotStateFresh"));
                break;
            case RotStage.Rotting:
                stringBuilder1.AppendLine(Translator.Translate("RotStateRotting"));
                break;
            case RotStage.Dessicated:
                stringBuilder1.AppendLine(Translator.Translate("RotStateDessicated"));
                break;
            }
            if ( InRefrigerator )
            {
                stringBuilder1.AppendLine("RefrigeratedStorage".Translate());
            }
            else if ((double) ((float) this.PropsRot.TicksToRotStart - this.rotProgress) > 0.0)
            {
                float num = GenTemperature.RotRateAtTemperature((float) Mathf.RoundToInt(GenTemperature.GetTemperatureForCell(this.parent.Position)));
                int rotAtCurrentTemp = this.TicksUntilRotAtCurrentTemp;
                if ((double) num < 1.0 / 1000.0 )
                    stringBuilder1.AppendLine(Translator.Translate("CurrentlyFrozen"));
                else if ( (double) num < 0.999000012874603 )
                {
                    StringBuilder stringBuilder2 = stringBuilder1;
                    string key = "CurrentlyRefrigerated";
                    object[] objArray = new object[1];
                    int index = 0;
                    string str1 = GenDate.TicksToDaysExtendedString(rotAtCurrentTemp);
                    objArray[index] = (object) str1;
                    string str2 = Translator.Translate(key, objArray);
                    stringBuilder2.AppendLine(str2);
                }
                else
                {
                    StringBuilder stringBuilder2 = stringBuilder1;
                    string key = "NotRefrigerated";
                    object[] objArray = new object[1];
                    int index = 0;
                    string str1 = GenDate.TicksToDaysExtendedString(rotAtCurrentTemp);
                    objArray[index] = (object) str1;
                    string str2 = Translator.Translate(key, objArray);
                    stringBuilder2.AppendLine(str2);
                }
            }
            return stringBuilder1.ToString();
        }

    }
}
