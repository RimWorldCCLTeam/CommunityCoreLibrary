using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal class _WorkGiver_DoBill : WorkGiver_DoBill
    {

        internal static FieldInfo           _ReCheckFailedBillTicksRange;
        internal static FieldInfo           _MissingSkillTranslated;
        internal static FieldInfo           _chosenIngThings;
        internal static FieldInfo           _MissingMaterialsTranslated;

        internal static MethodInfo          _FinishUftJob;
        internal static MethodInfo          _ClosestUnfinishedThingForBill;
        internal static MethodInfo          _TryFindBestBillIngredients;
        internal static MethodInfo          _TryStartNewDoBillJob;

        static                              _WorkGiver_DoBill()
        {
            _ReCheckFailedBillTicksRange = typeof( WorkGiver_DoBill ).GetField( "ReCheckFailedBillTicksRange", Controller.Data.UniversalBindingFlags );
            if( _ReCheckFailedBillTicksRange == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ReCheckFailedBillTicksRange' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _MissingSkillTranslated = typeof( WorkGiver_DoBill ).GetField( "MissingSkillTranslated", Controller.Data.UniversalBindingFlags );
            if( _MissingSkillTranslated == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'MissingSkillTranslated' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _chosenIngThings = typeof( WorkGiver_DoBill ).GetField( "chosenIngThings", Controller.Data.UniversalBindingFlags );
            if( _chosenIngThings == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'chosenIngThings' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _MissingMaterialsTranslated = typeof( WorkGiver_DoBill ).GetField( "MissingMaterialsTranslated", Controller.Data.UniversalBindingFlags );
            if( _MissingMaterialsTranslated == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'MissingMaterialsTranslated' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _FinishUftJob = typeof( WorkGiver_DoBill ).GetMethod( "FinishUftJob", Controller.Data.UniversalBindingFlags );
            if( _FinishUftJob == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'FinishUftJob' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _ClosestUnfinishedThingForBill = typeof( WorkGiver_DoBill ).GetMethod( "ClosestUnfinishedThingForBill", Controller.Data.UniversalBindingFlags );
            if( _ClosestUnfinishedThingForBill == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'ClosestUnfinishedThingForBill' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _TryFindBestBillIngredients = typeof( WorkGiver_DoBill ).GetMethod( "TryFindBestBillIngredients", Controller.Data.UniversalBindingFlags );
            if( _TryFindBestBillIngredients == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'TryFindBestBillIngredients' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
            _TryStartNewDoBillJob = typeof( WorkGiver_DoBill ).GetMethod( "TryStartNewDoBillJob", Controller.Data.UniversalBindingFlags );
            if( _TryStartNewDoBillJob == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'TryStartNewDoBillJob' in 'WorkGiver_DoBill'",
                    "Detour.WorkGiver_DoBill" );
            }
        }

        #region Reflected Methods

        internal static IntRange            ReCheckFailedBillTicksRange
        {
            get
            {
                return (IntRange) _ReCheckFailedBillTicksRange.GetValue( null );
            }
        }

        internal static string              MissingSkillTranslated
        {
            get
            {
                return (string) _MissingSkillTranslated.GetValue( null );
            }
        }

        internal List<ThingAmount>          chosenIngThings
        {
            get
            {
                return (List<ThingAmount>) _chosenIngThings.GetValue( this );
            }
        }

        internal static string              MissingMaterialsTranslated
        {
            get
            {
                return (string) _MissingMaterialsTranslated.GetValue( null );
            }
        }

        internal static Job                 FinishUftJob( Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill )
        {
            return (Job) _FinishUftJob.Invoke( null, new object[] { pawn, uft, bill } );
        }

        internal static UnfinishedThing     ClosestUnfinishedThingForBill( Pawn pawn, Bill_ProductionWithUft bill )
        {
            return (UnfinishedThing) _ClosestUnfinishedThingForBill.Invoke( null, new object[] { pawn, bill } );
        }

        internal static bool                TryFindBestBillIngredients( Bill bill, Pawn pawn, Thing billGiver, List<ThingAmount> chosen )
        {
            return (bool) _TryFindBestBillIngredients.Invoke( null, new object[] { bill, pawn, billGiver, chosen } );
        }

        internal Job                        TryStartNewDoBillJob( Pawn pawn, Bill bill, IBillGiver giver )
        {
            return (Job) _TryStartNewDoBillJob.Invoke( this, new object[] { pawn, bill, giver } );
        }

        #endregion

        #region Detoured Methods

        [DetourMember]
        internal Job                        _StartOrResumeBillJob( Pawn pawn, IBillGiver giver )
        {
            var giverThing = giver as Thing;
            var giverThingWithComps = giverThing as ThingWithComps;
            for( int index = 0; index < giver.BillStack.Count; ++index )
            {
                var billBase = giver.BillStack[ index ];
                if ( billBase.recipe.requiredGiverWorkType != null && billBase.recipe.requiredGiverWorkType != this.def.workType )
                {
                    continue;
                }

                var advancedRecipe = billBase.recipe as AdvancedRecipeDef;
                if(
                    (
                        ( billBase.recipe.requiredGiverWorkType == null )||
                        ( billBase.recipe.requiredGiverWorkType == this.def.workType )
                    )&&
                    (
                        ( Find.TickManager.TicksGame >= billBase.lastIngredientSearchFailTicks + ReCheckFailedBillTicksRange.RandomInRange )||
                        ( FloatMenuMakerMap.making )
                    )&&
                    (
                        ( advancedRecipe == null )||
                        ( advancedRecipe.requiredFacilities.NullOrEmpty() )||
                        (
                            ( giverThingWithComps != null )&&
                            ( giverThingWithComps.HasConnectedFacilities( advancedRecipe.requiredFacilities ) )
                        )
                    )&&
                    ( billBase.ShouldDoNow() )&&
                    ( billBase.PawnAllowedToStartAnew( pawn ) )
                )
                {
                    if( !billBase.recipe.PawnSatisfiesSkillRequirements( pawn ) )
                    {
                        JobFailReason.Is( MissingSkillTranslated );
                    }
                    else
                    {
                        var billUft = billBase as Bill_ProductionWithUft;
                        if( billUft != null )
                        {
                            if( billUft.BoundUft != null )
                            {
                                if(
                                    ( billUft.BoundWorker == pawn )&&
                                    ( pawn.CanReserveAndReach( billUft.BoundUft, PathEndMode.Touch, Danger.Deadly, 1 ) )&&
                                    ( !billUft.BoundUft.IsForbidden( pawn ) )
                                )
                                {
                                    return FinishUftJob( pawn, billUft.BoundUft, billUft );
                                }
                                continue;
                            }
                            var uft = ClosestUnfinishedThingForBill( pawn, billUft );
                            if( uft != null )
                            {
                                return FinishUftJob( pawn, uft, billUft );
                            }
                        }
                        if( TryFindBestBillIngredients( billBase, pawn, giverThing, chosenIngThings ) )
                        {
                            return TryStartNewDoBillJob( pawn, billBase, giver );
                        }
                        if( !FloatMenuMakerMap.making )
                        {
                            billBase.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                        }
                        else
                        {
                            JobFailReason.Is( MissingMaterialsTranslated );
                        }
                    }
                }
            }
            return (Job) null;
        }

        #endregion

    }

}

