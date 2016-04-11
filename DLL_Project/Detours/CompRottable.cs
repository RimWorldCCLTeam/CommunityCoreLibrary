using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _CompRottable
    {

        internal static MethodInfo          _ShouldTakeDessicateDamage;

        #region Reflected Methods

        internal static bool                ShouldTakeDessicateDamage( this CompRottable obj )
        {
            if( _ShouldTakeDessicateDamage == null )
            {
                _ShouldTakeDessicateDamage = typeof( CompRottable ).GetMethod( "ShouldTakeDessicateDamage", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool) _ShouldTakeDessicateDamage.Invoke( obj, null );
        }

        #endregion

        #region Comp Properties & Thing Comps

        internal static CompProperties_Rottable PropsRot( this CompRottable obj )
        {
            return obj.props as CompProperties_Rottable;
        }

        internal static CompRefrigerated    CompRefrigerated( this CompRottable obj )
        {
            IntVec3 checkPos = IntVec3.Invalid;
            if( obj.parent.holder != null )
            {
                checkPos = obj.parent.PositionHeld;
            }
            else
            {
                checkPos = obj.parent.Position;
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

        internal static CompPowerTrader     PowerTraderFor( this CompRefrigerated obj )
        {
            if( obj != null )
            {
                return obj.parent.TryGetComp<CompPowerTrader>();
            }
            return null;
        }

        #endregion

        #region Private Methods

        internal static bool                InRefrigerator( this CompRottable obj )
        {
            var compRefrigerated = obj.CompRefrigerated();
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

        #endregion

        #region Method Detours

        internal static void                _CompTickRare( this CompRottable obj )
        {
            if( obj.InRefrigerator() )
            {
                return;
            }
            float num = obj.rotProgress;
            obj.rotProgress += (float) Mathf.RoundToInt( 1f * GenTemperature.RotRateAtTemperature( GenTemperature.GetTemperatureForCell( obj.parent.PositionHeld ) ) * 250f );
            if(
                ( obj.Stage == RotStage.Rotting )&&
                ( obj.PropsRot().rotDestroys )
            )
            {
                obj.parent.Destroy( DestroyMode.Vanish );
            }
            else
            {
                if( Mathf.FloorToInt( num / 60000f ) == Mathf.FloorToInt( obj.rotProgress / 60000f ) )
                {
                    return;
                }
                if(
                    ( obj.Stage == RotStage.Rotting )&&
                    ( obj.PropsRot().rotDamagePerDay > 0.0f )
                )
                {
                    obj.parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, GenMath.RoundRandom( obj.PropsRot().rotDamagePerDay ), (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null ) );
                }
                else
                {
                    if(
                        ( obj.Stage != RotStage.Dessicated )||
                        ( obj.PropsRot().dessicatedDamagePerDay <= 0.0f )||
                        ( !obj.ShouldTakeDessicateDamage() )
                    )
                    {
                        return;
                    }
                    obj.parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, GenMath.RoundRandom( obj.PropsRot().dessicatedDamagePerDay ), (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null ) );
                }
            }
        }

        internal static string              _CompInspectStringExtra( this CompRottable obj )
        {
            var stringBuilder = new StringBuilder();
            switch( obj.Stage )
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
            if( obj.InRefrigerator() )
            {
                stringBuilder.AppendLine( "RefrigeratedStorage".Translate() );
            }
            else if( ( obj.PropsRot().TicksToRotStart - obj.rotProgress ) > 0.0f )
            {
                float num = GenTemperature.RotRateAtTemperature( Mathf.RoundToInt( GenTemperature.GetTemperatureForCell( obj.parent.Position ) ) );
                int rotAtCurrentTemp = obj.TicksUntilRotAtCurrentTemp;
                if( num < 1.0f / 1000.0f )
                {
                    stringBuilder.AppendLine( "CurrentlyFrozen".Translate() );
                }
                else if( num < 0.999000012874603f )
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
