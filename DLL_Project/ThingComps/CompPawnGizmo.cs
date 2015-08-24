using System.Collections.Generic;
using Verse;

namespace CommunityCoreLibrary
{
	public class CompPawnGizmo : ThingComp
	{
        public override IEnumerable<Command> CompGetGizmosExtra()
		{
	        var comp = (parent as Pawn)?.equipment?.Primary?.AllComps.Find(s =>
	        {
		        if (!s.GetType().IsSubclassOf(typeof (CompRangedGizmoGiver)))
		        {
			        Log.Message(s + " not sub");
			        return false;
		        }

		        var gizmoGiver = s as CompRangedGizmoGiver;
		        return gizmoGiver != null && gizmoGiver.isRangedGiver;
	        });

	        if (comp == null)
	        {
				Log.Message("null");
				yield break;
	        }

			Log.Message("gizmo");
			foreach (var current in comp.CompGetGizmosExtra())
				yield return current;
		}
	}
}
