using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

	public class CompPowerLowIdleDraw : ThingComp
	{

		[Unsaved]

		#region Instance Data

		List<IntVec3> scanPosition;
		Pawn curUser;
		Job curJob;
		// minIdleDraw is to prevent idlePower from being 0.0f
		// This is important so that the device stays connected to the
		// power net without actually drawing off of it when not in use.
		public const float MinIdleDraw = -0.01f;
		public float IdlePower;
		float curPower = 1f;
		bool onIfOn;

		int keepOnTicks;

		public CompProperties_LowIdleDraw IdleProps;

        private CompGlower                  _CompGlower;
        private CompPowerTrader             _CompPower;
        private CompFacility                _CompFacility;

		#endregion

		#region Comp Getters

		Building_AutomatedFactory AutomatedFactory
		{
			get
			{
				return parent as Building_AutomatedFactory;
			}
		}

        private CompGlower                  CompGlower
        {
            get
            {
                if( _CompGlower == null )
                {
                    _CompGlower = this.parent.TryGetComp<CompGlower>();
                }
                return _CompGlower;
            }
        }

        private CompPowerTrader             CompPower
        {
            get
            {
                if( _CompPower == null )
                {
                    _CompPower = this.parent.TryGetComp<CompPowerTrader>();
                }
                return _CompPower;
            }
        }

        private CompFacility                CompFacility
        {
            get
            {
                if( _CompFacility == null )
                {
                    _CompFacility = this.parent.TryGetComp<CompFacility>();
                }
                return _CompFacility;
            }
        }

		#endregion

		#region Query State

		public bool LowPowerMode
		{
			get
			{
				return !( CompPower.PowerOutput < IdlePower );
			}
		}

		#endregion

		public override void PostExposeData()
		{
			base.PostExposeData();

			Scribe_Values.LookValue<int>( ref keepOnTicks, "keepOnTicks", 30 );
			Scribe_Values.LookValue<float>( ref curPower, "curPower", 1f );
			Scribe_References.LookReference<Pawn>( ref curUser, "curUser" );

			if(
				( Scribe.mode == LoadSaveMode.LoadingVars ) &&
				( curUser != null )
			)
			{
				curJob = curUser.CurJob;
			}
		}

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();

			// Validate power comp
#if DEBUG
            if( CompPower == null )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompPower",
                    "CompPowerLowIdleDraw"
                );
                return;
            }
#endif

			// Get the idle properties
			IdleProps = this.CompProperties_LowIdleDraw();
#if DEBUG
            if( IdleProps == null )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompProperties_LowIdleDraw",
                    "CompPowerLowIdleDraw"
                );
                return;
            }
#endif

#if DEBUG
            // Validate "InUse"
            if(
                ( IdleProps.operationalMode == LowIdleDrawMode.InUse )&&
                ( !parent.def.hasInteractionCell )&&
                ( CompFacility == null )
            )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Parent building must be have an interaction cell or CompFacility to use 'InUse'",
                    "CompPowerLowIdleDraw"
                );
                return;
            }
            // Validate "Factory"
            if(
                ( IdleProps.operationalMode == LowIdleDrawMode.Factory )&&
                ( AutomatedFactory == null )
            )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Parent building must be ThingClass Building_AutomatedFactory to use 'Factory'",
                    "CompPowerLowIdleDraw"
                );
                return;
            }
            // Validate "GroupUse"
            if(
                ( IdleProps.operationalMode == LowIdleDrawMode.GroupUse )&&
                ( parent.def.GetJoyGiverDefsUsing().NullOrEmpty() )
            )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Parent building must be used as a thingDef by a JoyGiverDef to use 'GroupUse'",
                    "CompPowerLowIdleDraw"
                );
                return;
            }
