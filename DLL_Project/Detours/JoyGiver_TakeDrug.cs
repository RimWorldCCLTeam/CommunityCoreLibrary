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

    internal class _JoyGiver_TakeDrug : JoyGiver_TakeDrug
    {

        #region Detoured Methods
        
        [DetourMember]
        internal Thing                      _BestIngestItem( Pawn pawn, Predicate<Thing> extraValidator )
        {
            Predicate<Thing> validator = (Thing t) => (
                ( this.CanIngestForJoy( pawn, t ) )&&
                (
                    ( extraValidator == null )||
                    ( extraValidator( t ) )
                )
            );

            var container = pawn.inventory.innerContainer;
            for( int index = 0; index < container.Count; index++ )
            {
                var containerItem = container[ index ];
                if( validator( containerItem ) )
                {
                    return containerItem;
                }
            }

            // changed (custom drug finding logic) {
            Thing ingestible;
            ThingDef ingestibleDef;
            if( !DrugUtility.TryFindJoyDrug(
                pawn.Position,
                pawn,
                9999f,
                false,
                JoyGiver_TakeDrug_Extensions.TakeableDrugs(),
                out ingestible,
                out ingestibleDef ) )
            {
                return null;
            } // } changed
            return ingestible;
        }

        #endregion

    }

}
