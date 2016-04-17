using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _Building_Door
    {
        
        internal static int _AlignQualityAgainst( IntVec3 c )
        {
            if( !c.InBounds() )
            {
                return 0;
            }
            if( !c.Walkable() )
            {
                return 9;
            }
            List<Thing> list = Find.ThingGrid.ThingsListAt( c );
            for( int index = 0; index < list.Count; ++index )
            {
                Thing thing1 = list[ index ];
                if(
                    ( thing1.def.thingClass == typeof( Building_Door ) )||
                    ( thing1.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                )
                {
                    return 1;
                }
                Thing thing2 = thing1 as Blueprint;
                if( thing2 != null )
                {
                    if( thing2.def.entityDefToBuild.passability == Traversability.Impassable )
                    {
                        return 9;
                    }
                    if(
                        ( thing2.def.thingClass == typeof( Building_Door ) )||
                        ( thing2.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                    )
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }

    }

}
