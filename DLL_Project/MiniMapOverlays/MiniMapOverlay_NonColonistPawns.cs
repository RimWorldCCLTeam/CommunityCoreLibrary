using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

	public class MiniMapOverlay_NonColonistPawns : MiniMapOverlay_Pawns
	{

		#region Constructors

		public MiniMapOverlay_NonColonistPawns( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
		{
			radius = 2f;
		}

		#endregion Constructors

		#region Methods

		public override Color GetColor( Pawn pawn, float opacity = 1 )
		{
			var color = pawn.HostileTo( Faction.OfColony ) ? Color.red : GenUI.MouseoverColor;
			color.a = opacity;
			return color;
		}

		public override IEnumerable<Pawn> GetPawns()
		{
			return Find.MapPawns.AllPawnsSpawned.Where( pawn => !pawn.RaceProps.Animal && pawn.Faction != Faction.OfColony );
		}

		#endregion Methods

	}

}
