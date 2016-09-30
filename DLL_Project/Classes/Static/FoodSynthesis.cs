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
                        ( def.IsIngestible )&&
                        ( !def.IsDrug )
                    );
                }
                return _IsMeal;
            }
        }

        private static Func<ThingDef,ThingDef,int>  _SortMeal;
        public static Func<ThingDef,ThingDef,int> SortMeal
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

        private static Func<ThingDef,bool>          _IsDrug;
        public static Func<ThingDef,bool>           IsDrug
        {
            get
            {
                if( _IsDrug == null )
                {
                    _IsDrug = new Func<ThingDef,bool>( def =>
                        ( def.IsDrug )
                    );
                }
                return _IsDrug;
            }
        }

        private static Func<ThingDef,ThingDef,int>  _SortDrug;
        public static Func<ThingDef,ThingDef,int>   SortDrug
        {
            get
            {
                if( _SortDrug == null )
                {
                    _SortDrug = new Func<ThingDef, ThingDef, int>( ( x, y ) =>
                        ( x.ingestible.joy > y.ingestible.joy ) ? -1 : 1
                    );
                }
                return _SortDrug;
            }
        }

    }

}
