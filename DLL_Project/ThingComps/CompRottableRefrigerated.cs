using System.Linq;
using System.Text;
using UnityEngine;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompRottableRefrigerated : CompRottable
    {
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
                if( parent.Position.InBounds() )
                {
                    checkPos = parent.Position;
                }
                else if( parent.PositionHeld.InBounds() )
                {
                    checkPos = parent.PositionHeld;
                }
                if( checkPos == IntVec3.Invalid )
                {
                    return null;
                }
                var refrigerator = parent.Position.GetThingList().FirstOrDefault( t => (
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

        public override void                CompTickRare()
        {
            if( InRefrigerator )
            {
                return;
            }
            float num = rotProgress;
            rotProgress += (float) Mathf.RoundToInt( 1f * GenTemperature.RotRateAtTemperature( GenTemperature.GetTemperatureForCell( parent.Position ) ) * 250f );
            if(
                ( Stage == RotStage.Rotting )&&
                ( PropsRot.rotDestroys )
            )
            {
                parent.Destroy( DestroyMode.Vanish );
            }
            else
            {
                if(
                    ( Stage != RotStage.Rotting )||
                    ( PropsRot.rotDamagePerDay <= 0 )||
                    ( Mathf.FloorToInt( num / 30000f ) == Mathf.FloorToInt( rotProgress / 30000f ) )
                )
                {
                    return;
                }
                parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, PropsRot.rotDamagePerDay, (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null ) );
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
                    stringBuilder.AppendLine( "CurrentlyRefrigerated".Translate( rotAtCurrentTemp.TicksToDaysExtendedString() ) );
                }
                else
                {
                    stringBuilder.AppendLine( "NotRefrigerated".Translate( rotAtCurrentTemp.TicksToDaysExtendedString() ) );
                }
            }
            return stringBuilder.ToString();
        }

    }

}
