using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{
	public class Building_LightFixture : Building
	{
		private CompGlower				compGlower;
		private CompGlower				oldGlower;
		private CompPowerTrader			compPower;
		private int						ColourIndex = -1;
		private float					lightRadius;

		public override void ExposeData()
		{
			//Log.Message( def.defName + " - ExposeData()" );
			base.ExposeData();

			Scribe_Values.LookValue<int>( ref ColourIndex, "ColourIndex", -1 );
		}

		public override void SpawnSetup()
		{
			//Log.Message( def.defName + " - SpawnSetup()" );
			base.SpawnSetup();

			// Get power comp
			compPower = GetComp<CompPowerTrader>();
			if( compPower == null ){
				Log.Message( def.defName + " - Needs compPowerTrader!" );
				return;
			}

			// Get the default glower
			oldGlower = GetComp<CompGlower>();
			if( oldGlower == null )
			{
				Log.Message( def.defName + " - Needs compGlower!" );
				return;
			}

			// Set white as default
			if( ColourIndex < 0 )
				ColourIndex = 0;

			// Get the glow radius
			lightRadius = oldGlower.props.glowRadius;

			// Set the light colour
			changeColour( LightColour.value[ ColourIndex ] );
		}

		public void changeColour( ColorInt colour )
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
				Log.Message( def.defName + " - Unable to create new compGlower!" );
				return;
			}
			newGlower.parent = this;

			// Glower properties
			CompProperties newProps = new CompProperties();
			if( newProps == null )
			{
				Log.Message( def.defName + " - Unable to create new compProperties!" );
				return;
			}
			newProps.compClass = typeof( CompGlower );
			newProps.glowColor = colour;
			newProps.glowRadius = lightRadius;

			// Add properties to glower
			newGlower.Initialize( newProps );


			// Fetch the comps list
			List<ThingComp> allComps = typeof( ThingWithComps ).GetField( "comps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetValue( this ) as List<ThingComp>;
			if( allComps == null )
			{
				Log.Message( def.defName + " - Could not get list of comps!" );
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
			typeof( ThingWithComps ).GetField( "comps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).SetValue( this, allComps );


			// Update glow grid
			compGlower.Lit = false;
			Find.GlowGrid.MarkGlowGridDirty( Position );
			Find.MapDrawer.MapChanged( Position, MapChangeType.GroundGlow );
			Find.MapDrawer.MapChanged( Position, MapChangeType.Things );

			if( ( wasLit )&&( compPower.PowerOn ) )
			{
				// Turn it on if it the old glower was on
				compGlower.Lit = true;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.Append( "Colour: " + LightColour.name[ ColourIndex ] );
			return stringBuilder.ToString();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			// Default gizmos
			foreach( Gizmo curGizmo in base.GetGizmos() ) yield return curGizmo;

			if( DefDatabase< ResearchProjectDef >.GetNamed( "ColoredLights" ).IsFinished )
			{
				// Colour change gizmo
				Command_Action command_Action = new Command_Action();
				if( command_Action != null )
				{
					command_Action.icon = Icon.NextButton;
					command_Action.defaultLabel = "buttonChangeColour".Translate();
					command_Action.defaultDesc = LightColour.NextColourName( ColourIndex );
					command_Action.activateSound = SoundDef.Named( "Click" );
					command_Action.action = new Action( IncrementColourIndex );
					if( command_Action.action != null )
					{
						yield return command_Action;
					}
				}
			}
			// No more gizmos
			yield break;
		}

		private void IncrementColourIndex()
		{
			ColourIndex += 1;
			if( ColourIndex >= LightColour.count )
			{
				ColourIndex = 0;
			}
			changeColour( LightColour.value[ ColourIndex ] );
		}
	}
}

