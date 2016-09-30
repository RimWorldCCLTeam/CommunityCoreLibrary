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
        public override string              InjectString => "ITabs injected";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ITabs.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( var iTabIndex = 0; iTabIndex < def.ITabs.Count; iTabIndex++ )
            {
                var injectionSet = def.ITabs[ iTabIndex ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
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
                }
                isValid &= DefInjectionQualifier.TargetQualifierValid( injectionSet.targetDefs, injectionSet.qualifier, "ITabs", ref errors );
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

        public override bool                DefIsInjected( ModHelperDef def )
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

        public override bool                InjectByDef( ModHelperDef def )
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
#if DEBUG
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append( string.Format( "ITabs ({0}):: Qualifier returned: ", injectionSet.newITab.FullName ) );
#endif
                    foreach( var thingDef in thingDefs )
                    {
#if DEBUG
                        stringBuilder.Append( thingDef.defName + ", " );
#endif
                        if( !InjectOrReplaceITabOn( injectionSet.newITab, injectionSet.replaceITab, thingDef ) )
                        {
                            return false;
                        }
                    }
#if DEBUG
                    CCL_Log.Message( stringBuilder.ToString(), def.ModName );
#endif
                }
            }

            return true;

        }

        private bool                        InjectOrReplaceITabOn( Type newITab, Type replaceITab, ThingDef thingDef )
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
