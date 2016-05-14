using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	public class MiniMapOverlay_NonColonistPawns : MiniMapOverlay_Pawns, IConfigurable
	{
        // draw options
        private Color visitorColor = Color.green;
        private Color traderColor = Color.blue;
        private Color enemyColor = Color.red;
        private int visitorRadius = 2;
        private int traderRadius = 2;
        private int enemyRadius = 2;

        // inputfield classes for configurable options
        UI.LabeledInput_Color enemyColorField;
        UI.LabeledInput_Color visitorColorField;
        UI.LabeledInput_Color traderColorField;
        UI.LabeledInput_Int enemyRadiusField;
        UI.LabeledInput_Int visitorRadiusField;
        UI.LabeledInput_Int traderRadiusField;

        #region Constructors

        public MiniMapOverlay_NonColonistPawns( MiniMap minimap, MiniMapOverlayDef overlayDef ) : base( minimap, overlayDef )
        {
            radius = 2f;
            enemyColorField = new UI.LabeledInput_Color( enemyColor, "MiniMap.NCP.EnemyColor".Translate() );
            traderColorField = new UI.LabeledInput_Color( traderColor, "MiniMap.NCP.TraderColor".Translate() );
            visitorColorField = new UI.LabeledInput_Color( visitorColor, "MiniMap.NCP.VisitorColor".Translate() );
            enemyRadiusField = new UI.LabeledInput_Int( enemyRadius, "Minimap.NCP.EnemyRadius".Translate() );
            visitorRadiusField = new UI.LabeledInput_Int( visitorRadius, "Minimap.NCP.VisitorRadius".Translate() );
            traderRadiusField = new UI.LabeledInput_Int( traderRadius, "Minimap.NCP.TraderRadius".Translate() );
        }

        #endregion Constructors

        #region Iconfigurable implementation
        
        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            enemyColorField.Draw( row );
            enemyColor = enemyColorField.Value;
            row.y += 30f;

            enemyRadiusField.Draw( row );
            enemyRadius = enemyRadiusField.Value;
            row.y += 30f;

            visitorColorField.Draw( row );
            visitorColor = visitorColorField.Value;
            row.y += 30f;

            visitorRadiusField.Draw( row );
            visitorRadius = visitorRadiusField.Value;
            row.y += 30f;

            traderColorField.Draw( row );
            traderColor = traderColorField.Value;
            row.y += 30f;

            traderRadiusField.Draw( row );
            traderRadius = traderRadiusField.Value;

            return 6 * 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref visitorColor, "visitorColor" );
            Scribe_Values.LookValue( ref traderColor, "traderColor" );
            Scribe_Values.LookValue( ref enemyColor, "enemyColor" );
            Scribe_Values.LookValue( ref visitorRadius, "visitorRadius" );
            Scribe_Values.LookValue( ref traderRadius, "traderRadius" );
            Scribe_Values.LookValue( ref enemyRadius, "enemyRadius" );
        }

        #endregion

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
