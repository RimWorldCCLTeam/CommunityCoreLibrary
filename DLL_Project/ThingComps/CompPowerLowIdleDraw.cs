using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class CompPowerLowIdleDraw : ThingComp
    {

        [Unsaved]

        #region Instance Data

        CompPowerTrader                     PowerTrader;
        List<IntVec3>                       scanPosition;
        Pawn                                curUser;
        Job                                 curJob;
        // minIdleDraw is to prevent idlePower from being 0.0f
        // This is important so that the device stays connected to the
        // power net without actually drawing off of it when not in use.
        public const float                  MinIdleDraw = -0.01f;
        public float                        IdlePower;
        float                               curPower = 1f;
        bool                                onIfOn;

        int                                 keepOnTicks;

        public CompProperties_LowIdleDraw   IdleProps;

        #endregion

        #region Comp Getters

        CompGlower                          CompGlower
        {
            get
            {
                return parent.TryGetComp< CompGlower >();
            }
        }

        CompPowerTrader                     CompPowerTrader
        {
            get
            {
                return parent.TryGetComp< CompPowerTrader >();
            }
        }

        Building_AutomatedFactory           AutomatedFactory
        {
            get
            {
                return parent as Building_AutomatedFactory;
            }
        }

        #endregion

        #region Query State

        public bool                         LowPowerMode
        {
            get
            {
                return !( PowerTrader.PowerOutput < IdlePower );
            }
        }

        #endregion

        public override void                PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue<int>( ref keepOnTicks, "keepOnTicks", 30 );
            Scribe_Values.LookValue<float>( ref curPower, "curPower", 1f );
            Scribe_References.LookReference<Pawn>( ref curUser, "curUser" );

            if(
                ( Scribe.mode == LoadSaveMode.LoadingVars )&&
                ( curUser != null )
            )
            {
                curJob = curUser.CurJob;
            }
        }

        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();

            // Get the power comp
            PowerTrader = CompPowerTrader;
