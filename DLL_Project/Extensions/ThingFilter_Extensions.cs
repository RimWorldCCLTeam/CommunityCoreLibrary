using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class ThingFilter_Extensions
    {

        private static FieldInfo            _categories;
        private static FieldInfo            _exceptedCategories;
        private static FieldInfo            _thingDefs;
        private static FieldInfo            _exceptedThingDefs;

        static                              ThingFilter_Extensions()
        {
            _categories = typeof( ThingFilter ).GetField( "categories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            if( _categories == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'categories' in 'ThingFilter'",
                    "ThingFilter_Extensions" );
            }
            _exceptedCategories = typeof( ThingFilter ).GetField( "exceptedCategories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            if( _exceptedCategories == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'exceptedCategories' in 'ThingFilter'",
                    "ThingFilter_Extensions" );
            }
            _thingDefs = typeof( ThingFilter ).GetField( "thingDefs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            if( _thingDefs == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'thingDefs' in 'ThingFilter'",
                    "ThingFilter_Extensions" );
            }
            _exceptedThingDefs = typeof( ThingFilter ).GetField( "exceptedThingDefs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            if( _exceptedThingDefs == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'exceptedThingDefs' in 'ThingFilter'",
                    "ThingFilter_Extensions" );
            }
        }

        public static bool                  Matches( this ThingFilter a, ThingFilter b )
        {
            foreach( var thingDefA in a.AllowedThingDefs )
            {
                if( !b.Allows( thingDefA ) )
                {
                    return false;
                }
            }
            foreach( var thingDefB in b.AllowedThingDefs )
            {
                if( !a.Allows( thingDefB ) )
                {
                    return false;
                }
            }
            return true;
        }

        public static List<string>          Categories( this ThingFilter thingFilter )
        {
            return (List<string>) _categories.GetValue( thingFilter );
        }

        public static List<string>          ExceptedCategories( this ThingFilter thingFilter )
        {
            return (List<string>) _exceptedCategories.GetValue( thingFilter );
        }

        public static List<ThingDef>        ThingDefs( this ThingFilter thingFilter )
        {
            return (List<ThingDef>) _thingDefs.GetValue( thingFilter );
        }

        public static List<ThingDef>        ExceptedThingDefs( this ThingFilter thingFilter )
        {
            return (List<ThingDef>) _exceptedThingDefs.GetValue( thingFilter );
        }

    }

}
