using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using CommunityCoreLibrary.Commands;

namespace CommunityCoreLibrary
{

    public class CompColoredLight : ThingComp
    {
        #region Private vars

        private CompGlower                  compGlower;
        private CompGlower                  oldGlower;
        private CompPowerTrader             compPower;
        private int                         ColorIndex = -1;
        private float                       lightRadius;

        #endregion

        #region Gizmos

        private CommandChangeColor            _GizmoChangeColor;
        private CommandChangeColor            GizmoChangeColor {
            get{
                if( _GizmoChangeColor == null )
                    _GizmoChangeColor = new CommandChangeColor( this );
                return _GizmoChangeColor;
            }
        }

        private CommandGroupOfTouchingThingsByLinker  _GizmoGroupOfThingsByLinker;
        private CommandGroupOfTouchingThingsByLinker  GizmoGroupOfThingsByLinker {
            get {
                if( _GizmoGroupOfThingsByLinker == null )
                    _GizmoGroupOfThingsByLinker = new CommandGroupOfTouchingThingsByLinker( this.parent, GroupColorChange, GroupColorChange );
                return _GizmoGroupOfThingsByLinker;
            }
        }

        private CommandGroupOfDefOrThingCompInRoom  _GizmoChangeRoommateColor;
        private CommandGroupOfDefOrThingCompInRoom  GizmoChangeRoommateColor {
            get {
                if( _GizmoChangeRoommateColor == null )
                {
                    _GizmoChangeRoommateColor = new CommandGroupOfDefOrThingCompInRoom( this.parent, this.GetType(), "CommandChangeRoommateColorLabel".Translate() );
                    _GizmoChangeRoommateColor.ByDefLabel = Translator.Translate( "CommandChangeRoommateColorLClick", this.parent.def.label );
                    _GizmoChangeRoommateColor.ByDefAction = GroupColorChange;
                    _GizmoChangeRoommateColor.ByThingCompLabel = "CommandChangeRoommateColorRClick".Translate();
                    _GizmoChangeRoommateColor.ByThingCompAction = GroupColorChange;
                    _GizmoChangeRoommateColor.defaultDesc = _GizmoChangeRoommateColor.Desc;
                }
                return _GizmoChangeRoommateColor;
            }
        }

        #endregion

        #region Color properties

        private CompProperties_ColoredLight compProps
        {
            get
            {
                return (CompProperties_ColoredLight)props;
            }
        }

        #endregion

        #region Base ThingComp overrides

        public override void PostExposeData()
        {
            //Log.Message( def.defName + " - ExposeData()" );
            base.PostExposeData();

            Scribe_Values.LookValue<int>( ref ColorIndex, "ColorIndex", -1 );
        }

        public override void PostSpawnSetup()
        {
            //Log.Message( def.defName + " - SpawnSetup()" );
            base.PostSpawnSetup();

            // Get power comp
            compPower = parent.GetComp<CompPowerTrader>();
            if( compPower == null ){
                Log.Message( parent.def.defName + " - Needs compPowerTrader!" );
                return;
            }

            // Get the default glower
            oldGlower = parent.GetComp<CompGlower>();
            if( oldGlower == null )
            {
                Log.Message( parent.def.defName + " - Needs compGlower!" );
                return;
            }

            // Get the colour palette
            if( compProps == null )
            {
                Log.Message( parent.def.defName + " - Needs CompProperties_ColoredLight!" );
                return;
            }

            // Set default palette if none is specified
            if( compProps.color == null )
                compProps.color = Light.Color;

            // Set default
            if( ( ColorIndex < 0 )||
                ( ColorIndex >= compProps.color.Count ) )
                ColorIndex = compProps.Default;

            // Get the glow radius
            lightRadius = oldGlower.props.glowRadius;

            // Set the light colour
            changeColor( ColorIndex );
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append( "CompColorLightInspect".Translate() + compProps.color[ ColorIndex ].name );
            return stringBuilder.ToString();
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            // Color change gizmo (only if research is complete)
            if( DefDatabase< ResearchProjectDef >.GetNamed( compProps.requiredResearch ).IsFinished )
                yield return GizmoChangeColor;

            // In room
            if( parent.Position.IsInRoom() )
                // In room by def or comp
                if( parent.HasRoommateByThingComp( GetType() ) )
                    yield return GizmoChangeRoommateColor;
            
            // Has a link flag
            if( parent.def.graphicData.linkFlags != LinkFlags.None ){
                // Touching things by link
                if( parent.HasTouchingByLinker() )
                    yield return GizmoGroupOfThingsByLinker;
            }

            // No more gizmos
            yield break;
        }

