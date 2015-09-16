using System.Collections.Generic;
using Verse;

namespace CommunityCoreLibrary
{
    public class CompPawnGizmo : ThingComp
    {
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            var pawn = parent as Pawn;
            var equip = pawn != null
                ? pawn.equipment.Primary
                : null;

            var comp = equip != null
                ? equip.AllComps.Find( s =>
                {
                    if ( !s.GetType().IsSubclassOf(typeof (CompRangedGizmoGiver)) )
                    {
                        // Comp must be a subclass of ranged gizmo giver
                        return false;
                    }

                    var gizmoGiver = s as CompRangedGizmoGiver;
                    return gizmoGiver != null && gizmoGiver.isRangedGiver;
                } )
                : null;

            if ( comp == null )
            {
                yield break;
            }

            foreach ( var current in comp.CompGetGizmosExtra() )
            {
                yield return current;
            }
        }
    }
}
