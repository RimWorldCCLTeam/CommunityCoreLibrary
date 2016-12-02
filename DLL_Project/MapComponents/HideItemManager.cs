using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    [StaticConstructorOnStartup]
    public class HideItemManager : MapComponent
    {

        private const int                   REHIDE_TICKS = 20;

        private static int                  tickCount = REHIDE_TICKS;

        private static List<Thing>          itemHide;
        private static List<Thing>          itemShow;

        private static Dictionary<IntVec3,Thing> hiderBuildings;

        static HideItemManager()
        {
            itemHide = new List<Thing>();
            itemShow = new List<Thing>();
            hiderBuildings = new Dictionary<IntVec3, Thing>();
        }

        private static List<Thing>          listHasGUIOverlay
        {
            get
            {
                return ThingRequestGroup.HasGUIOverlay.ListOfThingsByGroup();
            }
        }

        public override void                MapComponentTick()
        {
            base.MapComponentTick();

            // Only do this periodically
            tickCount++;
            if( tickCount < REHIDE_TICKS )
            {
                return;
            }
            tickCount = 0;

            // Cache once per itteration (saves cycles due to reflection and function calls)
            var groupList = listHasGUIOverlay;

            // Hide registered items
            if( !itemHide.NullOrEmpty() )
            {
                foreach( var item in itemHide )
                {
                    Find.DynamicDrawManager.DeRegisterDrawable( item );
                    if( ThingRequestGroup.HasGUIOverlay.Includes( item.def ) )
                    {
                        groupList.Remove( item );
                    }
                }
            }

            // Show registered items
            if( !itemShow.NullOrEmpty() )
            {
                foreach( var item in itemShow )
                {
                    Find.DynamicDrawManager.RegisterDrawable( item );
                    if( ThingRequestGroup.HasGUIOverlay.Includes( item.def ) )
                    {
                        groupList.AddUnique( item );
                    }
                }
                itemShow.Clear();
            }

        }

        public static void                  RegisterBuilding( Thing building )
        {
            var occupiedCells = building.OccupiedRect();
            foreach( var cell in occupiedCells )
            {
                hiderBuildings.Add( cell, building );
            }
        }

        public static void                  DeregisterBuilding( Thing building )
        {
            var occupiedCells = building.OccupiedRect();
            foreach( var cell in occupiedCells )
            {
                hiderBuildings.Remove( cell );
            }
        }

        public static void                  RegisterForHide( Thing item )
        {
            if( item.def.drawerType != DrawerType.MapMeshOnly )
            {
                itemHide.AddUnique( item );
            }
        }

        public static void                  RegisterForShow( Thing item )
        {
            if( item.def.drawerType != DrawerType.MapMeshOnly )
            {
                itemShow.AddUnique( item );
                if( itemHide.Contains( item ) )
                {
                    itemHide.Remove( item );
                }
            }
        }

        public static bool                  PreventItemSelection( Thing item )
        {
            Thing hiderBuilding;
            if( !hiderBuildings.TryGetValue( item.Position, out hiderBuilding ) )
            {
                return false;
            }
            if( item == hiderBuilding )
            {   // Don't prevent the building from being selected
                return false;
            }
            var comp = hiderBuilding.TryGetComp<CompHideItem>();
            if( comp == null )
            {
                return false;
            }
            return comp.Properties.preventItemSelection;
        }

    }

}
