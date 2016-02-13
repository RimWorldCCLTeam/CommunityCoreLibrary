using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _ThingSelectionUtility
    {
        
        internal static bool _SelectableNow( this Thing t )
        {
            if(
                ( !t.def.selectable )||
                ( !t.SpawnedInWorld )||
                ( HideItemManager.PreventItemSelection( t ) )
            )
            {
                return false;
            }
            if(
                ( t.def.size.x == 1 )&&
                ( t.def.size.z == 1 )
            )
            {
                return !GridsUtility.Fogged( t.Position );
            }
            CellRect.CellRectIterator iterator = GenAdj.OccupiedRect( t ).GetIterator();
            while( !iterator.Done() )
            {
                if( !GridsUtility.Fogged( iterator.Current ) )
                {
                    return true;
                }
                iterator.MoveNext();
            }
            return false;
        }

    }

}
