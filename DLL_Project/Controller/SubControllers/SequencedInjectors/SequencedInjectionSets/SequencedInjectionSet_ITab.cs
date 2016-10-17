using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class SequencedInjectionSet_ITab : SequencedInjectionSet
    {

        public Type                         newITab;
        public Type                         replaceITab;

        public                              SequencedInjectionSet_ITab()
        {
            injectionSequence               = InjectionSequence.MainLoad;
            injectionTiming                 = InjectionTiming.ITabs;
        }

        public override Type                defType => typeof( ThingDef );
        public override InjectorTargetting  Targetting => InjectorTargetting.Multi;

        public override bool                IsValid()
        {
            var valid = true;
            if( newITab == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "newITab is null",
                    Name
                );
                valid = false;
            }
            else if( !newITab.IsSubclassOf( typeof( ITab ) ) )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    string.Format( "Unable to resolve ITab '{0}'", newITab.FullName ),
                    Name
                );
                valid = false;
            }
            if(
                ( replaceITab != null )&&
                ( !replaceITab.IsSubclassOf( typeof( ITab ) ) )
            )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    string.Format( "Unable to resolve ITab '{0}'", replaceITab.FullName ),
                    Name
                );
                valid = false;
            }
            return valid;
        }

        public override bool                TargetIsValid( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            return(
                ( !thingDef.inspectorTabs.NullOrEmpty() )&&
                ( thingDef.inspectorTabs.Contains( replaceITab ) )
            );
        }

        public override bool                Inject( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
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
#if DEBUG
            return(
                ( thingDef.inspectorTabs.Contains( newITab ) )&&
                ( thingDef.inspectorTabsResolved.Contains( injectedITab ) )
            );
#else
            return true;
#endif
        }

    }

}

