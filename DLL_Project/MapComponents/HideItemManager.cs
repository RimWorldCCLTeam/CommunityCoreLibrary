using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class HideItemManager : MapComponent
    {

        private const int                   REHIDE_TICKS = 20;

        private readonly List<Thing>        itemHide = new List<Thing>();
        private readonly List<Thing>        itemShow = new List<Thing>();

        private int                         tickCount = REHIDE_TICKS;

        private static readonly Dictionary<IntVec3,Thing>
                                            hiderBuildings = new Dictionary<IntVec3, Thing>();

        private static List<Thing>          listHasGUIOverlay
        {
            get
            {
                return ThingRequestGroup.HasGUIOverlay.ListByGroup();
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
                        groupList.Add( item );
                    }
                }
                itemShow.Clear();
            }

        }

        public void                         RegisterBuilding( Thing building )
        {
            var occupiedCells = building.OccupiedRect();
            foreach( var cell in occupiedCells )
            {
                hiderBuildings.Add( cell, building );
            }
        }

        public void                         DeregisterBuilding( Thing building )
        {
            var occupiedCells = building.OccupiedRect();
            foreach( var cell in occupiedCells )
            {
                hiderBuildings.Remove( cell );
            }
        }

        public void                         RegisterForHide( Thing item )
        {
            if(
                ( item.def.drawerType != DrawerType.MapMeshOnly )&&
                ( !itemHide.Contains( item ) )
            )
            {
                itemHide.Add( item );
            }
        }

        public void                         RegisterForShow( Thing item )
        {
            if(
                ( item.def.drawerType != DrawerType.MapMeshOnly )&&
                ( !itemShow.Contains( item ) )
            )
            {
                itemShow.Add( item );
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
            var comp = hiderBuilding.TryGetComp<CompHideItem>();
            if( comp == null )
            {
                return false;
            }
            return comp.Properties.preventItemSelection;
        }

    }

}
