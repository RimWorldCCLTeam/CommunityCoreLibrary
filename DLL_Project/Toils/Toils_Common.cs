using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
    public static class Toils_Common
    {

        public static Toil                  SpawnThingDefOfCountAt( ThingDef of, int count, IntVec3 at, Map map )
        {
            return new Toil
            {
                initAction = delegate
                {
                    Common.SpawnThingDefOfCountAt( of, count, at, map );
                }
            };
        }

        public static Toil                  ReplaceThingWithThingDefOfCount( Thing oldThing, ThingDef of, int count )
        {
            return new Toil
            {
                initAction = delegate
                {
                    Common.ReplaceThingWithThingDefOfCount( oldThing, of, count );
                }
            };
        }

        public static Toil                  RemoveDesignationDefOfAt( DesignationDef of, IntVec3 at, Map map )
        {
            return new Toil
            {
                initAction = delegate
                {
                    Common.RemoveDesignationDefOfAt( of, at, map );
                }
            };
        }

        public static Toil                  RemoveDesignationDefOfOn( DesignationDef of, Thing on )
        {
            return new Toil
            {
                initAction = delegate
                {
                    Common.RemoveDesignationDefOfOn( of, on );
                }
            };
        }

    }

}
