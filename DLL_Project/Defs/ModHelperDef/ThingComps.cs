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
        public override string              InjectString => "ThingComps injected";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ThingComps.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

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
                isValid &= DefInjectionQualifier.TargetQualifierValid( injectionSet.targetDefs, injectionSet.qualifier, "ThingComps", ref errors );
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

        public override bool                DefIsInjected( ModHelperDef def )
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

        public override bool                InjectByDef( ModHelperDef def )
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
                    stringBuilder.Append( string.Format( "ThingComps ({0}):: Qualifier returned: ", injectionSet.compProps.compClass.FullName ) );
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
