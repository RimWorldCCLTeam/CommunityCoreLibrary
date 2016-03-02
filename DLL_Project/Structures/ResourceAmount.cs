using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public struct ResourceAmount
    {

        public ThingDef                     thingDef;
        public int                          count;

        public                              ResourceAmount( ThingDef thingDef, int count )
        {
            this.thingDef = thingDef;
            this.count = count;
        }

        public static void                  AddToList( List<ResourceAmount> list, ThingDef thingDef, int countToAdd )
        {
            for( int index = 0; index < list.Count; ++index )
            {
                if( list[ index ].thingDef == thingDef )
                {
                    list[ index ] = new ResourceAmount( list[ index ].thingDef, list[ index ].count + countToAdd );
                    return;
                }
            }
            list.Add( new ResourceAmount( thingDef, countToAdd ) );
        }

    }

}
