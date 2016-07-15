using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapOverlay_NonColonistPawns : MiniMapOverlay_Pawns, IConfigurable
    {
        #region Fields

        private Color enemyColor = Color.red;

        // inputfield classes for configurable options
        private UI.LabeledInput_Color enemyColorField;

        private int enemyRadius = 2;

        private UI.LabeledInput_Int enemyRadiusField;

        private Color traderColor = Color.blue;

        private UI.LabeledInput_Color traderColorField;

        private int traderRadius = 2;

        private UI.LabeledInput_Int traderRadiusField;

        // draw options
        private Color visitorColor = Color.green;

        private UI.LabeledInput_Color visitorColorField;
        private int visitorRadius = 2;
        private UI.LabeledInput_Int visitorRadiusField;

        #endregion Fields

        #region Constructors

        public MiniMapOverlay_NonColonistPawns( MiniMap minimap, MiniMapOverlayDef overlayDef ) : base( minimap, overlayDef )
        {
            CreateInputFields();
        }

        #endregion Constructors

        #region Methods

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

            // re-create input fields to update values
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
                UpdateInputFields();
        }

        public override Color GetColor( Pawn pawn )
        {
            if ( pawn.HostileTo( Faction.OfPlayer ) )
                return enemyColor;

            if ( pawn.trader != null )
                return traderColor;

            return visitorColor;
        }

        public override IEnumerable<Pawn> GetPawns()
        {
            return Find.MapPawns.AllPawnsSpawned.Where( pawn => !pawn.RaceProps.Animal && pawn.Faction != Faction.OfPlayer );
        }

        public override float GetRadius( Pawn pawn )
        {
            if ( pawn.HostileTo( Faction.OfPlayer ) )
                return enemyRadius;

            if ( pawn.trader != null )
                return traderRadius;

            return visitorRadius;
        }

        public override void Update()
        {
            base.Update();
        }

        private void CreateInputFields()
        {
            enemyColorField    = new UI.LabeledInput_Color( enemyColor, "MiniMap.NCP.EnemyColor".Translate(), "MiniMap.NCP.EnemyColorTip".Translate() );
            traderColorField   = new UI.LabeledInput_Color( traderColor, "MiniMap.NCP.TraderColor".Translate(), "MiniMap.NCP.TraderColorTip".Translate() );
            visitorColorField  = new UI.LabeledInput_Color( visitorColor, "MiniMap.NCP.VisitorColor".Translate(), "MiniMap.NCP.VisitorColorTip".Translate() );
            enemyRadiusField   = new UI.LabeledInput_Int( enemyRadius, "MiniMap.NCP.EnemyRadius".Translate(), "MiniMap.NCP.EnemyRadiusTip".Translate() );
            visitorRadiusField = new UI.LabeledInput_Int( visitorRadius, "MiniMap.NCP.VisitorRadius".Translate(), "MiniMap.NCP.VisitorRadiusTip".Translate() );
            traderRadiusField  = new UI.LabeledInput_Int( traderRadius, "MiniMap.NCP.TraderRadius".Translate(), "MiniMap.NCP.TraderRadiusTip".Translate() );
        }

        private void UpdateInputFields()
        {
            enemyColorField.Value    = enemyColor;
            traderColorField.Value   = traderColor;
            visitorColorField.Value  = visitorColor;
            enemyRadiusField.Value   = enemyRadius;
            visitorRadiusField.Value = visitorRadius;
            traderRadiusField.Value  = traderRadius;
        }

        #endregion Methods
    }
}