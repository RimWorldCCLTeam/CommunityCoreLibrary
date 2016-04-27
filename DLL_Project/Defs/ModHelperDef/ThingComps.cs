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
        public string                       InjectString
        {
            get
            {
                return "ThingComps injected";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.ThingComps.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var compSet in def.ThingComps )
            {
                if( compSet.targetDefs.NullOrEmpty() )
                {
                    errors += "\n\tNull or no targetDefs in ThingComps";
                    isValid = false;
                }
                if( compSet.compProps == null )
                {
                    errors += "\n\tNull compProps in ThingComps";
                    isValid = false;
                }
                for( int index = 0; index < compSet.targetDefs.Count; ++index )
                {
                    if( compSet.targetDefs[ index ].NullOrEmpty() )
                    {
                        errors += string.Format( "targetDef in ThingComps is null or empty at index {0}", index.ToString() );
                        isValid = false;
                    }
                    else
                    {
                        var target = compSet.targetDefs[ index ];
                        var targetDef = DefDatabase< ThingDef >.GetNamed( target, false );
                        if( targetDef == null )
                        {
                            errors += string.Format( "Unable to resolve targetDef '{0}' in ThingComps", target );
                            isValid = false;
                        }
                        else
                        {
                            if( compSet.compProps.compClass != null )
                            {
                                if( targetDef.HasComp( compSet.compProps.compClass ) )
                                {
                                    errors += string.Format( "targetDef '{0}' in ThingComps already has comp '{1}'", target, compSet.compProps.compClass );
                                    isValid = false;
                                }
                            }
                            else if( compSet.compProps.GetType() != typeof( CompProperties ) )
                            {
                                if( targetDef.GetCompProperty( compSet.compProps.GetType() ) != null )
                                {
                                    errors += string.Format( "targetDef '{0}' in ThingComps already has comp '{1}'", target, compSet.compProps );
                                    isValid = false;
                                }
                            }
                            else
                            {
                                errors += string.Format( "Can not inject CompProperties without a compClass into '{0}'", target );
                                isValid = false;
                            }
                        }
                    }
                }
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.ThingComps.NullOrEmpty() )
            {
                return true;
            }

            foreach( var compSet in def.ThingComps )
            {
                foreach( var targetName in compSet.targetDefs )
                {
                    var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                    if(
                        ( compSet.compProps.compClass != null )&&
                        ( !targetDef.comps.Exists( s => ( s.compClass == compSet.compProps.compClass ) ) )
                    )
                    {
                        return false;
                    }
                    else if( targetDef.GetCompProperty( compSet.compProps.GetType() ) == null )
                    {
                        return false;
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

            foreach( var compSet in def.ThingComps )
            {
                foreach( var targetName in compSet.targetDefs )
                {
                    // TODO:  Make a full copy using the comp in this def as a template
                    // Currently adds the comp in this def so all target use the same def
                    var targetDef = DefDatabase< ThingDef >.GetNamed( targetName );
                    if( targetDef.HasComp( compSet.compProps.compClass ) )
                    {
                        CCL_Log.TraceMod(
                            def,
                            Verbosity.Warnings,
                            string.Format( "Trying to inject ThingComp '{0}' into '{1}' when it already exists (another mod may have already injected).", compSet.compProps.compClass.ToString(), targetName ),
                            "ThingComp Injector" );
                    }
                    else
                    {
                        targetDef.comps.Add( compSet.compProps );
                    }
                }
            }

            return true;
        }

    }

}
