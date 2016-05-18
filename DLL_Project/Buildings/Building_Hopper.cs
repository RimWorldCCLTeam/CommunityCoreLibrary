using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class Building_Hopper : Building, IStoreSettingsParent, ISlotGroupParent
    {

        #region Instance Data

        public SlotGroup                    slotGroup;
        public StorageSettings              settings;

        private IEnumerable<IntVec3>        cachedOccupiedCells;

        #endregion

        #region Properties

        private CompHopper                  CompHopper
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopper", this.ThingID ) );
                return GetComp<CompHopper>();
            }
        }

        #endregion

        #region IStoreSettingsParent

        public bool                         StorageTabVisible
        {
            get
            {
                //Log.Message( string.Format( "{0}.StorageTabVisible", this.ThingID ) );
                return true;
            }
        }

        public StorageSettings              GetStoreSettings()
        {
            //Log.Message( string.Format( "{0}.GetStoreSettings()", this.ThingID ) );
            return settings;
        }

        public StorageSettings              GetParentStoreSettings()
        {
            //Log.Message( string.Format( "{0}.GetParentStoreSettings( {1} )", this.ThingID, Scribe.mode.ToString() ) );
            if(
                ( Scribe.mode != LoadSaveMode.Inactive )&&
                ( Scribe.mode != LoadSaveMode.ResolvingCrossRefs )
            )
            {
                return null;
            }
            var hopperUser = CompHopper.FindHopperUser();
            if( hopperUser == null )
            {
                return null;
            }
            return hopperUser.ResourceSettings;
        }

        #endregion

        #region ISlotGroupParent

        public virtual IEnumerable<IntVec3> AllSlotCells()
        {
            //Log.Message( string.Format( "{0}.AllSlotCells()", this.ThingID ) );
            if( cachedOccupiedCells == null )
            {
                cachedOccupiedCells = this.OccupiedRect().Cells;
            }
            return cachedOccupiedCells;
        }

        public List<IntVec3>                AllSlotCellsList()
        {
            //Log.Message( string.Format( "{0}.AllSLotCellsList()", this.ThingID ) );
            return AllSlotCells().ToList();
        }

        public virtual void                 Notify_ReceivedThing( Thing newItem )
        {
            //Log.Message( string.Format( "{0}.Notify_RecievedThing( {1} )", this.ThingID, newItem == null ? "null" : newItem.ThingID ) );
        }

        public virtual void                 Notify_LostThing( Thing newItem )
        {
            //Log.Message( string.Format( "{0}.Notify_LostThing( {1} )", this.ThingID, newItem == null ? "null" : newItem.ThingID ) );
        }

        public string                       SlotYielderLabel()
        {
            //Log.Message( string.Format( "{0}.SlotYielderLabel()", this.ThingID ) );
            return LabelCap;
        }

        public SlotGroup                    GetSlotGroup()
        {
            //Log.Message( string.Format( "{0}.GetSlotGroup()", this.ThingID ) );
            return slotGroup;
        }

        #endregion

        #region Base Class Overrides

        public override void                PostMake()
        {
            //Log.Message( string.Format( "{0}.PostName()", this.ThingID ) );
            base.PostMake();
            settings = new StorageSettings((IStoreSettingsParent) this);
            if( def.building.defaultStorageSettings != null )
            {
                settings.CopyFrom( def.building.defaultStorageSettings );
            }
            //settings.filter.BlockDefaultAcceptanceFilters();
            settings.filter.ResolveReferences();
        }

        public override void                SpawnSetup()
        {
            //Log.Message( string.Format( "{0}.SpawnSetup()", this.ThingID ) );
            base.SpawnSetup();
            cachedOccupiedCells = this.OccupiedRect().Cells;
            slotGroup = new SlotGroup( (ISlotGroupParent) this );
        }

        public override void                ExposeData()
        {
            //Log.Message( string.Format( "Building_Hopper.ExposeData( {0} )", Scribe.mode.ToString() ) );
            base.ExposeData();
            Scribe_Deep.LookDeep<StorageSettings>(ref settings, "settings", new Object[1]{ this } );

            /*
            if( Scribe.mode == LoadSaveMode.ResolvingCrossRefs )
            {
                var parentSettings = GetParentStoreSettings();
                if(
                    ( settings != null )&&
                    ( parentSettings != null )
                )
                {
                    settings.Priority = parentSettings.Priority;
                }
            }
            */
            // Disallow quality
            //settings.filter.allowedQualitiesConfigurable = false;

            // Block default special filters
            //settings.filter.BlockDefaultAcceptanceFilters( GetParentStoreSettings() );
        }

        public override void                DeSpawn()
        {
            //Log.Message( string.Format( "{0}.DeSpawn()", this.ThingID ) );
            if( slotGroup != null )
            {
                slotGroup.Notify_ParentDestroying();
                slotGroup = null;
            }
            base.DeSpawn();
        }

        public override IEnumerable<Gizmo>  GetGizmos()
        {
            //Log.Message( string.Format( "{0}.GetGizmos()", this.ThingID ) );
            var copyPasteGizmos = StorageSettingsClipboard.CopyPasteGizmosFor( settings );
            foreach( var gizmo in copyPasteGizmos )
            {
                yield return gizmo;
            }
            foreach( var gizmo in base.GetGizmos() )
            {
                yield return gizmo;
            }
        }

        #endregion

    }

}