#if DEBUG
            if( PowerTrader == null )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompPowerTrader",
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
                ( !parent.def.hasInteractionCell )
            )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Parent building must be have an interaction cell to use 'InUse'",
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
#endif
            
            // Generate the list of cells to check
            BuildScanList();

            // Calculate low-power mode consumption
            IdlePower = IdleProps.idlePowerFactor * -PowerTrader.props.basePowerConsumption;
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
            PowerTrader.PowerOutput = curPower;
        }

        public override void                CompTick()
        {
            base.CompTick();

            if( !CompPowerTrader.PowerOn )
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

        public override void                CompTickRare()
        {
            base.CompTickRare();

            if( !CompPowerTrader.PowerOn )
            {
                return;
            }

            // keepOnTicks -= 250;
            PowerLevelToggle( 250 );
        }

        public override void                ReceiveCompSignal( string signal )
        {
            // Asked to power down?
            if(
                ( signal == "PowerTurnedOff" )||
                ( signal == "PowerTurnedOn " )
            )
            {
                // Force toggle now
                PowerLevelToggle( 1000000 );
            }
        }

        void                                PowerLevelToggle( int thisTickCount )
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

                switch( IdleProps.operationalMode ){
                case LowIdleDrawMode.InUse :
                    // This building has an interaction cell, this means it has
                    // jobs associated with it.  Only go to full-power if the
                    // pawn standing there has a job using the building.

                    // Break out of a loop for optimization because for some
                    // reason using gotos is considered evil, even though
                    // having an machine language background I think people
                    // who think this way are just insisting a "potatoe"
                    // is a "potato"
                    while( true )
                    {

                        // Quickly check the last user is still using...
                        if(
                            ( curUser != null )&&
                            ( curUser.Position == scanPosition[0] )
                        )
                        {
                            // They're here...
                            if(
                                ( curUser.CurJob != null )&&
                                ( curUser.CurJob == curJob )
                            )
                            {
                                // ...they're using!
                                turnItOn = true;
                                break;
                            }
                        }

                        // Look for a new user...

                        Pawn pUser = Find.ThingGrid.ThingAt<Pawn>( scanPosition[0] );
                        if( pUser != null )
                        {
                            // ...A pawn is here!...

                            if( pUser.CurJob != null )
                            {
                                // ...With a job!...

                                // ...(checking targets)...
                                if(
                                    ( pUser.CurJob.targetA != null )&&
                                    ( pUser.CurJob.targetA.Thing != null )&&
                                    ( pUser.CurJob.targetA.Thing.def == parent.def )
                                )
                                {
                                    turnItOn = true;
                                }
                                else if(
                                    ( pUser.CurJob.targetB != null )&&
                                    ( pUser.CurJob.targetB.Thing != null )&&
                                    ( pUser.CurJob.targetB.Thing.def == parent.def )
                                )
                                {
                                    turnItOn = true;
                                }
                                else if(
                                    ( pUser.CurJob.targetC != null )&&
                                    ( pUser.CurJob.targetC.Thing != null )&&
                                    ( pUser.CurJob.targetC.Thing.def == parent.def )
                                )
                                {
                                    turnItOn = true;
                                }

                                if( turnItOn )
                                {
                                    // ..Using this building!...
                                    curUser = pUser;
                                    curJob = pUser.CurJob;
                                }
                            }
                        }

                        // Exit loop
                        break;
                    }

                    break;
                case LowIdleDrawMode.Cycle :
                    // Power cycler

                    if( LowPowerMode )
                    {
                        // Going to full power cycle
                        turnItOn = true;
                    }
                    break;
                case LowIdleDrawMode.Factory :
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
                // Full-power when any pawn is standing on any monitored cell...
                foreach( IntVec3 curPos in scanPosition )
                {
                    if( Find.ThingGrid.ThingAt<Pawn>( curPos ) != null )
                    {
                        // Found a pawn, turn it on and early out
                        turnItOn = true;
                        break;
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
                ( LowPowerMode )&&
                ( CompGlower != null )&&
                ( CompGlower.Lit )
            )
            {
                // Glower on while idle???
                ToggleGlower( false );
            }
        }

        void                                TogglePower()
        {
            if( LowPowerMode )
            {
                // Is idle, power up
                curPower = -PowerTrader.props.basePowerConsumption;
                PowerTrader.PowerOutput = curPower;
                keepOnTicks = IdleProps.cycleHighTicks;
            }
            else
            {
                // Is not idle, power down
                curPower = IdlePower;
                PowerTrader.PowerOutput = curPower;
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

        void                                ToggleGlower( bool turnItOn )
        {
            if(
                ( IdleProps.operationalMode == LowIdleDrawMode.Cycle )&&
                ( !turnItOn )
            )
            {
                // Cyclers don't toggle glowers, but let them turn it back on (old saves)
                return;
            }

            // If no state change, abort
            if( turnItOn == CompGlower.Lit )
            {
                return;
            }

            // Toggle and update glow grid
            CompGlower.Lit = turnItOn;
            Find.GlowGrid.MarkGlowGridDirty( parent.Position );
            Find.MapDrawer.MapMeshDirty( parent.Position, MapMeshFlag.GroundGlow );
            Find.MapDrawer.MapMeshDirty( parent.Position, MapMeshFlag.Things );
        }

        void                                BuildScanList()
        {
            // Default to interaction cell only, is also means that a pawn must
            // be using the building so star-gazers aren't turning the stove on.
            onIfOn = false;

            // Power cyclers don't need to scan anything
            if( IdleProps.operationalMode == LowIdleDrawMode.Cycle )
            {
                // Set default scan tick intervals
                if( IdleProps.cycleLowTicks < 0 )
                {
                    IdleProps.cycleLowTicks = 1000;
                }

                if( IdleProps.cycleHighTicks < 0 )
                {
                    IdleProps.cycleHighTicks = 500;
                }

                return;
            }
            else if ( IdleProps.operationalMode == LowIdleDrawMode.Factory )
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

            // List of cells to check
            scanPosition = new List<IntVec3>();

            // Get the map positions to monitor
            if( parent.def.hasInteractionCell )
            {
                // Force-add interaction cell
                scanPosition.Add( parent.InteractionCell );

                switch( IdleProps.operationalMode )
                {
                case LowIdleDrawMode.WhenNear :
                    // And the adjacent cells too
                    foreach( IntVec3 curPos in GenAdj.CellsAdjacent8Way( parent.InteractionCell ) )
                    {
                        AddScanPositionIfAllowed( curPos );
                    }
                    onIfOn = true;
                    break;
                case LowIdleDrawMode.GroupUser :
                    // Group user adds cells "in front" of it
                    // Only really used by TVs
                    // TODO:  Make this actually do something!
                    /*
                     * if( ( parent.def == ThingDefOf.Television )
                        ||( parent.def == ThingDefOf.TelevisionLED ) ){

                        foreach( IntVec3 curPos in GenAdj.CellsAdjacent8Way( parent.InteractionCell ) )
                        {
                            AddScanPositionIfAllowed( curPos );
                        }
                    }
                    */
                    break;
                }

            }
            else
            {
                // Pawn standing on building means we need the buildings occupancy
                onIfOn = true;

                foreach( IntVec3 curPos in GenAdj.CellsOccupiedBy( parent ) )
                {
                    AddScanPositionIfAllowed( curPos );
                }

                if( IdleProps.operationalMode == LowIdleDrawMode.WhenNear )
                {
                    // And the adjacent cells too???
                    foreach( var curPos in GenAdj.CellsAdjacent8Way( parent ) )
                    {
                        AddScanPositionIfAllowed( curPos );
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

        void                                AddScanPositionIfAllowed( IntVec3 position )
        {
            // Look at each thing at this position and check it's passability
            foreach( Thing curThing in Find.ThingGrid.ThingsListAt( position ) )
            {
                if(
                    ( curThing.def.passability == Traversability.Impassable )||
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
