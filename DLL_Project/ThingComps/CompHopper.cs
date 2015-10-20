using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompHopper : ThingComp
    {

        private bool                        wasProgrammed;

        public Building_Hopper              Building
        {
            get
            {
                return parent as Building_Hopper;
            }
        }

        public bool                         WasProgrammed
        {
            get
            {
                return wasProgrammed;
            }
            set
            {
                wasProgrammed = value;
            }
        }

        public List<ThingDef>               ResourceDefs
        {
            get
            {
                return Building.GetStoreSettings().filter.AllowedThingDefs.ToList();
            }
        }

        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();
            var userSettings = Building.GetParentStoreSettings();
            if( userSettings == null )
            {
                return;
            }
            ProgramHopper( userSettings );
        }

        public override void                PostExposeData()
        {
            Scribe_Values.LookValue( ref wasProgrammed, "wasProgrammed", false );
        }

        public override void                PostDeSpawn()
        {
            base.PostDeSpawn();
            DeprogramHopper();
        }

        public void                         DeprogramHopper()
        {
            if( !WasProgrammed )
            {
                return;
            }
            var hopperSettings = Building.GetStoreSettings();
            if( hopperSettings == null )
            {
                // No storage settings
                return;
            }

            // Clear the programming
            hopperSettings.filter.SetDisallowAll();

            // Reset the flag
            WasProgrammed = false;
        }

        public void                         ProgramHopper( StorageSettings HopperUserSettings )
        {
            if(
                ( WasProgrammed )||
                ( Building == null )||
                ( HopperUserSettings == null )
            )
            {
                // Already programmed or not a valid hopper
                return;
            }

            var hopperSettings = Building.GetStoreSettings();
            if( hopperSettings == null )
            {
                // No storage settings
                return;
            }

            // Copy the settings from the controller
            hopperSettings.CopyFrom( HopperUserSettings );

            // Set the programming flag
            WasProgrammed = true;
        }

        public Thing                        FindHopperUser()
        {
            return FindHopperUser( parent.Position + parent.Rotation.FacingCell );
        }

        public Thing                        GetResource( ThingFilter acceptableResources )
        {
            var things = GetAllResources( acceptableResources );
            if( things.NullOrEmpty() )
            {
                return null;
            }
            return things.FirstOrDefault();
        }

        public Thing                        GetResource( ThingDef resourceDef )
        {
            var things = GetAllResources( resourceDef );
            if( things.NullOrEmpty() )
            {
                return null;
            }
            return things.FirstOrDefault();
        }

        public List< Thing >                GetAllResources( ThingFilter acceptableResources )
        {
            var things = parent.Position.GetThingList().Where( t => (
                ( acceptableResources.Allows( t.def ) )
            ) ).ToList();
            if( things.NullOrEmpty() )
            {
                return null;
            }

            things.Sort( ( Thing x, Thing y ) => ( x.stackCount > y.stackCount ) ? -1 : 1 );

            // Return sorted by quantity list of things
            return things;
        }

        public List< Thing >                GetAllResources( ThingDef resourceDef )
        {
            var things = parent.Position.GetThingList().Where( t => (
                ( resourceDef == t.def )
            ) ).ToList();
            if( things.NullOrEmpty() )
            {
                return null;
            }

            things.Sort( ( Thing x, Thing y ) => ( x.stackCount > y.stackCount ) ? -1 : 1 );

            // Return sorted by quantity list of things
            return things;
        }

        public static Thing                 FindHopperUser( IntVec3 cell )
        {
            if( !cell.InBounds() )
            {
                // Out of bounds
                return null;
            }
            var thingList = cell.GetThingList();
            foreach( var thing in thingList )
            {
                var thingDef = GenSpawn.BuiltDefOf( thing.def ) as ThingDef;
                if(
                    ( thingDef != null )&&
                    ( thingDef.IsHopperUser() )
                )
                {
                    // This thing wants a hopper
                    return thing;
                }
            }

            // Nothing found
            return null;
        }

    }

}
