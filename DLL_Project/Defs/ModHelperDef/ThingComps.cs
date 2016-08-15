using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_ThingComps : IInjector
    {

#if DEBUG
        public string                       InjectString => "ThingComps injected";

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ThingComps.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( var index = 0; index < def.ThingComps.Count; index++ )
            {
                var injectionSet = def.ThingComps[ index ];
                var qualifierValid = true;
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                if(
                    ( injectionSet.targetDefs.NullOrEmpty() )&&
                    ( injectionSet.qualifier == null )
                )
                {
                    errors += "targetDefs and qualifier are both null, one or the other must be supplied";
                    isValid = false;
                    qualifierValid = false;
                }
                if(
                    ( !injectionSet.targetDefs.NullOrEmpty() )&&
                    ( injectionSet.qualifier != null )
                )
                {
                    errors += "targetDefs and qualifier are both supplied, only one or the other must be supplied";
                    isValid = false;
                    qualifierValid = false;
                }
                if( injectionSet.compProps == null )
                {
                    errors += "\n\tNull compProps in ThingComps";
                    isValid = false;
                }
                if( qualifierValid )
                {
                    if( !injectionSet.targetDefs.NullOrEmpty() )
                    {
                        for( int index2 = 0; index2 < injectionSet.targetDefs.Count; ++index2 )
                        {
                            if( injectionSet.targetDefs[ index2 ].NullOrEmpty() )
                            {
                                errors += string.Format( "targetDef in ThingComps is null or empty at index {0}", index2.ToString() );
                                isValid = false;
                            }
                            else
                            {
                                var thingDef = DefDatabase< ThingDef >.GetNamed( injectionSet.targetDefs[ index2 ], false );
                                if( thingDef == null )
                                {
                                    errors += string.Format( "Unable to resolve targetDef '{0}' in ThingComps", thingDef.defName );
                                    isValid = false;
                                }
                                else
                                {
                                    if( !CanInjectInto( thingDef, injectionSet.compProps.compClass, injectionSet.compProps.GetType() ) )
                                    {
                                        errors += string.Format( "Cannot inject ThingComps '{0}' into targetDef '{1}' - ThingComp with compClass or CompProperties may already exist, ", injectionSet.compProps, thingDef.defName );
                                        isValid = false;
                                    }
                                }
                            }
                        }
                    }
                    if( injectionSet.qualifier != null )
                    {
                        if( !injectionSet.qualifier.IsSubclassOf( typeof( DefInjectionQualifier ) ) )
                        {
                            errors += string.Format( "Unable to resolve qualifier '{0}'", injectionSet.qualifier );
                            isValid = false;
                        }
                        else
                        {
                            var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, null );
                            if( !thingDefs.NullOrEmpty() )
                            {
                                foreach( var thingDef in thingDefs )
                                {
                                    if( !CanInjectInto( thingDef, injectionSet.compProps.compClass, injectionSet.compProps.GetType() ) )
                                    {
                                        errors += string.Format( "Cannot inject ThingComps '{0}' into targetDef '{1}' - ThingComp with compClass or CompProperties may already exist, ", injectionSet.compProps, thingDef.defName );
                                        isValid = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return isValid;
        }

        private bool                        CanInjectInto( ThingDef thingDef, Type compClass, Type compProps )
        {
            if( compClass != null )
            {
                return !thingDef.HasComp( compClass );
            }
            else if( compProps != typeof( CompProperties ) )
            {
                return thingDef.GetCompProperty( compProps ) == null;
            }
            return false;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.ThingComps.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.ThingComps.Count; index++ )
            {
                var injectionSet = def.ThingComps[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
                    foreach( var thingDef in thingDefs )
                    {
                        if(
                            ( injectionSet.compProps.compClass != null )&&
                            ( !thingDef.comps.Exists( s => ( s.compClass == injectionSet.compProps.compClass ) ) )
                        )
                        {
                            return false;
                        }
                        else if( thingDef.GetCompProperty( injectionSet.compProps.GetType() ) == null )
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool                         Inject( ModHelperDef def )
        {
            if( def.ThingComps.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.ThingComps.Count; index++ )
            {
                var injectionSet = def.ThingComps[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
#if DEBUG
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append( "ThingComps :: Qualifier returned: " );
#endif
                    foreach( var thingDef in thingDefs )
                    {
#if DEBUG
                        stringBuilder.Append( thingDef.defName + ", " );
#endif
                        // TODO:  Make a full copy using the comp in this def as a template
                        // Currently adds the comp in this def so all target use the same def
                        if( !thingDef.HasComp( injectionSet.compProps.compClass ) )
                        {
                            thingDef.comps.Add( injectionSet.compProps );
                        }
                    }
#if DEBUG
                    CCL_Log.Message( stringBuilder.ToString(), def.ModName );
#endif
                }
            }

            return true;
        }

    }

}
