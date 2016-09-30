using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal class _CompRottable : CompRottable
    {

        internal static MethodInfo          _ShouldTakeDessicateDamage;

        static                              _CompRottable()
        {
            _ShouldTakeDessicateDamage = typeof( CompRottable ).GetMethod( "ShouldTakeDessicateDamage", Controller.Data.UniversalBindingFlags );
            if( _ShouldTakeDessicateDamage == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ShouldTakeDessicateDamage' in 'CompRottable'",
                    "Detour.CompRottable" );
            }
        }

        #region Reflected Methods

        internal bool                       ShouldTakeDessicateDamage()
        {
            return (bool) _ShouldTakeDessicateDamage.Invoke( this, null );
        }

        #endregion

        #region Comp Properties & Thing Comps

        internal CompProperties_Rottable    PropsRot
        {
            get
            {
                return this.props as CompProperties_Rottable;
            }
        }

        internal CompRefrigerated           refrigeratedComp
        {
            get
            {
                var checkPos = this.parent.PositionHeld;
                if( !checkPos.InBounds() )
                {
                    return null;
                }
                var refrigerator = checkPos.GetThingList().FirstOrDefault( t => (
                    ( t.TryGetComp<CompRefrigerated>() != null )
                ) );
                return refrigerator?.TryGetComp<CompRefrigerated>();
            }
        }

        internal CompPowerTrader            compPowerFor( CompRefrigerated compRefrigerated )
        {
            if( compRefrigerated != null )
            {
                return compRefrigerated.parent.TryGetComp<CompPowerTrader>();
            }
            return null;
        }

        #endregion

        #region Private Methods

        internal bool                       IsInRefrigerator
        {
            get
            {
                var compRefrigerated = this.refrigeratedComp;
                var compPowerTrader = compPowerFor( compRefrigerated );
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

        #endregion

        #region Method Detours

        [DetourClassMethod( typeof( CompRottable ), "CompTickRare" )]
        public override void                CompTickRare()
        {
            if( this.IsInRefrigerator )
            {
                return;
            }
            float num = this.RotProgress;
            this.RotProgress += (float) Mathf.RoundToInt( 1f * GenTemperature.RotRateAtTemperature( GenTemperature.GetTemperatureForCell( this.parent.PositionHeld ) ) * 250f );
            if(
                ( this.Stage == RotStage.Rotting )&&
                ( this.PropsRot.rotDestroys )
            )
            {
                this.parent.Destroy( DestroyMode.Vanish );
            }
            else
            {
                if( Mathf.FloorToInt( num / (float) GenDate.TicksPerDay ) == Mathf.FloorToInt( this.RotProgress / (float) GenDate.TicksPerDay ) )
                {
                    return;
                }
                if(
                    ( this.Stage == RotStage.Rotting )&&
                    ( this.PropsRot.rotDamagePerDay > 0.0f )
                )
                {
                    this.parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, GenMath.RoundRandom( this.PropsRot.rotDamagePerDay ), (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null ) );
                }
                else
                {
                    if(
                        ( this.Stage != RotStage.Dessicated )||
                        ( this.PropsRot.dessicatedDamagePerDay <= 0.0f )||
                        ( !this.ShouldTakeDessicateDamage() )
                    )
                    {
                        return;
                    }
                    this.parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, GenMath.RoundRandom( this.PropsRot.dessicatedDamagePerDay ), (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null ) );
                }
            }
        }

        [DetourClassMethod( typeof( CompRottable ), "CompInspectStringExtra" )]
        public override string              CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            switch( this.Stage )
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
            if( this.IsInRefrigerator )
            {
                stringBuilder.AppendLine( "RefrigeratedStorage".Translate() );
            }
            else if( ( this.PropsRot.TicksToRotStart - this.RotProgress ) > 0.0f )
            {
                float num = GenTemperature.RotRateAtTemperature( Mathf.RoundToInt( GenTemperature.GetTemperatureForCell( this.parent.PositionHeld ) ) );
                int rotAtCurrentTemp = this.TicksUntilRotAtCurrentTemp;
                if( num < 1.0f / 1000.0f )
                {
                    stringBuilder.AppendLine( "CurrentlyFrozen".Translate() );
                }
                else if( num < 0.999000012874603f )
                {
                    stringBuilder.AppendLine( "CurrentlyRefrigerated".Translate( rotAtCurrentTemp.ToStringTicksToPeriodVagueMax() ) );
                }
                else
                {
                    stringBuilder.AppendLine( "NotRefrigerated".Translate( rotAtCurrentTemp.ToStringTicksToPeriodVagueMax() ) );
                }
            }
            return stringBuilder.ToString();
        }

        #endregion

    }

}
