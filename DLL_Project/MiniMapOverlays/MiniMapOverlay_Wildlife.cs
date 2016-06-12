using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapOverlay_Wildlife : MiniMapOverlay_Pawns, IConfigurable
    {
        #region Fields

        private Color
            wildColor = Color.yellow,
            tameColor = Color.green,
            hostileColor = Color.red,
            huntingColor = GenUI.MouseoverColor,
            tamingColor = GenUI.MouseoverColor;

        private UI.LabeledInput_Color
            wildColorInput,
            tameColorInput,
            hostileColorInput,
            huntingColorInput,
            tamingColorInput;

        private float
                    wildRadius = 1f,
            tameRadius = 1f,
            hostileRadius = 1f,
            huntingRadius = 1f,
            tamingRadius = 1f;

        private UI.LabeledInput_Float
            wildRadiusInput,
            tameRadiusInput,
            hostileRadiusInput,
            huntingRadiusInput,
            tamingRadiusInput;

        #endregion Fields

        #region Constructors

        public MiniMapOverlay_Wildlife( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
            CreateInputFields();
        }

        #endregion Constructors

        #region Methods

        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            tameColorInput.Draw( row );
            tameColor = tameColorInput.Value;
            row.y += 30f;

            tameRadiusInput.Draw( row );
            tameRadius = tameRadiusInput.Value;
            row.y += 30f;

            wildColorInput.Draw( row );
            wildColor = wildColorInput.Value;
            row.y += 30f;

            wildRadiusInput.Draw( row );
            wildRadius = wildRadiusInput.Value;
            row.y += 30f;

            hostileColorInput.Draw( row );
            hostileColor = hostileColorInput.Value;
            row.y += 30f;

            hostileRadiusInput.Draw( row );
            hostileRadius = hostileRadiusInput.Value;
            row.y += 30f;

            huntingColorInput.Draw( row );
            huntingColor = huntingColorInput.Value;
            row.y += 30f;

            huntingRadiusInput.Draw( row );
            huntingRadius = huntingRadiusInput.Value;
            row.y += 30f;

            tamingColorInput.Draw( row );
            tamingColor = tamingColorInput.Value;
            row.y += 30f;

            tamingRadiusInput.Draw( row );
            tamingRadius = tamingRadiusInput.Value;

            return 10 * 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref tameColor, "tameColor" );
            Scribe_Values.LookValue( ref wildColor, "wildColor" );
            Scribe_Values.LookValue( ref hostileColor, "hostileColor" );
            Scribe_Values.LookValue( ref huntingColor, "huntingColor" );
            Scribe_Values.LookValue( ref tamingColor, "tamingColor" );
            Scribe_Values.LookValue( ref tameRadius, "tameRadius" );
            Scribe_Values.LookValue( ref huntingRadius, "huntingRadius" );
            Scribe_Values.LookValue( ref tamingRadius, "tamingRadius" );
            Scribe_Values.LookValue( ref wildRadius, "wildRadius" );
            Scribe_Values.LookValue( ref hostileRadius, "hostileRadius" );

            // re-create input fields to update values
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
                UpdateInputFields();
        }

        public override Color GetColor( Pawn pawn )
        {
            // tame
            if ( pawn.Faction == Faction.OfColony )
                return tameColor;

            // predator
            if ( pawn.def.race.predator )
                return hostileColor;
            
            // hostile
            if ( pawn.HostileTo( Faction.OfColony ) )
                return hostileColor;

            var designation = Find.DesignationManager.DesignationOn( pawn )?.def;
            // hunted
            if ( designation == DesignationDefOf.Hunt )
                return huntingColor;

            // being tamed
            if ( designation == DesignationDefOf.Tame )
                return tameColor;

            // other
            return wildColor;
        }

        public override IEnumerable<Pawn> GetPawns()
        {
            return Find.MapPawns.AllPawns.Where( pawn => pawn.RaceProps.Animal );
        }

        public override float GetRadius( Pawn pawn )
        {
            // tame
            if ( pawn.Faction == Faction.OfColony )
                return tameRadius;

            // hostile
            if ( pawn.HostileTo( Faction.OfColony ) )
                return hostileRadius;

            // hunted
            var designation = Find.DesignationManager.DesignationOn( pawn )?.def;
            if ( designation == DesignationDefOf.Hunt )
                return huntingRadius;

            // being tamed
            if ( designation == DesignationDefOf.Tame )
                return tameRadius;

            // other
            return wildRadius;
        }

        private void CreateInputFields()
        {
            wildColorInput       = new UI.LabeledInput_Color( wildColor, "MiniMap.WL.WildColor".Translate(), "MiniMap.WL.WildColorTip".Translate() );
            tameColorInput       = new UI.LabeledInput_Color( tameColor, "MiniMap.WL.TameColor".Translate(), "MiniMap.WL.TameColorTip".Translate() );
            hostileColorInput    = new UI.LabeledInput_Color( hostileColor, "MiniMap.WL.HostileColor".Translate(), "MiniMap.WL.HostileColorTip".Translate() );
            huntingColorInput    = new UI.LabeledInput_Color( huntingColor, "MiniMap.WL.HuntingColor".Translate(), "MiniMap.WL.HuntingColorTip".Translate() );
            tamingColorInput     = new UI.LabeledInput_Color( tamingColor, "MiniMap.WL.TamingColor".Translate(), "MiniMap.WL.TamingColorTip".Translate() );
            wildRadiusInput      = new UI.LabeledInput_Float( wildRadius, "MiniMap.WL.WildRadius".Translate(), "MiniMap.WL.WildRadiusTip".Translate() );
            tameRadiusInput      = new UI.LabeledInput_Float( tameRadius, "MiniMap.WL.TameRadius".Translate(), "MiniMap.WL.TameRadiusTip".Translate() );
            hostileRadiusInput   = new UI.LabeledInput_Float( hostileRadius, "MiniMap.WL.HostileRadius".Translate(), "MiniMap.WL.HostileRadiusTip".Translate() );
            huntingRadiusInput   = new UI.LabeledInput_Float( huntingRadius, "MiniMap.WL.HuntingRadius".Translate(), "MiniMap.WL.HuntingRadiusTip".Translate() );
            tamingRadiusInput    = new UI.LabeledInput_Float( tamingRadius, "MiniMap.WL.TamingRadius".Translate(), "MiniMap.WL.TamingRadiusTip".Translate() );
        }

        private void UpdateInputFields()
        {
            wildColorInput.Value   = wildColor;
            tameColorInput.Value   = tameColor;
            hostileColorInput.Value  = hostileColor;
            huntingColorInput.Value  = huntingColor;
            tamingColorInput.Value   = tamingColor;
            wildRadiusInput.Value  = wildRadius;
            tameRadiusInput.Value  = tameRadius;
            hostileRadiusInput.Value = hostileRadius;
            huntingRadiusInput.Value = huntingRadius;
            tamingRadiusInput.Value  = tamingRadius;
        }

        #endregion Methods
    }
}