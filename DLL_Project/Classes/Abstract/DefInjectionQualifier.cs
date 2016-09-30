using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public abstract class DefInjectionQualifier
    {
        public abstract bool                DefIsUsable( Def def );

        public static bool                  TargetQualifierValid( List<string> targetDefs, Type qualifier, string injector, ref string errors )
        {
            var valid = true;
            if(
                ( targetDefs.NullOrEmpty() )&&
                ( qualifier == null )
            )
            {
                errors += string.Format( "targetDefs and qualifier are both null in {0}, one or the other must be supplied", injector );
                valid = false;
            }
            if(
                ( !targetDefs.NullOrEmpty() )&&
                ( qualifier != null )
            )
            {
                errors += string.Format( "targetDefs and qualifier are both supplied in {0}, only one or the other can be supplied", injector );
                valid = false;
            }
            if( valid )
            {
                if( !targetDefs.NullOrEmpty() )
                {
                    foreach( var targetName in targetDefs )
                    {
                        var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                        if( targetDef == null )
                        {
                            errors += string.Format( "Unable to resolve targetDef '{0}' in {1}", targetName, injector );
                            valid = false;
                        }
                    }
                }
                if( qualifier != null )
                {
                    if( !qualifier.IsSubclassOf( typeof( DefInjectionQualifier ) ) )
                    {
                        errors += string.Format( "Unable to resolve qualifier '{0}' in {1}", qualifier, injector );
                        valid = false;
                    }
                }
            }
            return valid;
        }

        public static List<ThingDef>        FilteredThingDefs( Type qualifier, ref DefInjectionQualifier qualifierInt, List<string> targetDefs )
        {
            if( !targetDefs.NullOrEmpty() )
            {
                return DefDatabase<ThingDef>.AllDefs.Where( def => targetDefs.Contains( def.defName ) ).ToList();
            }
            if( qualifierInt == null )
            {
                qualifierInt = (DefInjectionQualifier) Activator.CreateInstance( qualifier );
                if( qualifierInt == null )
                {
                    return null;
                }
            }
            return DefDatabase<ThingDef>.AllDefs.Where( qualifierInt.DefIsUsable ).ToList();
        }

    }

}

