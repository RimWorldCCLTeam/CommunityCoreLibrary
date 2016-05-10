using System;

using Verse;

namespace CommunityCoreLibrary
{

    public static class Common
    {

        public static void                  SpawnThingDefOfCountAt( ThingDef of, int count, IntVec3 at )
        {
            while( count > 0 )
            {
                Thing newStack = ThingMaker.MakeThing( of, null );
                newStack.stackCount = Math.Min( count, of.stackLimit );
                GenSpawn.Spawn( newStack, at );
                count -= newStack.stackCount;
            }
        }

        public static void                  ReplaceThingWithThingDefOfCount( Thing oldThing, ThingDef of, int count )
        {
            IntVec3 at = oldThing.Position;
            oldThing.Destroy();
            while( count > 0 )
            {
                Thing newStack = ThingMaker.MakeThing( of, null );
                newStack.stackCount = Math.Min( count, of.stackLimit );
                GenSpawn.Spawn( newStack, at );
                count -= newStack.stackCount;
            }
        }

        public static void                  RemoveDesignationDefOfAt( DesignationDef of, IntVec3 at )
        {
            Designation designation = Find.DesignationManager.DesignationAt( at, of );
            if( designation != null )
            {
                Find.DesignationManager.RemoveDesignation( designation );
            }
        }

        public static void                  RemoveDesignationDefOfOn( DesignationDef of, Thing on )
        {
            Designation designation = Find.DesignationManager.DesignationOn( on, of );
            if( designation != null )
            {
                Find.DesignationManager.RemoveDesignation( designation );
            }
        }

    }

}
