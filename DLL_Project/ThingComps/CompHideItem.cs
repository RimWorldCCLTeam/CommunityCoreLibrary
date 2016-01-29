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

        private HideItemManager             _HideItemManager;
        private HideItemManager             HideItemManager
        {
            get
            {
                if( _HideItemManager == null )
                {
                    _HideItemManager = (HideItemManager) Find_Extensions.MapComponent( typeof( HideItemManager ) );
                    if( _HideItemManager == null )
                    {
                        CCL_Log.TraceMod(
                            parent.def,
                            Verbosity.FatalErrors,
                            "MapComponent missing :: HideItemManager"
                        );
                    }
                }
                return _HideItemManager;
            }
        }

        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();
            tickCount = parent.GetHashCode() % RECHECK_TICKS;
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
            if( HideItemManager == null )
            {
                return;
            }
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
            if( HideItemManager == null )
            {
                return;
            }
            if( knownItems.Contains( item ) )
            {
                HideItemManager.RegisterForShow( item );
                knownItems.Remove( item );
            }
        }

    }

}
