using System;
using System.Reflection;
using System.Security.Permissions;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompRottableRefrigeratedInjector : SpecialInjector
    {

        // TODO:  Alpha 13 API change
        //public override bool Inject()
        public override void                Inject()
        {
            // Replace CompRottable on ThingDefs
            var thingDefs = DefDatabase< ThingDef >.AllDefsListForReading;
            if( !thingDefs.NullOrEmpty() )
            {
                foreach( var thingDef in thingDefs )
                {
                    if( !thingDef.comps.NullOrEmpty() )
                    {
                        foreach( var prop in thingDef.comps )
                        {
                            if( prop.GetType() == typeof( CompProperties_Rottable ) )
                            {
                                prop.compClass = typeof( CompRottableRefrigerated );
                            }
                        }
                    }
                }
            }
            // TODO:  Alpha 13 API change
            //return true;
        }

    }

}