#endif

			// Generate the list of cells to check
			BuildScanList();

			// Calculate low-power mode consumption
            if( IdleProps.idlePowerFactor < 1.0f )
            {
			    IdlePower = IdleProps.idlePowerFactor * -CompPower.Props.basePowerConsumption;
            }
            else
            {
                IdlePower = -IdleProps.idlePowerFactor;
            }
			if( IdlePower > MinIdleDraw )
			{
				IdlePower = MinIdleDraw;
			}

			// Initial state...

			if( curPower > IdlePower )
			{
				// ...Default off...
				curPower = IdlePower;
			}

			// Set power usage
			CompPower.PowerOutput = curPower;
		}

		public override void CompTick()
		{
			base.CompTick();

			if( !CompPower.PowerOn )
			{
				return;
			}

			if( !parent.IsHashIntervalTick( 30 ) )
			{
				return;
			}

			// keepOnTicks -= 30;
			PowerLevelToggle( 30 );
		}

		public override void CompTickRare()
		{
			base.CompTickRare();

			if( !CompPower.PowerOn )
			{
				return;
			}

			// keepOnTicks -= 250;
			PowerLevelToggle( 250 );
		}

		public override void ReceiveCompSignal( string signal )
		{
			// Asked to power down?
			if(
                ( signal == CompPowerTrader.PowerTurnedOnSignal )||
                ( signal == CompPowerTrader.PowerTurnedOffSignal )
			)
			{
				// Force toggle now
				PowerLevelToggle( 1000000 );
			}
		}


		void PowerLevelToggle( int thisTickCount )
		{
			// If it's on, don't recheck until it times out
			keepOnTicks -= thisTickCount;
			if( keepOnTicks > 0 )
			{
				return;
			}

			// Basic decision, turn on if someone is on it, only if it turns
			// on when someone is on it (duh?), otherwise turn on if someone is
			// on it's interaction cell AND is using it.  Failing all that, go
			// to low power mode.
			bool turnItOn = false;

			// Should it...?
			if( !onIfOn )
			{

				switch( IdleProps.operationalMode )
				{
				case LowIdleDrawMode.InUse:
                    // This building has an interaction cell or CompAffectedByFacilities,
                    // this means it has jobs associated with it.  Only go to full-power
                    // if the pawn standing there has a job using the building.

                    if( CompFacility != null )
                    {
                        foreach( var linked in CompFacility.LinkedBuildings() )
                        {
                            if( linked.def.hasInteractionCell )
                            {
                                // Look for a user at interaction cell...
                                Pawn pUser = linked.GetPawnUsing();
                                if( pUser != null )
                                {
                                    // ...A pawn is here using linked building!
                                    turnItOn = true;
                                }
                            }
                            else
                            {
                                // look for a user at any occupied cell
                                if( !linked.GetOccupyingPawnsUsing().NullOrEmpty() )
                                {
                                    // A pawn is here using linked building!
                                    turnItOn = true;
                                }
                            }
                            if( turnItOn )
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
    					while( true )
    					{

    						// Quickly check the last user is still using...
    						if(
    							( curUser != null ) &&
    							( curUser.Position == scanPosition[ 0 ] )
    						)
    						{
    							// They're here...
    							if(
    								( curUser.CurJob != null ) &&
    								( curUser.CurJob == curJob )
    							)
    							{
    								// ...they're using!
    								turnItOn = true;
    								break;
    							}
    						}

    						// Look for a new user...
                            var pUser = Find.ThingGrid.ThingsAt( scanPosition[0 ] ).FirstOrDefault( t => (
                               ( t is Pawn )&&
                               ( parent.PawnHasJobUsing( (Pawn)t ) )
                               ) ) as Pawn;
    						if( pUser != null )
    						{
    							// A pawn is here using this building!
								curUser = pUser;
								curJob = pUser.CurJob;
                                turnItOn = true;
							}
    						
    						// Exit loop
    						break;
    					}
                    }

					break;
				case LowIdleDrawMode.Cycle:
					// Power cycler

					if( LowPowerMode )
					{
						// Going to full power cycle
						turnItOn = true;
					}
					break;
				case LowIdleDrawMode.Factory:
					// Automated Factory production
					if( AutomatedFactory.CurrentRecipe != null )
					{
						turnItOn = true;
					}
					break;
				}
			}
			else
			{
				var joyGiverDefs = parent.def.GetJoyGiverDefsUsing();
				var isJoyJob = !joyGiverDefs.NullOrEmpty();

				// Full-power when any pawn is standing on any monitored cell...
				foreach( IntVec3 curPos in scanPosition )
				{
					var pawn = Find.ThingGrid.ThingAt<Pawn>( curPos );
					if( pawn != null )
					{
						if(
							( !isJoyJob )||
                            ( parent.PawnHasJobUsing( pawn ) )
						)
						{
							// Found a pawn, turn it on and early out
							turnItOn = true;
							break;
						}
					}
				}
			}

			// Do?...
			if( turnItOn )
			{
				// ...Turn on...
				if( LowPowerMode )
				{
					// ...because it is idle
					TogglePower();
				}
				else
				{
					// ..maintain the current state
					keepOnTicks = IdleProps.cycleHighTicks;
				}
			}
			else if( !LowPowerMode )
			{
				// ...Is not idle, go to idle mode
				TogglePower();
			}
			else
			{
				// ..maintain idle state
				keepOnTicks = IdleProps.cycleLowTicks;
			}

			if(
				( LowPowerMode ) &&
				( CompGlower != null )
			)
			{
				// Glower on while idle???
                ToggleGlower( false );
			}
		}

		void TogglePower()
		{
			if( LowPowerMode )
			{
				// Is idle, power up
				curPower = -CompPower.Props.basePowerConsumption;
				CompPower.PowerOutput = curPower;
				keepOnTicks = IdleProps.cycleHighTicks;
			}
			else
			{
				// Is not idle, power down
				curPower = IdlePower;
				CompPower.PowerOutput = curPower;
				keepOnTicks = IdleProps.cycleLowTicks;
				curUser = null;
				curJob = null;
			}

			// Adjust glower...
			if( CompGlower != null )
			{
				// ...if it exists
				ToggleGlower( !LowPowerMode );
			}
		}

		void ToggleGlower( bool turnItOn )
		{
			if(
				( IdleProps.operationalMode == LowIdleDrawMode.Cycle )&&
				( !turnItOn )
			)
			{
				// Cyclers don't toggle glowers, but let them turn it back on (old saves)
				return;
			}

			// Toggle and update glow grid
            var toggleableGlower = CompGlower as CompGlowerToggleable;
            if( toggleableGlower != null )
            {
                toggleableGlower.Lit = turnItOn;
            }
			CompGlower.UpdateLit();
		}

		void BuildScanList()
		{
			// Default to interaction cell only, is also means that a pawn must
			// be using the building so star-gazers aren't turning the stove on.
			onIfOn = false;

			// Power cyclers don't need to scan anything
			if( IdleProps.operationalMode == LowIdleDrawMode.Cycle )
			{
				// Set default scan tick intervals
				{
					IdleProps.cycleLowTicks = 1000;
				}

				if( IdleProps.cycleHighTicks < 0 )
				{
					IdleProps.cycleHighTicks = 500;
				}

				return;
			}
			else if( IdleProps.operationalMode == LowIdleDrawMode.Factory )
			{
				// Set default scan tick intervals
				if( IdleProps.cycleLowTicks < 0 )
				{
					IdleProps.cycleLowTicks = 30;
				}

				if( IdleProps.cycleHighTicks < 0 )
				{
					IdleProps.cycleHighTicks = 100;
				}

				return;
			}

            // If it's not a facility...
            if( CompFacility == null )
            {
                // List of cells to check
                scanPosition = new List<IntVec3>();

                // Get the map positions to monitor
    			if( parent.def.hasInteractionCell )
    			{
    				// Force-add interaction cell
    				scanPosition.Add( parent.InteractionCell );

    				if( IdleProps.operationalMode == LowIdleDrawMode.WhenNear )
    				{
    					// And the adjacent cells too
    					foreach( IntVec3 curPos in GenAdj.CellsAdjacent8Way( parent.InteractionCell ) )
    					{
    						AddScanPositionIfAllowed( curPos );
    					}
    					onIfOn = true;
    				}

    			}
    			else
    			{
    				// Pawn standing on building means we need the buildings occupancy
    				onIfOn = true;

    				if( parent.def.passability == Traversability.Standable )
    				{
    					// Add building cells if it's standable
    					foreach( IntVec3 curPos in GenAdj.CellsOccupiedBy( parent ) )
    					{
    						AddScanPositionIfAllowed( curPos );
    					}
    				}

    				if( IdleProps.operationalMode == LowIdleDrawMode.WhenNear )
    				{
    					// And the adjacent cells too???
    					foreach( var curPos in GenAdj.CellsAdjacent8Way( parent ) )
    					{
    						AddScanPositionIfAllowed( curPos );
    					}
    				}
    				if( IdleProps.operationalMode == LowIdleDrawMode.GroupUse )
    				{
    					// Group use adds cells "in front" of it
    					// Only really used by TVs
                        // WatchBuildingUtility already filters invalid cells for us
                        var cells = WatchBuildingUtility.CalculateWatchCells( parent.def, parent.Position, parent.Rotation );
    					foreach( var curPos in cells )
    					{
    						scanPosition.Add( curPos );
    					}
    				}
    			}
            }
			
            // Set default scan tick intervals
			if( IdleProps.cycleLowTicks < 0 )
			{
				IdleProps.cycleLowTicks = 30;
			}

			if( IdleProps.cycleHighTicks < 0 )
			{
				if( ( parent as Building_Door ) != null )
				{
					// Doors can be computed
					IdleProps.cycleHighTicks = ( (Building_Door)parent ).TicksToOpenNow * 3;
				}
				else
				{
					// Give work tables more time so pawns have time to fetch ingredients
					IdleProps.cycleHighTicks = 500;
				}
			}
		}

		void AddScanPositionIfAllowed( IntVec3 position )
		{
			// Look at each thing at this position and check it's passability
			foreach( Thing curThing in Find.ThingGrid.ThingsListAt( position ) )
			{
				if(
					( curThing.def.passability == Traversability.Impassable ) ||
					( curThing.def.IsEdifice() )
				)
				{
					// Impassable cell, exit right meow!
					return;
				}
			}
			// All things are passable, add cell
			scanPosition.Add( position );
		}

	}

}
