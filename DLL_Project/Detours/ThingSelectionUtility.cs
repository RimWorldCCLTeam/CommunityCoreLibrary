using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _ThingSelectionUtility
    {
        
        // HARMONY CANDIDATE: postfix
        [DetourMember( typeof( ThingSelectionUtility ) )]
        internal static bool                _SelectableByMapClick( Thing t )
        {
            // If it's not selectable,
            // not spawned, or;
            // the thing is registered with the Hide Item Manager
            if(
                ( !t.def.selectable )||
                ( !t.Spawned )||
                ( HideItemManager.PreventItemSelection( t ) ) // changed
            )
            {
                return false;
            }
            if (
                ( t.def.size.x == 1 )&&
                ( t.def.size.z == 1 )
            )
            {
                return !t.Position.Fogged( t.Map );
            }
            foreach ( var cell in t.OccupiedRect() )
            {
                if( !cell.Fogged( t.Map ) )
                {
                    return true;
                }
            }
            return false;
        }

        // HARMONY CANDIDATE: prefix
        [DetourMember( typeof( ThingSelectionUtility ) )]
        internal static bool                _SelectableByHotkey( Thing t )
        {
            if(
                ( !t.def.selectable )||
                ( HideItemManager.PreventItemSelection( t ) ) // changed
            )
            {
                return false;
            }
            Pawn pawn = t as Pawn;
            if( pawn != null )
            {
                if( pawn.Dead )
                {
                    return false;
                }
                if( pawn.InContainerEnclosed )
                {
                    return false;
                }
            }
            return true;
        }

    }

}
