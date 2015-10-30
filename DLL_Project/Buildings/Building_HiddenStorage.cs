using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Building_HiddenStorage : Building_Storage
    {

        private static List<Thing>          listGroupHasGUIOverlay;

        public static List<Thing>           listHasGUIOverlay
        {
            get
            {
                if( listGroupHasGUIOverlay == null )
                {
                    var listsByGroup = typeof( ListerThings ).GetField( "listsByGroup", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( Find.ListerThings ) as List<Thing>[];
                    listGroupHasGUIOverlay = listsByGroup[ (int)ThingRequestGroup.HasGUIOverlay ];
                }
                return listGroupHasGUIOverlay;
            }
        }

        private List<Thing>                 knownItems = new List<Thing>();

        public override void                Tick()
        {
            if( this.IsHashIntervalTick( 120 ) )
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
