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

    // Note:  Core now supports outside access to the list of designators
    // This change makes these extensions redundant
    // TODO:  Remove this for A15
    public static class ReverseDesignatorDatabase_Extensions
    {

        public static void                  Add( Designator designator )
        {
            if( ReverseDesignatorDatabase.AllDesignators == null )
            {
                return;
            }
            ReverseDesignatorDatabase.AllDesignators.Add( designator );
        }

        public static Designator            Find( Type designator )
        {
            return ReverseDesignatorDatabase.AllDesignators == null
                ? null
                : ReverseDesignatorDatabase.AllDesignators.FirstOrDefault( d => ( d.GetType() == designator ) );
        }

        public static void                  Remove( Type designator )
        {
            if( ReverseDesignatorDatabase.AllDesignators == null )
            {
                return;
            }
            var des = Find( designator );
            if( des != null )
            {
                ReverseDesignatorDatabase.AllDesignators.Remove( des );
            }
        }

    }
}

