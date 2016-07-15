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
    public static class DesignationCategoryDef_Extensions
    {
        // A14 - resolvedDesignators is now private - the public accessor filters by currently allowed.
        // TODO: This should be moved to a helper class.
        public static FieldInfo _resolvedDesignatorsField = typeof( DesignationCategoryDef ).GetField( "resolvedDesignators", BindingFlags.NonPublic | BindingFlags.Instance );

        public static List<Designator> _resolvedDesignators( this DesignationCategoryDef category )
        {
            return _resolvedDesignatorsField.GetValue( category ) as List<Designator>;
        }
    }
}
