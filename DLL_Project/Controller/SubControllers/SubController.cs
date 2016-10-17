using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary.Controller
{

    public enum SubControllerState
    {
        Uninitialized = 0,
        ValidationError = -100,
        InitializationError = -101,
        RuntimeError = -102,
        Validated = 1,
        Ok = 100,
        Hybernating = 101,
        _BaseError = ValidationError,
        _BaseOk = Ok
    }

    internal abstract class SubController
    {

        public const int                    DontProcessThisPhase = -99999;

        // Controller name
        public abstract string              Name { get; }

        // Return message from method
        public string                       strReturn;

        // Current controller state
        public SubControllerState           State;

        // Object hash
        internal int                        Hash { get; private set; }

        // Akin to Thing.IsHashTickInterval
        internal bool                       IsHashIntervalTick ( int interval )
        {
            return ( ( ( Hash + interval ) % UpdateTicks ) == 0 );
        }

        // Start in an uninitialized state
        internal                            SubController ()
        {
            strReturn = string.Empty;
            State = SubControllerState.Uninitialized;
            Hash = this.GetHashCode() & 0xFFFFF;
        }

        // Get sequence priorities
        // Default class property for sub-classes which don't need to do this
        public virtual int                  ValidationPriority      => DontProcessThisPhase;
        public virtual int                  InitializationPriority  => DontProcessThisPhase;
        public virtual int                  UpdatePriority          => DontProcessThisPhase;

        // Default update rate is every 30 ticks
        public virtual int                  UpdateTicks             => 30;

        // If the controller Initialize() method should be called on game load return true
        public virtual bool                 ReinitializeOnGameLoad  => false;

        // Entry with controller state Uninitialized
        // Exit with ValidationError (false) on error
        // Validated (true) if ready for initialization
        public virtual bool                 Validate()
        {
            // Default class method for sub-classes which don't require validation
            strReturn = string.Empty;
            State = SubControllerState.Validated;
            return true;
        }

        // Entry with controller state Validated or in a running state after a game load
        // Exit with InitializationError (false) on error
        // Ok (true) if ready for game play
        // Hybernating (true) if system is ok and no frame updates are required
        public virtual bool                 Initialize()
        {
            // Default class method for sub-classes which don't require initialization
            strReturn = string.Empty;
            State = SubControllerState.Ok;
            return true;
        }

        // Entry with controller state Ok
        // Executes every frame during game play
        // Exit with RuntimeError (false) on error
        // Ok (true) if ready for game play
        // Hybernating (true) if system is ok and no further frame updates are required
        public virtual bool                 Update()
        {
            // Default class method for sub-classes which don't require updating
            strReturn = string.Empty;
            State = SubControllerState.Hybernating;
            return true;
        }

    }

}
