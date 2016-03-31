using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompHideItem : ThingComp
    {

        private const int                   RECHECK_TICKS = 20;

        private readonly List<Thing>        knownItems = new List<Thing>();
        private int                         tickCount;

        private CompProperties_HideItem      _Properties = null;
        public CompProperties_HideItem      Properties
        {
            get
            {
                if( _Properties == null )
                {
                    _Properties = parent.def.GetCompProperties( typeof( CompHideItem ) ) as CompProperties_HideItem;
                }
                return _Properties;
            }
        }

        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();
            tickCount = parent.GetHashCode() % RECHECK_TICKS;
            HideItemManager.RegisterBuilding( parent );
        }

        public override void                PostDestroy( DestroyMode mode = DestroyMode.Vanish )
        {
            base.PostDestroy( mode );
            if( knownItems.NullOrEmpty() )
            {
                return;
            }
            foreach( var item in knownItems )
            {
                HideItemManager.RegisterForShow( item );
            }
            HideItemManager.DeregisterBuilding( parent );
        }

        public override void                PostDraw()
        {
            base.PostDraw();

            // Only do this periodically
            tickCount++;
            if( tickCount < RECHECK_TICKS )
            {
                return;
            }
            tickCount = 0;

            // Scan for new items
            var rect = parent.OccupiedRect();
            foreach( var cell in rect.Cells )
            {
                foreach( var item in cell.GetThingList() )
                {
                    ReceivedThing( item );
                }
            }

            // Scan for removed items
            for( int i = knownItems.Count - 1; i >= 0; i-- )
            {
                var item = knownItems[ i ];
                if( !rect.Cells.Contains( item.Position ) )
                {
                    LostThing( item );
                }
            }
        }

        public void                         ReceivedThing( Thing item )
        {
            if(
                ( item.def.category == ThingCategory.Item )&&
                ( !knownItems.Contains( item ) )
            )
            {
                HideItemManager.RegisterForHide( item );
                knownItems.Add( item );
            }
        }

        public void                         LostThing( Thing item )
        {
            if( knownItems.Contains( item ) )
            {
                HideItemManager.RegisterForShow( item );
                knownItems.Remove( item );
            }
        }

    }

}
