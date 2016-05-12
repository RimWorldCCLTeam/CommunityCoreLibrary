using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	public class MiniMapOverlay_Wildlife : MiniMapOverlay_Pawns
	{

		#region Constructors

		public MiniMapOverlay_Wildlife( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
		{
			radius = 1f;
		}

		#endregion Constructors

		#region Methods

		public override Color GetColor( Pawn pawn, float opacity = 1 )
		{
			if( pawn.Faction == Faction.OfColony )
				return Color.green;
			else
			{
				if( pawn.HostileTo( Faction.OfColony ) )
					return Color.red;
				if( Find.DesignationManager.DesignationOn( pawn ) != null )
					return GenUI.MouseoverColor;
				return Color.yellow;
			}
		}

		public override IEnumerable<Pawn> GetPawns()
		{
			return Find.MapPawns.AllPawns.Where( pawn => pawn.RaceProps.Animal );
		}

		#endregion Methods

	}

}
