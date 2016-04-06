using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _Building_NutrientPasteDispenser
    {
        
        internal static List<IntVec3> _AdjCellsCardinalInBounds( Building_NutrientPasteDispenser obj )
        {
            var __AdjCellsCardinalInBounds = typeof( Building_NutrientPasteDispenser ).GetProperty( "AdjCellsCardinalInBounds", BindingFlags.Instance | BindingFlags.NonPublic );
            return __AdjCellsCardinalInBounds.GetValue( obj, null ) as List<IntVec3>;
        }

        internal static Building _AdjacentReachableHopper( this Building_NutrientPasteDispenser obj, Pawn reacher )
        {
            if( Controller.Data.GenericHoppersEnabled )
            {
                var CompHopperUser = obj.TryGetComp<CompHopperUser>();
                if( CompHopperUser != null )
                {
                    var hoppers = CompHopperUser.FindHoppers();
                    if( !hoppers.NullOrEmpty() )
                    {
                        foreach( var hopper in hoppers )
                        {
                            if(
                                reacher.CanReach(
                                    ( TargetInfo )( ( Thing )hopper.parent ),
                                    PathEndMode.Touch,
                                    reacher.NormalMaxDanger(),
                                    false )
                            )
                            {
                                return (Building) hopper.parent;
                            }
                        }
                    }
                }
            }
            if( !Controller.Data.VanillaHoppersDisabled )
            {
                var adjCells = _AdjCellsCardinalInBounds( obj );
                for( int index = 0; index < adjCells.Count; ++index )
                {
                    Building edifice = adjCells[ index ].GetEdifice();
                    if(
                        ( edifice != null )&&
                        ( edifice.def == ThingDefOf.Hopper )&&
                        ( reacher.CanReach(
                            ( TargetInfo )( ( Thing )edifice ),
                            PathEndMode.Touch,
                            reacher.NormalMaxDanger(),
                            false ) )
                    )
                    {
                        return edifice;
                    }
                }
            }
            return (Building) null;
        }

        internal static Thing _FindFeedInAnyHopper( this Building_NutrientPasteDispenser obj )
        {
            if( Controller.Data.GenericHoppersEnabled )
            {
                var CompHopperUser = obj.TryGetComp<CompHopperUser>();
                if( CompHopperUser != null )
                {
                    var hoppers = CompHopperUser.FindHoppers();
                    if( !hoppers.NullOrEmpty() )
                    {
                        foreach( var hopper in hoppers )
                        {
                            var resources = hopper.GetAllResources( CompHopperUser.Resources );
                            if( !resources.NullOrEmpty() )
                            {
                                return resources.FirstOrDefault();
                            }
                        }
                    }
                }
            }
            if( !Controller.Data.VanillaHoppersDisabled )
            {
                var adjCells = _AdjCellsCardinalInBounds( obj );
                for( int cellIndex = 0; cellIndex < adjCells.Count; ++cellIndex )
                {
                    IntVec3 c = adjCells[ cellIndex ];
                    Thing resource = (Thing) null;
                    Thing hopper = (Thing) null;
                    List<Thing> thingList = GridsUtility.GetThingList( c );
                    for( int thingIndex = 0; thingIndex < thingList.Count; ++thingIndex )
                    {
                        Thing thisThing = thingList[ thingIndex ];
                        if( Building_NutrientPasteDispenser.IsAcceptableFeedstock( thisThing.def ) )
                        {
                            resource = thisThing;
                        }
                        if( thisThing.def == ThingDefOf.Hopper )
                        {
                            hopper = thisThing;
                        }
                    }
                    if(
                        ( resource != null )&&
                        ( hopper != null )
                    )
                    {
                        return resource;
                    }
                }
            }
            return (Thing) null;
        }

        // TODO: see other todos
        /*internal static bool _HasEnoughFeedstockInHoppers( this Building_NutrientPasteDispenser obj )
        {
            int costPerDispense = obj.def.building.foodCostPerDispense;
            if( ResearchProjectDef.Named( "NutrientResynthesis" ).IsFinished )
            {
                costPerDispense--;
            }
            if( Controller.Data.GenericHoppersEnabled )
            {
                var CompHopperUser = obj.TryGetComp<CompHopperUser>();
                if( CompHopperUser != null )
                {
                    if( CompHopperUser.EnoughResourcesInHoppers( costPerDispense ) )
                    {
                        return true;
                    }
                }
            }
            if( !Controller.Data.VanillaHoppersDisabled )
            {
                var adjCells = _AdjCellsCardinalInBounds( obj );
                int resourceCount = 0;
                for( int cellIndex = 0; cellIndex < adjCells.Count; ++cellIndex )
                {
                    IntVec3 c = adjCells[ cellIndex ];
                    Thing resource = (Thing) null;
                    Thing hopper = (Thing) null;
                    List<Thing> thingList = GridsUtility.GetThingList( c );
                    for( int thingIndex = 0; thingIndex < thingList.Count; ++thingIndex )
                    {
                        Thing thisThing = thingList[ thingIndex ];
                        if( Building_NutrientPasteDispenser.IsAcceptableFeedstock( thisThing.def ) )
                        {
                            resource = thisThing;
                        }
                        if( thisThing.def == ThingDefOf.Hopper )
                        {
                            hopper = thisThing;
                        }
                    }
                    if(
                        ( resource != null )&&
                        ( hopper != null )
                    )
                    {
                        resourceCount += resource.stackCount;
                    }
                    if( resourceCount >= costPerDispense )
                    {
                        return true;
                    }
                }
            }
            return false;
        }*/

    }

}
