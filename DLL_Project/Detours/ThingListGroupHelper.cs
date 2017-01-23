#if DEVELOPER
// Enable this define for a whole bunch of debug logging
//#define _I_AM_A_POTATO_
// Enable this define to target only a specific def, see MonitorForDef below
#define _I_AM_WATCHING_YOU_
#endif

using System;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

using CommunityCoreLibrary;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _ThingListGroupHelper
    {

#if _I_AM_A_POTATO_
#if _I_AM_WATCHING_YOU_
        private const string                MonitorForDef = "Synthesizer";
#endif
#endif
        
        [DetourMember( typeof( ThingListGroupHelper ) )]
        internal static bool                _Includes( this ThingRequestGroup group, ThingDef def )
        {
#if _I_AM_A_POTATO_
            var rVal = _IncludesInt( group, def );
#if _I_AM_WATCHING_YOU_
            if( def.defName == MonitorForDef )
            {
#endif
                CCL_Log.Trace(
                    Verbosity.Default,
                    string.Format( "Group '{0}' includes Def '{1}' = '{2}'", group, def.defName, rVal ),
                    "Detour.ThingListGroupHelper.Includes"
                );
#if _I_AM_WATCHING_YOU_
            }
#endif
            return rVal;
        }

        internal static bool                _IncludesInt( ThingRequestGroup group, ThingDef def )
        {
#endif
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
                    
            case ThingRequestGroup.Plant:
                return def.category == ThingCategory.Plant;
                
            case ThingRequestGroup.FoodSource:
                return def.IsNutritionGivingIngestible || def.IsFoodDispenser;
                
            case ThingRequestGroup.FoodSourceNotPlantOrTree:
                return ( def.IsNutritionGivingIngestible &&
                         ( def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree ) == FoodTypeFlags.None ||
                         def.IsFoodDispenser );
                
            case ThingRequestGroup.Corpse:
                return def.thingClass == typeof( Corpse );
                
            case ThingRequestGroup.Blueprint:
                return def.IsBlueprint;
                
            case ThingRequestGroup.BuildingArtificial:
                return ( ( def.category == ThingCategory.Building || def.IsFrame )&&
                         ( def.building == null || ( !def.building.isNaturalRock && !def.building.isResourceRock ) )
                       );
                
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
                return def.EverHaulable || def.Minifiable;

            case ThingRequestGroup.Drug:
                // Catch Automated Factories that can be drug synthesizers
                if(
                    ( typeof( Building_AutomatedFactory ).IsAssignableFrom( def.thingClass ) )&&
                    ( Building_AutomatedFactory.DefOutputToPawnsDirectly( def ) )
                )
                {   // Since nutrient paste dispensers are also food dispensers, we need to check this way instead of IsFoodDispenser
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
                return def.IsDrug;
                
            case ThingRequestGroup.Construction:
                return def.IsBlueprint || def.IsFrame;
                
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
                
            case ThingRequestGroup.ContainerEnclosure:
                return typeof( IThingContainerOwner ).IsAssignableFrom( def.thingClass ) && !def.IsCorpse;

            case ThingRequestGroup.ActiveDropPod:
                return typeof( IActiveDropPod ).IsAssignableFrom( def.thingClass );

            case ThingRequestGroup.Transporter:
                return def.HasComp( typeof( CompTransporter ) );

            default:
                throw new ArgumentException( "group" );
            }
        }

    }

}
