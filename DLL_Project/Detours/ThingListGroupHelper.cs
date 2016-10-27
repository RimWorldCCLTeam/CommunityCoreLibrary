using System;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _ThingListGroupHelper
    {

        [DetourClassMethod( typeof( ThingListGroupHelper ), "Includes" )]
        internal static bool _Includes( this ThingRequestGroup group, ThingDef def )
        {
            switch( group )
            {
            case ThingRequestGroup.Undefined:
                return false;
            case ThingRequestGroup.Nothing:
                return false;
            case ThingRequestGroup.Everything:
                return true;
            case ThingRequestGroup.HaulableEver:
                return def.EverHaulable;
            case ThingRequestGroup.HaulableAlways:
                return def.alwaysHaulable;
            case ThingRequestGroup.CultivatedPlant:
                if( def.category == ThingCategory.Plant )
                {
                    return (double) Find.Map.Biome.CommonalityOfPlant( def ) == 0.0;
                }
                return false;
            case ThingRequestGroup.FoodSource:
                if( !def.IsNutritionGivingIngestible )
                {
                    return def.IsFoodDispenser;
                }
                return true;
            case ThingRequestGroup.FoodSourceNotPlantOrTree:
                if(
                    ( !def.IsNutritionGivingIngestible )||
                    ( ( def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree ) == FoodTypeFlags.None )
                )
                {   // Modified behaviour to catch all Nutrient Paste Dispensers
                    // and Automated Factories that are food dispensers
                    return def.IsFoodDispenser;
                }
                return true;
            case ThingRequestGroup.Corpse:
                return def.thingClass == typeof( Corpse );
            case ThingRequestGroup.Blueprint:
                return def.IsBlueprint;
            case ThingRequestGroup.BuildingArtificial:
                if(
                    ( def.category == ThingCategory.Building )||
                    ( def.IsFrame )
                )
                {
                    return
                        (
                            def.building == null
                            ? 0
                            :
                            (
                                def.building.isNaturalRock
                                ? 1
                                :
                                (
                                    def.building.isResourceRock
                                    ?
                                    1 :
                                    0
                                )
                            )
                        ) == 0;
                }
                return false;
            case ThingRequestGroup.BuildingFrame:
                return def.IsFrame;
            case ThingRequestGroup.Pawn:
                return def.category == ThingCategory.Pawn;
            case ThingRequestGroup.PotentialBillGiver:
                return !def.AllRecipes.NullOrEmpty();
            case ThingRequestGroup.Medicine:
                return def.IsMedicine;
            case ThingRequestGroup.Filth:
                return def.filth != null;
            case ThingRequestGroup.AttackTarget:
                return typeof( IAttackTarget ).IsAssignableFrom( def.thingClass );
            case ThingRequestGroup.Weapon:
                return def.IsWeapon;
            case ThingRequestGroup.Refuelable:
                return def.HasComp( typeof( CompRefuelable ) );
            case ThingRequestGroup.HaulableEverOrMinifiable:
                if( !def.EverHaulable )
                {
                    return def.Minifiable;
                }
                return true;
            case ThingRequestGroup.Drug:
                // Catch Automated Factories that can be drug synthesizers
                /* TODO:  Investigate and expand drug system to use factories
                if( def.IsFoodDispenser )
                {
                    var products = def.AllRecipes;
                    if( products.NullOrEmpty() )
                    {
                        return false;
                    }
                    foreach( var product in products )
                    {
                        if( product.products.Any( thingCount => thingCount.thingDef.IsDrug ) )
                        {
                            return true;
                        }
                    }
                    return false;
                }
                */
                return def.IsDrug;
            case ThingRequestGroup.Construction:
                if( !def.IsBlueprint )
                {
                    return def.IsFrame;
                }
                return true;
            case ThingRequestGroup.HasGUIOverlay:
                return def.drawGUIOverlay;
            case ThingRequestGroup.Apparel:
                return def.IsApparel;
            case ThingRequestGroup.MinifiedThing:
                return typeof( MinifiedThing ).IsAssignableFrom( def.thingClass );
            case ThingRequestGroup.Grave:
                return typeof( Building_Grave ).IsAssignableFrom( def.thingClass );
            case ThingRequestGroup.Art:
                return def.HasComp( typeof( CompArt ) );
            case ThingRequestGroup.Container:
                return typeof( IThingContainerOwner ).IsAssignableFrom( def.thingClass );
            case ThingRequestGroup.DropPod:
                if( !typeof( DropPod ).IsAssignableFrom( def.thingClass ) )
                {
                    return typeof( DropPodIncoming ).IsAssignableFrom( def.thingClass );
                }
                return true;
            default:
                throw new ArgumentException("group");
            }
        }

    }

}
