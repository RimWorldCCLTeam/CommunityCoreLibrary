using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class HediffSet_Extensions
    {

        public static BodyPartRecord GetBodyPartRecord( this HediffSet hediffSet, BodyPartDef bodyPartDef )
        {
            foreach( BodyPartRecord bodyPartRecord in hediffSet.GetNotMissingParts( new BodyPartHeight?(), new BodyPartDepth?() ) )
            {
                if( bodyPartRecord.def == bodyPartDef )
                {
                    return bodyPartRecord;
                }
            }
            return (BodyPartRecord) null;
        }

        public static BodyPartRecord GetBodyPartRecord( this HediffSet hediffSet, string bodyPartDefName )
        {
            foreach( BodyPartRecord bodyPartRecord in hediffSet.GetNotMissingParts( new BodyPartHeight?(), new BodyPartDepth?() ) )
            {
                if( bodyPartRecord.def.defName == bodyPartDefName )
                {
                    return bodyPartRecord;
                }
            }
            return (BodyPartRecord) null;
        }

    }

}
