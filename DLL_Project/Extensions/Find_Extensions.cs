using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Find_Extensions
    {

        // This is a safe method of fetching a map component of a specified type
        // If an instance of the component doesn't exist or map isn't loaded it will return null
        public static MapComponent          MapComponent( Type t )
        {
            if(
                ( Find.Map != null )&&
                ( !Find.Map.components.NullOrEmpty() )&&
                ( Find.Map.components.Exists( c => c.GetType() == t ) )
            )
            {
                return Find.Map.components.Find( c => c.GetType() == t );
            }
            return null;
        }

    }

}
