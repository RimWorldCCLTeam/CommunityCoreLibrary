using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _ThingSelectionUtility
    {
        
        [DetourClassMethod( typeof( ThingSelectionUtility ), "SelectableNow" )]
        internal static bool _SelectableNow( this Thing t )
        {
            // If it's not selectable,
            // not spawned, or;
            // the thing is registered with the Hide Item Manager
            if(
                ( !t.def.selectable )||
                ( !t.Spawned )||
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
                return !t.Position.Fogged();
            }
            foreach( var cell in t.OccupiedRect() )
            {
                if( !cell.Fogged() )
                {
                    return true;
                }
            }
            return false;
        }

    }

}
