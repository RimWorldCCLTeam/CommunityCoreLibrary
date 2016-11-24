using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal class _JoyGiver : JoyGiver
    {

        internal static FieldInfo           _tmpCandidates;

        static                              _JoyGiver()
        {
            _tmpCandidates = typeof( JoyGiver ).GetField( "tmpCandidates", Controller.Data.UniversalBindingFlags );
            if( _tmpCandidates == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'tmpCandidates' in 'JoyGiver'",
                    "Detour.JoyGiver" );
            }
        }

        internal static List<Thing>         tmpCandidates
        {
            get
            {
                return (List<Thing>) _tmpCandidates.GetValue( null );
            }
        }

        [DetourMember]
        internal List<Thing>                _SearchSet
        {
            get
            {
                tmpCandidates.Clear();
                if( this.def.thingDefs == null )
                {
                    return tmpCandidates;
                }
                for( int index = 0; index < this.def.thingDefs.Count; index++ )
                {
                    var targetDef = this.def.thingDefs[ index ];
                    // Add things direct
                    tmpCandidates.AddRange( Find.ListerThings.ThingsOfDef( targetDef ) );
                    // TODO: Add NPDs that produce
                    // tmpCandidates.AddRangeUnique( Find.ListerBuildings.AllBuildingsColonistOfClass<Building_NutrientPasteDispenser>().Where( npd => npd.DispensableDef == targetDef ).ToList().ConvertAll( npd => (Thing)npd ) );
                    // Add Synthesizers that produce
                    tmpCandidates.AddRange( Find.ListerBuildings.AllBuildingsColonistOfClass<Building_AutomatedFactory>().Where( synthesizer => (
                        ( !synthesizer.IsReserved )&&
                        ( synthesizer.CanProduce( targetDef ) )
                    ) ).ToList().ConvertAll( synthesizer => (Thing)synthesizer ) );
                }
                return tmpCandidates;
            }
        }

        // This method must be implemented as the base class is abstract
        public override Job                 TryGiveJob( Pawn pawn )
        {
            throw new NotImplementedException();
        }

    }

}
