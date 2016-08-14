using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_ITabs : IInjector
    {

#if DEBUG
        public string                       InjectString
        {
            get
            {
                return "ITabs injected";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ITabs.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( var iTabIndex = 0; iTabIndex < def.ITabs.Count; iTabIndex++ )
            {
                var qualifierValid = true;
                var injectionSet = def.ITabs[ iTabIndex ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var replaceTabIsValid = true;
                if(
                    ( injectionSet.newITab == null )||
                    ( !injectionSet.newITab.IsSubclassOf( typeof( ITab ) ) )
                )
                {
                    errors += string.Format( "Unable to resolve ITab '{0}'", injectionSet.newITab );
                    isValid = false;
                }
                if(
                    ( injectionSet.replaceITab != null )&&
                    ( !injectionSet.replaceITab.IsSubclassOf( typeof( ITab ) ) )
                )
                {
                    errors += string.Format( "Unable to resolve ITab '{0}'", injectionSet.replaceITab );
                    isValid = false;
                    replaceTabIsValid = false;
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
                if( qualifierValid )
                {
                    if( !injectionSet.targetDefs.NullOrEmpty() )
                    {
                        for( var index = 0; index < injectionSet.targetDefs.Count; index++ )
                        {
                            if( injectionSet.targetDefs[ index ].NullOrEmpty() )
                            {
                                errors += string.Format( "targetDef in ITabs is null or empty at index {0}", index.ToString() );
                                isValid = false;
                            }
                            else
                            {
                                var thingDef = DefDatabase<ThingDef>.GetNamed( injectionSet.targetDefs[ index ], false );
                                if( thingDef == null )
                                {
                                    errors += string.Format( "Unable to resolve targetDef '{0}'", injectionSet.targetDefs[ index ] );
                                    isValid = false;
                                }
                                else if(
                                    ( injectionSet.replaceITab != null )&&
                                    ( replaceTabIsValid )
                                )
                                {
                                    if( !CanReplaceOn( thingDef, injectionSet.replaceITab ) )
                                    {
                                        errors += string.Format( "targetDef '{0}' does not contain ITab '{1}' to replace", injectionSet.targetDefs[ index ], injectionSet.replaceITab );
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
                        else if(
                                ( injectionSet.replaceITab != null )&&
                                ( replaceTabIsValid )
                            )
                        {
                            var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, null );
                            if( !thingDefs.NullOrEmpty() )
                            {
                                foreach( var thingDef in thingDefs )
                                {
                                    if( !CanReplaceOn( thingDef, injectionSet.replaceITab ) )
                                    {
                                        errors += string.Format( "qualified ThingDef '{0}' does not contain ITab '{1}' to replace", thingDef.defName, injectionSet.replaceITab );
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

        private bool                        CanReplaceOn( ThingDef thingDef, Type replaceITab )
        {
            return(
                ( !thingDef.inspectorTabs.NullOrEmpty() )&&
                ( thingDef.inspectorTabs.Contains( replaceITab ) )
            );
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.ITabs.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.ITabs.Count; index++ )
            {
                var injectionSet = def.ITabs[ index ];
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
                            ( thingDef.inspectorTabs.NullOrEmpty() )||
                            ( !thingDef.inspectorTabs.Contains( injectionSet.newITab ) )
                        )
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
            if( def.ITabs.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.ITabs.Count; index ++ )
            {
                var injectionSet = def.ITabs[ index ];
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
                        if( !InjectITab( injectionSet.newITab, injectionSet.replaceITab, thingDef ) )
                        {
                            return false;
                        }
                    }
                }
            }

            return true;

        }

        private bool                        InjectITab( Type newITab, Type replaceITab, ThingDef thingDef )
        {
            var injectedITab = (ITab) Activator.CreateInstance( newITab );
            if( injectedITab == null )
            {
                return false;
            }
            if( thingDef.inspectorTabs.NullOrEmpty() )
            {
                thingDef.inspectorTabs = new List<Type>();
            }
            if( thingDef.inspectorTabsResolved.NullOrEmpty() )
            {
                thingDef.inspectorTabsResolved = new List<ITab>();
            }
            var injectTypeAt = replaceITab == null
                ? thingDef.inspectorTabs.Count
                : thingDef.inspectorTabs.IndexOf( replaceITab );
            var injectResolvedAt = replaceITab == null
                ? thingDef.inspectorTabsResolved.Count
                : thingDef.inspectorTabsResolved.FindIndex( r => r.GetType() == replaceITab );
            if( replaceITab != null )
            {
                thingDef.inspectorTabs.RemoveAt( injectTypeAt );
                thingDef.inspectorTabsResolved.RemoveAt( injectResolvedAt );
            }
            thingDef.inspectorTabs.Insert( injectTypeAt, newITab );
            thingDef.inspectorTabsResolved.Insert( injectResolvedAt, injectedITab );
            return true;
        }

    }

}
