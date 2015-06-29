using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class CompColoredLight : ThingComp
    {
        private CompGlower              compGlower;
        private CompGlower              oldGlower;
        private CompPowerTrader         compPower;
        private int                     ColorIndex = -1;
        private float                   lightRadius;

        private CompProperties_ColoredLight compProps
        {
            get
            {
                return (CompProperties_ColoredLight)props;
            }
        }

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
            changeColor( compProps.color[ ColorIndex ].value );
        }

        public void changeColor( ColorInt colour )
        {
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

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append( "inspectColor".Translate() + compProps.color[ ColorIndex ].name );
            return stringBuilder.ToString();
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            // Color change gizmo (only if research is complete)
            if( DefDatabase< ResearchProjectDef >.GetNamed( compProps.requiredResearch ).IsFinished )
            {
                Command_Action command_Action = new Command_Action();
                if( command_Action != null )
                {
                    command_Action.icon = Icon.NextButton;
                    command_Action.defaultLabel = "buttonChangeColor".Translate();
                    command_Action.defaultDesc = NextColorName( ColorIndex );
                    command_Action.activateSound = SoundDef.Named( "Click" );
                    command_Action.action = new Action( IncrementColorIndex );
                    if( command_Action.action != null )
                    {
                        yield return command_Action;
                    }
                }
            }
            // No more gizmos
            yield break;
        }

        private void IncrementColorIndex()
        {
            ColorIndex += 1;
            if( ColorIndex >= compProps.color.Count )
            {
                ColorIndex = 0;
            }
            changeColor( compProps.color[ ColorIndex ].value );
        }
        
        private string NextColorName( int index )
        {
            int nextIndex = index + 1;
            if( nextIndex >= compProps.color.Count )
            {
                nextIndex = 0;
            }
            return compProps.color[ nextIndex ].name;
        }

    }
}

