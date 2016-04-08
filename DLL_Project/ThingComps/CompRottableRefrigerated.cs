using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class CompRottableRefrigerated : CompRottable
    {

        private MethodInfo                  _ShouldTakeDessicateDamage;

        #region Comp Properties & Thing Comps

        private CompProperties_Rottable     PropsRot
        {
            get
            {
                return props as CompProperties_Rottable;
            }
        }

        private CompRefrigerated            CompRefrigerated
        {
            get
            {
                IntVec3 checkPos = IntVec3.Invalid;
                if( parent.holder != null )
                {
                    checkPos = parent.PositionHeld;
                }
                else
                {
                    checkPos = parent.Position;
                }
                if(
                    ( checkPos == IntVec3.Invalid )||
                    ( !checkPos.InBounds() )
                )
                {
                    return null;
                }
                var refrigerator = checkPos.GetThingList().FirstOrDefault( t => (
                    ( t.TryGetComp<CompRefrigerated>() != null )
                ) );
                return refrigerator?.TryGetComp<CompRefrigerated>();
            }
        }

        private CompPowerTrader             PowerTraderFor( CompRefrigerated refrigerator )
        {
            if( refrigerator != null )
            {
                return refrigerator.parent.TryGetComp<CompPowerTrader>();
            }
            return null;
        }

        #endregion

        #region Private Methods

        private bool                        InRefrigerator
        {
            get
            {
                var compRefrigerated = CompRefrigerated;
                var compPowerTrader = PowerTraderFor( compRefrigerated );
                return (
                    ( compRefrigerated != null )&&
                    (
                        ( compPowerTrader == null )||
                        (
                            ( compPowerTrader != null )&&
                            ( compPowerTrader.PowerOn )
                        )
                    )
                );
            }
        }

        private bool                         ShouldTakeDessicateDamage()
        {
            if( _ShouldTakeDessicateDamage == null )
            {
                _ShouldTakeDessicateDamage = typeof( CompRottable ).GetMethod( "ShouldTakeDessicateDamage", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool) _ShouldTakeDessicateDamage.Invoke( this, null );
        }

        #endregion

        #region Base Overrides

        public override void                CompTickRare()
        {
            if( InRefrigerator )
            {
                return;
            }
            float num = this.rotProgress;
            this.rotProgress += (float) Mathf.RoundToInt(1f * GenTemperature.RotRateAtTemperature(GenTemperature.GetTemperatureForCell(this.parent.PositionHeld)) * 250f);
            if (this.Stage == RotStage.Rotting && this.PropsRot.rotDestroys)
            {
                this.parent.Destroy(DestroyMode.Vanish);
            }
            else
            {
                if (Mathf.FloorToInt(num / 60000f) == Mathf.FloorToInt(this.rotProgress / 60000f))
                    return;
                if (this.Stage == RotStage.Rotting && (double) this.PropsRot.rotDamagePerDay > 0.0)
                {
                    this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(this.PropsRot.rotDamagePerDay), (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null));
                }
                else
                {
                    if (this.Stage != RotStage.Dessicated || (double) this.PropsRot.dessicatedDamagePerDay <= 0.0 || !this.ShouldTakeDessicateDamage())
                        return;
                    this.parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, GenMath.RoundRandom(this.PropsRot.dessicatedDamagePerDay), (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null));
                }
            }
        }

        public override string              CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            switch( Stage )
            {
            case RotStage.Fresh:
                stringBuilder.AppendLine( "RotStateFresh".Translate() );
                break;
            case RotStage.Rotting:
                stringBuilder.AppendLine( "RotStateRotting".Translate() );
                break;
            case RotStage.Dessicated:
                stringBuilder.AppendLine( "RotStateDessicated".Translate() );
                break;
            }
            if( InRefrigerator )
            {
                stringBuilder.AppendLine( "RefrigeratedStorage".Translate() );
            }
            else if( (double) ((float) PropsRot.TicksToRotStart - rotProgress) > 0.0 )
            {
                float num = GenTemperature.RotRateAtTemperature( (float) Mathf.RoundToInt( GenTemperature.GetTemperatureForCell( parent.Position ) ) );
                int rotAtCurrentTemp = TicksUntilRotAtCurrentTemp;
                if( (double) num < 1.0 / 1000.0 )
                {
                    stringBuilder.AppendLine( "CurrentlyFrozen".Translate() );
                }
                else if( (double) num < 0.999000012874603 )
                {
                    stringBuilder.AppendLine( "CurrentlyRefrigerated".Translate( rotAtCurrentTemp.TickstoDaysString() ) );
                }
                else
                {
                    stringBuilder.AppendLine( "NotRefrigerated".Translate( rotAtCurrentTemp.TickstoDaysString() ) );
                }
            }
            return stringBuilder.ToString();
        }

        #endregion

    }

}
