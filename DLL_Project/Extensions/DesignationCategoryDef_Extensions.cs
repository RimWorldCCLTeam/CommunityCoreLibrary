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
        public static FieldInfo             _resolvedDesignatorsField;

        static                              DesignationCategoryDef_Extensions()
        {
            _resolvedDesignatorsField = typeof( DesignationCategoryDef ).GetField( "resolvedDesignators", Controller.Data.UniversalBindingFlags );
            if( _resolvedDesignatorsField == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'resolvedDesignatorsField' in 'DesignationCategoryDef'",
                    "DesignationCategoryDef_Extensions" );
            }
        }

        public static List<Designator>      ResolvedDesignators( this DesignationCategoryDef category )
        {
            return _resolvedDesignatorsField.GetValue( category ) as List<Designator>;
        }
    }
}
