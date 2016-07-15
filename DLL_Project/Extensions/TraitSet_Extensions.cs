using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class TraitSet_Extensions
    {
        
        public static Trait GetTrait( this TraitSet traits, TraitDef traitDef )
        {
            foreach( var trait in traits.allTraits )
            {
                if( trait.def == traitDef )
                {
                    return trait;
                }
            }
            return null;
        }

        public static void RemoveTrait( this TraitSet traits, TraitDef traitDef )
        {
            var trait = traits.GetTrait( traitDef );
            if( trait == null )
            {
                return;
            }
            traits.allTraits.Remove( trait );
        }

    }

}
