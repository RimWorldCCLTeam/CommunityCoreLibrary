using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public static class FoodSynthesis
    {
        
        private static Func<ThingDef,bool>          _IsMeal;
        public static Func<ThingDef,bool>           IsMeal
        {
            get
            {
                if( _IsMeal == null )
                {
                    _IsMeal = new Func<ThingDef,bool>( def =>
                        ( def.IsIngestible() )&&
                        ( !def.IsAlcohol() )
                    );
                }
                return _IsMeal;
            }
        }

        private static Func<ThingDef,ThingDef,int>  _SortMeal;
        internal static Func<ThingDef,ThingDef,int> SortMeal
        {
            get
            {
                if( _SortMeal == null )
                {
                    _SortMeal = new Func<ThingDef, ThingDef, int>( ( x, y ) =>
                        ( x.ingestible.nutrition > y.ingestible.nutrition ) ? -1 : 1
                    );
                }
                return _SortMeal;
            }
        }

        private static Func<ThingDef,bool>          _IsAlcohol;
        public static Func<ThingDef,bool>           IsAlcohol
        {
            get
            {
                if( _IsAlcohol == null )
                {
                    _IsAlcohol = new Func<ThingDef,bool>( def =>
                        ( def.IsAlcohol() )
                    );
                }
                return _IsAlcohol;
            }
        }

        private static Func<ThingDef,ThingDef,int>  _SortAlcohol;
        public static Func<ThingDef,ThingDef,int>   SortAlcohol
        {
            get
            {
                if( _SortAlcohol == null )
                {
                    _SortAlcohol = new Func<ThingDef, ThingDef, int>( ( x, y ) =>
                        ( x.ingestible.joy > y.ingestible.joy ) ? -1 : 1
                    );
                }
                return _SortAlcohol;
            }
        }

    }

}
