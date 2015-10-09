using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public static class ReverseDesignatorDatabase_Extensions
    {

        private static List<Designator>     desList = null;
        private static List<Designator>     DesList
        {
            get
            {
                if( desList == null )
                {
                    desList = typeof( ReverseDesignatorDatabase ).GetField( "desList", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( null ) as List<Designator>;
                    if( desList == null )
                    {
                        CCL_Log.Error( "Reflection unable to get field \"desList\"", "ReverseDesignatorDatabase" );
                    }
                }
                return desList;
            }
        }

        public static void                  Add( Designator designator )
        {
            if( DesList == null )
            {
                return;
            }
            DesList.Add( designator );
        }

        public static Designator            Find( Type designator )
        {
            return DesList == null
                ? null
                    : DesList.FirstOrDefault( d => ( d.GetType() == designator ) );
        }

        public static void                  Remove( Type designator )
        {
            if( DesList == null )
            {
                return;
            }
            var des = Find( designator );
            if( des != null )
            {
                DesList.Remove( des );
            }
        }

    }
}

