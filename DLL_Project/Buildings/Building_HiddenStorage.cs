using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Building_HiddenStorage : Building_Storage
    {

        private readonly List<Thing>        knownItems = new List<Thing>();

        private static List<Thing>          listHasGUIOverlay
        {
            get
            {
                return ThingRequestGroup.HasGUIOverlay.ListByGroup();
            }
        }

        public void                         RecheckItems()
        {
            var rect = this.OccupiedRect();
            foreach( var cell in rect.Cells )
            {
                foreach( var item in cell.GetThingList() )
                {
                    if(
                        ( item.def.category == ThingCategory.Item )&&
                        ( !knownItems.Contains( item ) )
                    )
                    {
                        Notify_ReceivedThing( item );
                    }
                }
            }
            for( int i = 0; i < knownItems.Count; )
            {
                var item = knownItems[ i ];
                if( !rect.Cells.Contains( item.PositionHeld ) )
                {
                    Notify_LostThing( item );
                }
                else
                {
                    i++;
                }
            }
        }

        public override void                Tick()
        {
            base.Tick();
            if( this.IsHashIntervalTick( 120 ) )
            {
                RecheckItems();
            }
        }

        public override void                TickRare()
        {
            base.TickRare();
            RecheckItems();
        }

        public override void                Notify_ReceivedThing( Thing newItem )
        {
            if( !knownItems.Contains( newItem ) )
            {
                if( newItem.def.drawerType != DrawerType.MapMeshOnly )
                {
                    Find.DynamicDrawManager.DeRegisterDrawable( newItem );
                    if( ThingRequestGroup.HasGUIOverlay.Includes( newItem.def ) )
                    {
                        listHasGUIOverlay.Remove( newItem );
                    }
                }
                knownItems.Add( newItem );
            }
        }

        public override void                Notify_LostThing( Thing newItem )
        {
            if( newItem.def.drawerType != DrawerType.MapMeshOnly )
            {
                Find.DynamicDrawManager.RegisterDrawable( newItem );
                if( ThingRequestGroup.HasGUIOverlay.Includes( newItem.def ) )
                {
                    listHasGUIOverlay.Add( newItem );
                }
            }
            knownItems.Remove( newItem );
        }

    }

}