        #endregion

        #region Public functions

        public void DecrementColorIndex()
        {
            int index = ( ColorIndex + compProps.color.Count - 1 ) % compProps.color.Count;
            changeColor( index );
        }
        public void IncrementColorIndex()
        {
            int index = ( ColorIndex + 1 ) % compProps.color.Count;
            changeColor( index );
        }

        public int GetColorByName( string name )
        {
            for( int i = 0, count = compProps.color.Count; i < count; i++ ){
                var cv = compProps.color[ i ];
                if( cv.name == name )
                    return i;
            }
            return -1;
        }

        public string PrevColorName()
        {
            int index = ( ColorIndex + compProps.color.Count - 1 ) % compProps.color.Count;
            return compProps.color[ index ].name;
        }

        public string NextColorName()
        {
            int index = ( ColorIndex + 1 ) % compProps.color.Count;
            return compProps.color[ index ].name;
        }

        #endregion

        #region Gizmo callbacks
        // The list of things are all the lights we want to change
        private void GroupColorChange( List< Thing > things )
        {
            // Now set their color (if *their* research is complete)
            foreach( Thing l in things )
            {
                // Get it's colored light comp
                var otherComp = l.TryGetComp<CompColoredLight>();
                if( otherComp != null )
                {
                    // Do they even have this color?
                    int otherColor = otherComp.GetColorByName( compProps.color[ ColorIndex ].name );

                    // If it does, check it's research
                    if( ( otherColor > -1 )&&
                        ( DefDatabase<ResearchProjectDef>.GetNamed( otherComp.compProps.requiredResearch ).IsFinished ) )
                    {    // Change it's color
                        otherComp.changeColor( otherColor );
                    }
                }
            }
        }

        public void changeColor( int index )
        {
            ColorInt colour = compProps.color[ index ].value;
            ColorIndex = index;
            GizmoChangeColor.defaultDesc = GizmoChangeColor.Desc;

            // Current lit state from base compGlower
            bool wasLit = oldGlower.Lit;

            // Turn off old glower
            oldGlower.Lit = false;

            // Turn off replaced glower
            if( compGlower != null )
            {
                // Current lit state from base replacement compGlower
                wasLit = compGlower.Lit;

                // Turn off replacement compGlower
                compGlower.Lit = false;
            }

            // New glower
            CompGlower newGlower = new CompGlower();
            if( newGlower == null )
            {
                Log.Message( parent.def.defName + " - Unable to create new compGlower!" );
                return;
            }
            newGlower.parent = parent;

            // Glower properties
            CompProperties newProps = new CompProperties();
            if( newProps == null )
            {
                Log.Message( parent.def.defName + " - Unable to create new compProperties!" );
                return;
            }
            newProps.compClass = typeof( CompGlower );
            newProps.glowColor = colour;
            newProps.glowRadius = lightRadius;

            // Add properties to glower
            newGlower.Initialize( newProps );


            // Fetch the comps list
            List<ThingComp> allComps = typeof( ThingWithComps ).GetField( "comps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetValue( parent ) as List<ThingComp>;
            if( allComps == null )
            {
                Log.Message( parent.def.defName + " - Could not get list of comps!" );
                return;
            }


            // Remove default glower
            allComps.Remove( oldGlower );

            // Remove current glower
            if( compGlower != null )
            {
                allComps.Remove( compGlower );
            }

            // Add new glower
            allComps.Add( newGlower );
            compGlower = newGlower;

            // Store comps list
            typeof( ThingWithComps ).GetField( "comps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( parent, allComps );


            // Update glow grid
            compGlower.Lit = false;
            Find.GlowGrid.MarkGlowGridDirty( parent.Position );
            Find.MapDrawer.MapMeshDirty( parent.Position, MapMeshFlag.GroundGlow );
            Find.MapDrawer.MapMeshDirty( parent.Position, MapMeshFlag.Things );

            if( ( wasLit )&&( compPower.PowerOn ) )
            {
                // Turn it on if it the old glower was on
                compGlower.Lit = true;
            }
        }

        #endregion
   }
}