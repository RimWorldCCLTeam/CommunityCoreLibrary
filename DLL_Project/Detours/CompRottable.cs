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
                if( !checkPos.InBounds( parent.Map ) )
                {
                    return null;
                }
                var refrigerator = checkPos.GetThingList( parent.Map ).FirstOrDefault( t => (
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
        
        [DetourMember]
        internal void                       _CompTickRare()
        {
            if( this.IsInRefrigerator ) // changed
            {
                return;
            }
            float num = this.RotProgress;
            this.RotProgress += (float) Mathf.RoundToInt( 1f * GenTemperature.RotRateAtTemperature(
                GenTemperature.GetTemperatureForCell( this.parent.PositionHeld, this.parent.Map ) ) * 250f );
            if(
                ( this.Stage == RotStage.Rotting )&&
                ( this.PropsRot.rotDestroys )
            )
            {
                if ( this.parent.Map.slotGroupManager.SlotGroupAt( this.parent.Position ) != null )
                {
                    Messages.Message( "MessageRottedAwayInStorage".Translate( new object[]
                    {
                        this.parent.Label
                    } ).CapitalizeFirst(), MessageSound.Silent );
                    LessonAutoActivator.TeachOpportunity( ConceptDefOf.SpoilageAndFreezers, OpportunityType.GoodToKnow );
                }
                this.parent.Destroy( DestroyMode.Vanish );
                return;
            }
            // changed (made TicksPerDay configurable)
            if( Mathf.FloorToInt( num / (float) GenDate.TicksPerDay ) == Mathf.FloorToInt( this.RotProgress / (float) GenDate.TicksPerDay ) )
            {
                return;
            }
            if(
                ( this.Stage == RotStage.Rotting )&&
                ( this.PropsRot.rotDamagePerDay > 0.0f )
            )
            {
                this.parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, GenMath.RoundRandom( this.PropsRot.rotDamagePerDay ) ) );
                return;
            }
            if(
                ( this.Stage == RotStage.Dessicated )&&
                ( this.PropsRot.dessicatedDamagePerDay > 0.0f )&&
                ( this.ShouldTakeDessicateDamage() )
            )
            {
                this.parent.TakeDamage( new DamageInfo( DamageDefOf.Rotting, GenMath.RoundRandom( this.PropsRot.dessicatedDamagePerDay ) ) );
                return;
            }
        }

        [DetourMember]
        internal string                     _CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            switch( this.Stage )
            {
            case RotStage.Fresh:
                stringBuilder.Append( "RotStateFresh".Translate() + "." );
                    break;
            case RotStage.Rotting:
                stringBuilder.Append( "RotStateRotting".Translate() + "." );
                    break;
            case RotStage.Dessicated:
                stringBuilder.Append( "RotStateDessicated".Translate() + "." );
                    break;
            }
            if( this.IsInRefrigerator ) // changed
            {
                // TODO: check that these strings output as expected
                stringBuilder.Append( "RefrigeratedStorage".Translate() + "." );
            }
            else if( ( this.PropsRot.TicksToRotStart - this.RotProgress ) > 0.0f )
            {
                float num = GenTemperature.RotRateAtTemperature( Mathf.RoundToInt(
                    GenTemperature.GetTemperatureForCell( this.parent.PositionHeld, this.parent.Map ) ) );
                int rotAtCurrentTemp = this.TicksUntilRotAtCurrentTemp;
                stringBuilder.AppendLine();
                if ( num < 1.0f / 1000.0f )
                {
                    stringBuilder.Append( "CurrentlyFrozen".Translate() + "." );
                }
                else if( num < 0.999000012874603f )
                {
                    stringBuilder.Append( "CurrentlyRefrigerated".Translate( rotAtCurrentTemp.ToStringTicksToPeriodVagueMax() ) + "." );
                }
                else
                {
                    stringBuilder.Append( "NotRefrigerated".Translate( rotAtCurrentTemp.ToStringTicksToPeriodVagueMax() ) + "." );
                }
            }
            return stringBuilder.ToString();
        }

        #endregion

    }

}
