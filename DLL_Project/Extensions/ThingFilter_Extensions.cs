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
            if( _categories == null )
            {
                _categories = typeof( ThingFilter ).GetField( "categories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            }
            return (List<string>) _categories.GetValue( thingFilter );
        }

        public static List<string>          ExceptedCategories( this ThingFilter thingFilter )
        {
            if( _exceptedCategories == null )
            {
                _exceptedCategories = typeof( ThingFilter ).GetField( "exceptedCategories", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            }
            return (List<string>) _exceptedCategories.GetValue( thingFilter );
        }

        public static List<ThingDef>        ThingDefs( this ThingFilter thingFilter )
        {
            if( _thingDefs == null )
            {
                _thingDefs = typeof( ThingFilter ).GetField( "thingDefs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            }
            return (List<ThingDef>) _thingDefs.GetValue( thingFilter );
        }

        public static List<ThingDef>        ExceptedThingDefs( this ThingFilter thingFilter )
        {
            if( _exceptedThingDefs == null )
            {
                _exceptedThingDefs = typeof( ThingFilter ).GetField( "exceptedThingDefs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField );
            }
            return (List<ThingDef>) _exceptedThingDefs.GetValue( thingFilter );
        }

    }

}
