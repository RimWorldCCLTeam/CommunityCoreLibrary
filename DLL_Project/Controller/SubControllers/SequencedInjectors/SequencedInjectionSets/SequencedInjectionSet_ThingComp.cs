using System;
using System.Collections.Generic;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjectionSet_ThingComp : SequencedInjectionSet
    {

        public CompProperties               compProps;

        public                              SequencedInjectionSet_ThingComp()
        {
            injectionSequence               = InjectionSequence.MainLoad;
            injectionTiming                 = InjectionTiming.ThingComps;
        }

        public override Type                defType => typeof( ThingDef );
        public override InjectorTargetting  Targetting => InjectorTargetting.Multi;

        public override bool                IsValid()
        {
            var valid = true;
            if( compProps == null )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    "compProps is null",
                    Name
                );
                valid = false;
            }
            else if( compProps.compClass == null )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "compClass is null for '{0}'", compProps.GetType().FullName ),
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
            if( !typeof( ThingWithComps ).IsAssignableFrom( thingDef.thingClass ) )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "ThingDef '{0}' is not a ThingWithComps", thingDef.defName ),
                    Name
                );
                return false;
            }
            if( !thingDef.comps.NullOrEmpty() )
            {
                foreach( var prop in thingDef.comps )
                {
                    if( prop.compClass == compProps.compClass )
                    {
                        CCL_Log.Trace(
                            Verbosity.Validation,
                            string.Format( "ThingDef '{0}' already has ThingComp '{1}'", thingDef.defName, compProps.compClass.FullName ),
                            Name
                        );
                        return false;
                    }
                    else if(
                        ( compProps.GetType() != typeof( CompProperties ) )&&
                        ( prop.GetType() == compProps.GetType() )
                    )
                    {
                        CCL_Log.Trace(
                            Verbosity.Validation,
                            string.Format( "ThingDef '{0}' already has CompProperties '{1}'", thingDef.defName, compProps.GetType().FullName ),
                            Name
                        );
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool                Inject( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            if( thingDef.comps == null )
            {
                thingDef.comps = new List<CompProperties>();
            }
            // TODO:  Make a full copy using the comp in this def as a template
            // Currently adds the comp in this def so all targets use the same def
            // 12/10/2016 - Is that really needed?  This injector is applied to defs
            // which are templates for game objects (Things) and not actual game
            // objects themselves.  When a game object is instantiated the data
            // is copied to the ThingComp object on the game object itself.
            thingDef.comps.Add( compProps );
#if DEBUG
            return thingDef.comps.Contains( compProps );
#else
            return true;
#endif
        }

    }

}
