using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompHopper : ThingComp
    {

        #region Instance Data

        private bool                        wasProgrammed;

        private CompHopperUser              hopperUser;

        #endregion

        #region Properties

        public Building_Hopper              Building
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopper.Building", this.parent.ThingID ) );
                return parent as Building_Hopper;
            }
        }

        public bool                         WasProgrammed
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopper.WasProgrammed", this.parent.ThingID ) );
                return wasProgrammed;
            }
            set
            {
                //Log.Message( string.Format( "{0}.CompHopper.WasProgrammed = {1}", this.parent.ThingID, value ) );
                wasProgrammed = value;
            }
        }

        public List<ThingDef>               ResourceDefs
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopper.ResourceDefs", this.parent.ThingID ) );
                return Building.GetStoreSettings().filter.AllowedThingDefs.ToList();
            }
        }

        public bool                         IsRefrigerated
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopper.IsRefrigerated", this.parent.ThingID ) );
                return ( Building.TryGetComp<CompRefrigerated>() != null );
            }
        }

        #endregion

        #region Base Class Overrides

        public override void                PostSpawnSetup()
        {
            //Log.Message( string.Format( "{0}.CompHopper.PostSpawnSetup()", this.parent.ThingID ) );
            base.PostSpawnSetup();
            hopperUser = FindHopperUser();
            if(
                ( !WasProgrammed )&&
                ( hopperUser != null )
            )
            {
                hopperUser.NotifyHopperAttached();
            }
        }

        public override void                PostExposeData()
        {
            //Log.Message( string.Format( "CompHopper.PostExposeData( {0} )", Scribe.mode.ToString() ) );
            base.PostExposeData();
            Scribe_Values.LookValue( ref wasProgrammed, "wasProgrammed", false );
        }

        public override void                PostDeSpawn()
        {
            //Log.Message( string.Format( "{0}.CompHopper.PostDeSpawn()", this.parent.ThingID ) );
            base.PostDeSpawn();
            DeprogramHopper();
            if( hopperUser != null )
            {
                hopperUser.NotifyHopperDetached();
                hopperUser = null;
            }
        }

        #endregion

        #region Hopper Programming

        public void                         DeprogramHopper()
        {
            //Log.Message( string.Format( "{0}.CompHopper.DeprogramHopper()", this.parent.ThingID ) );
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
            hopperSettings.filter = new ThingFilter();

            // Reset the flag
            WasProgrammed = false;
        }

        public void                         ProgramHopper( StorageSettings HopperUserSettings )
        {
            //Log.Message( string.Format( "{0}.CompHopper.ProgramHopper( StorageSettings )", this.parent.ThingID ) );
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

        #endregion

        #region Resource Enumeration

        public StorageSettings              GetStoreSettings()
        {
            //Log.Message( string.Format( "{0}.CompHopper.GetStoreSettings()", this.parent.ThingID ) );
            return Building.GetStoreSettings();
        }

        public Thing                        GetResource( ThingFilter acceptableResources )
        {
            //Log.Message( string.Format( "{0}.CompHopper.GetResource( ThingFilter )", this.parent.ThingID ) );
            var things = GetAllResources( acceptableResources );
            if( things.NullOrEmpty() )
            {
                return null;
            }
            return things.FirstOrDefault();
        }

        public Thing                        GetResource( ThingDef resourceDef )
        {
            //Log.Message( string.Format( "{0}.CompHopper.GetResource( {1} )", this.parent.ThingID, resourceDef == null ? "null" : resourceDef.defName ) );
            var things = GetAllResources( resourceDef );
            if( things.NullOrEmpty() )
            {
                return null;
            }
            return things.FirstOrDefault();
        }

        public List< Thing >                GetAllResources( ThingFilter acceptableResources )
        {
            //Log.Message( string.Format( "{0}.CompHopper.GetAllResources( ThingFilter )", this.parent.ThingID ) );
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
            //Log.Message( string.Format( "{0}.CompHopper.GetAllResources( {1} )", this.parent.ThingID, resourceDef == null ? "null" : resourceDef.defName ) );
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

        #endregion

        #region Hopper User Enumeration

        public CompHopperUser               FindHopperUser()
        {
            //Log.Message( string.Format( "{0}.CompHopper.FindHopperUser()", this.parent.ThingID ) );
            return FindHopperUser( parent.Position + parent.Rotation.FacingCell );
        }

        public static CompHopperUser        FindHopperUser( IntVec3 cell )
        {
            //var str = string.Format( "CompHopper.FindHopperUser( {0} )", cell.ToString() );
            if( !cell.InBounds() )
            {
                //Log.Message( str );
                return null;
            }
            List<Thing> thingList = null;
            if( Scribe.mode != LoadSaveMode.Inactive )
            {   // Find hopper user in world matching cell
                if(
                    ( Find.ThingGrid == null )||
                    ( Find.ThingGrid.ThingsAt( cell ).Count() == 0 )
                )
                {
                    //Log.Message( str );
                    return null;
                }
                thingList = Find.ThingGrid.ThingsAt( cell ).ToList();
            }
            else
            {   // Find hopper user in cell
                thingList = cell.GetThingList();
            }
            if( !thingList.NullOrEmpty() )
            {
                var hopperUser = thingList.FirstOrDefault( (thing) =>
                {
                    var thingDef = GenSpawn.BuiltDefOf( thing.def ) as ThingDef;
                    return ( thingDef != null )&&( thingDef.IsHopperUser() );
                } );
                if( hopperUser != null )
                {   // Found a hopper user
                    //str += " = " + hopperUser.ThingID;
                    //Log.Message( str );
                    return hopperUser.TryGetComp<CompHopperUser>();
                }
            }
            // Nothing found
            //Log.Message( str );
            return null;
        }

        #endregion

    }

}
