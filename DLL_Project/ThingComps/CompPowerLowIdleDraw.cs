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
	public class CompPowerLowIdleDraw : ThingComp
	{
		private CompGlower				compGlower;
		private CompPowerTrader			compPower;
		private Building				thisBuilding;
		private List<IntVec3>			scanPosition;
		private Pawn					curUser;
		private Job						curJob;
		// minIdleDraw is to prevent idlePower from being 0.0f.  This
		// is important so that the device stays connected to the power
		// net without actually drawing off of it when not in use.
		private const float				minIdleDraw = 0.01f;
		private float					idlePower;
		private float					curPower = -1f;
		private bool					onIfOn;

		private int						keepOnTicks;
		private int						keepOnTickCount;

		private CompProperties_LowIdleDraw compProps
		{
			get
			{
				return (CompProperties_LowIdleDraw)props;
			}
		}

		private bool					isItOn
		{
			get
			{
				return ( -compPower.PowerOutput > idlePower );
			}
		}

		public override void PostExposeData()
		{

			//Log.Message( parent.def.defName + " - PostExposeData()" );
			base.PostExposeData();

			Scribe_Values.LookValue<int>( ref keepOnTicks, "keepOnTicks", 0 );
			Scribe_Values.LookValue<float>( ref curPower, "curPower", -1f );

		}

		public override void PostSpawnSetup()
		{
			//Log.Message( parent.def.defName + " - PostSpawnSetup()" );
			base.PostSpawnSetup();

			// Get this building
			thisBuilding = (Building)parent;
			if( thisBuilding == null ) {
				Log.Message( "Community Core Library :: CompPowerLowIdleDraw :: Unable to cast '" + parent.def.defName + "' to Building" );
				return;
			}

			// Get the power comp
			compPower = parent.GetComp<CompPowerTrader>();
			if( compPower == null )
			{
				Log.Message( "Community Core Library :: CompPowerLowIdleDraw :: '" + parent.def.defName + "' needs compPowerTrader!" );
				return;
			}

			// Get the idle properties
			if( compProps == null )
			{
				Log.Message( "Community Core Library :: CompPowerLowIdleDraw :: '" + parent.def.defName + "' unable to get properties of CompProperties_LowIdleDraw!" );
				return;
			}

			// Get the glower (optional)
			compGlower = parent.GetComp<CompGlower>();

			// Generate the list of cells to check
			BuildScanList();

			// Calculate low-power mode consumption
			idlePower = compProps.idlePowerFactor * compPower.props.basePowerConsumption;
			if( idlePower < minIdleDraw )
				idlePower = minIdleDraw;
			//Log.Message( parent.def.defName + " - " + idlePower + " - " + compPower.props.basePowerConsumption + " - " + compProps.idlePowerFactor );

			// Initial state...

			// ...No user...
			curUser = null;
			curJob = null;

			if( curPower < idlePower ){
				// ...Default full power...
				curPower = compPower.props.basePowerConsumption;
				keepOnTicks = keepOnTickCount;
			}

			// Set power usage
			compPower.PowerOutput = -curPower;
		}

		public override void CompTick()
		{
			base.CompTick();

			// keepOnTicks--;
			PowerLevelToggle( 1 );
		}

		public override void CompTickRare()
		{
			base.CompTickRare();

			// keepOnTicks -= 250;
			PowerLevelToggle( 250 );
		}

		public override void ReceiveCompSignal( string signal )
		{
			// Asked to power down?
			if( ( signal == "PowerTurnedOff" )||( signal == "PowerTurnedOn " ) ){
				// Force toggle now
				PowerLevelToggle( 1000000 );
			}
		}

		private void PowerLevelToggle( int thisTickCount )
		{
			// If it's on, don't recheck until it times out
			if( keepOnTicks > 0 ) {
				keepOnTicks -= thisTickCount;
				if( keepOnTicks > 0 ) return;
			}
			//Log.Message( parent.def.defName + " - PowerLevelToggle" );

			// Basic decision, turn on if someone is on it, only if it turns
			// on when someone is on it (duh?), otherwise turn on if someone is
			// on it's interaction cell AND is using it.  Failing all that, go
			// to low power mode.
			bool turnItOn = false;

			// Should it...?
			if( !onIfOn ) {
				//Log.Message( parent.def.defName + " - InteractionCell" );
				// This building has an interaction cell, this means it has
				// jobs associated with it.  Only go to full-power if the
				// pawn standing there has a job using the building.

				// Break out of a loop for optimization because for some
				// reason using gotos is considered evil, even though
				// having an machine language background I think people
				// who think this way are just insisting a "potatoe"
				// is a "potato"
				while( true ){

					// Quickly check the last user is still using...
					if( ( curUser != null )&&( curUser.Position == scanPosition[0] ) ) {
						// They're here...
						if( ( curUser.CurJob != null )&&( curUser.CurJob == curJob ) ) {
							// ...they're using!
							//Log.Message( parent.def.defName + " - Same User" );
							turnItOn = true;
							break;
						}
					}

					// Look for a new user...

					Pawn pUser = Find.ThingGrid.ThingAt<Pawn>( scanPosition[0] );
					if( pUser != null ) {
						// ...A pawn is here!...

						if( pUser.CurJob != null ) {
							// ...With a job!...

							// ...(checking targets)...
							if      ( ( pUser.CurJob.targetA != null )&&( pUser.CurJob.targetA.Thing != null )&&( pUser.CurJob.targetA.Thing.def == thisBuilding.def ) ) {
								turnItOn = true;
							}else if( ( pUser.CurJob.targetB != null )&&( pUser.CurJob.targetB.Thing != null )&&( pUser.CurJob.targetB.Thing.def == thisBuilding.def ) ) {
								turnItOn = true;
							}else if( ( pUser.CurJob.targetC != null )&&( pUser.CurJob.targetC.Thing != null )&&( pUser.CurJob.targetC.Thing.def == thisBuilding.def ) ) {
								turnItOn = true;
							}

							if( turnItOn ){
								// ..Using this building!...
								//Log.Message( parent.def.defName + " - New User" );
								curUser = pUser;
								curJob = pUser.CurJob;
							}
						}
					}

					// Exit loop
					break;
				}
			}else{
				//Log.Message( parent.def.defName + " - onIfOn" );
				// Full-power when any pawn is standing on any monitored cell...
				foreach( IntVec3 curPos in scanPosition ) {
					if( Find.ThingGrid.ThingAt<Pawn>( curPos ) != null ){
						// Found a pawn, turn it on and early out
						//Log.Message( parent.def.defName + " - A pawn upon" );
						turnItOn = true;
						break;
					}
				}
			}

			// Do?...
			if( turnItOn ){
				// ...Turn on...
				if( !isItOn ){
					// ...because it is off
					//Log.Message( parent.def.defName + " - turnItOn" );
					TogglePower();
				}else{
					// ..keep it on, actually
					//Log.Message( parent.def.defName + " - keepItOn" );
					keepOnTicks = keepOnTickCount;
				}
			}
			else if ( isItOn )	{
				// ...Is on, turn off
				//Log.Message( parent.def.defName + " - turnItOff" );
				TogglePower();
			}
			if( ( !isItOn )&&( compGlower != null )&&( compGlower.Lit ) ){
				// Glower on with on power???
				ToggleGlower( false );
			}
		}

		private void TogglePower()
		{
			if( isItOn )
			{
				// Is on, turn off
				//Log.Message( parent.def.defName + " - Going low-power" );
				curPower = idlePower;
				compPower.PowerOutput = -curPower;
				keepOnTicks = 0;
				curUser = null;
				curJob = null;
			}
			else
			{
				// Is off, turn on
				//Log.Message( parent.def.defName + " - Going full-power" );
				curPower = compPower.props.basePowerConsumption;
				compPower.PowerOutput = -curPower;
				keepOnTicks = keepOnTickCount;
			}

			// Adjust glower...
			if( compGlower != null ){
				// ...if it exists
				ToggleGlower( isItOn );
			}
		}

		private void ToggleGlower( bool turnItOn )
		{
			//Log.Message( parent.def.defName + " - Toggle glower" );
			// If no state change, abort
			if( turnItOn == compGlower.Lit ) return;

			// Toggle and update glow grid
			compGlower.Lit = turnItOn;
			Find.GlowGrid.MarkGlowGridDirty( thisBuilding.Position );
			Find.MapDrawer.MapChanged( thisBuilding.Position, MapChangeType.GroundGlow );
			Find.MapDrawer.MapChanged( thisBuilding.Position, MapChangeType.Things );
		}

		private void BuildScanList()
		{
			//Log.Message( parent.def.defName + " - BuildScanlist" );
			// Default to interaction cell only, is also means that a pawn must
			// be using the building so star-gazers aren't turning the stove on.
			onIfOn = false;

			// List of cells to check
			scanPosition = new List<IntVec3>();

			// Get the map positions to monitor
			if( thisBuilding.def.hasInteractionCell ){
				// Force-add interaction cell
				//Log.Message( parent.def.defName + " - Force-add InteractionCell" );
				scanPosition.Add( thisBuilding.InteractionCell );

				switch( compProps.operationalMode ){
				case LowIdleDrawMode.WhenNear :
					// And the adjacent cells too???
					// Only really used by NPDs as they "need time to prepare"
					foreach( IntVec3 curPos in GenAdj.CellsAdjacent8Way( thisBuilding.InteractionCell ) )
					{
						AddScanPositionIfAllowed( curPos );
					}
					onIfOn = true;
					break;
				case LowIdleDrawMode.GroupUser :
					// Group user adds cells "in front" of it
					// Only really used by TVs
					/*
					 * if( ( parent.def == ThingDefOf.Television )
						||( parent.def == ThingDefOf.TelevisionLED ) ){

						foreach( IntVec3 curPos in GenAdj.CellsAdjacent8Way( thisBuilding.InteractionCell ) )
						{
							AddScanPositionIfAllowed( curPos );
						}
					}
					*/
					break;
				}

				// Give work tables more time so pawns have time to fetch ingredients
				keepOnTickCount = 250;

			}else{
				// Pawn standing on building means we need the buildings occupancy
				onIfOn = true;

				foreach( IntVec3 curPos in GenAdj.CellsOccupiedBy( thisBuilding ) )
				{
					AddScanPositionIfAllowed( curPos );
				}

				if (compProps.operationalMode == LowIdleDrawMode.WhenNear)
					// And the adjacent cells too???
					foreach (IntVec3 curPos in GenAdj.CellsAdjacent8Way( thisBuilding )) {
						AddScanPositionIfAllowed (curPos);
					}

				keepOnTickCount = 50;
			}
		}

		private void AddScanPositionIfAllowed( IntVec3 position )
		{
			// Look at each thing at this position and check it's passability
			foreach( Thing curThing in Find.ThingGrid.ThingsListAt( position ) )
			{
				if( curThing.def.passability == Traversability.Impassable )
				{
					// Impassable cell, exit right meow!
					return;
				}
			}
			// All things are passable, add cell
			scanPosition.Add( position );
			//Log.Message( parent.def.defName + " - Add Cell " + position.ToString() );
		}
	}
}