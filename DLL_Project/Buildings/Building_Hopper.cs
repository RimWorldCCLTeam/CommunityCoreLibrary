using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    // TODO: reimplement IStoreSettingsParent, ISlotGroupParent when file is fixed
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
                return GetComp<CompHopper>();
            }
        }

        #endregion

        #region IStoreSettingsParent

        public bool                         StorageTabVisible
        {
            get
            {
                return true;
            }
        }

        public StorageSettings              GetStoreSettings()
        {
            return settings;
        }

        public StorageSettings              GetParentStoreSettings()
        {
            var hopperUser = CompHopper.FindHopperUser();
            if( hopperUser == null )
            {
                return null;
            }
            var compHopperUser = hopperUser.TryGetComp<CompHopperUser>();
            if( compHopperUser == null )
            {
                return null;
            }
            return compHopperUser.ResourceSettings;
        }

        #endregion

        #region ISlotGroupParent

        public virtual IEnumerable<IntVec3> AllSlotCells()
        {
            if( cachedOccupiedCells == null )
            {
                cachedOccupiedCells = this.OccupiedRect().Cells;
            }
            return cachedOccupiedCells;
        }

        public List<IntVec3>                AllSlotCellsList()
        {
            return AllSlotCells().ToList();
        }

        public virtual void                 Notify_ReceivedThing(Thing newItem)
        {
        }

        public virtual void                 Notify_LostThing(Thing newItem)
        {
        }

        public string                       SlotYielderLabel()
        {
            return LabelCap;
        }

        public SlotGroup                    GetSlotGroup()
        {
            return slotGroup;
        }

        #endregion

        #region Base Class Overrides

        public override void                PostMake()
        {
            base.PostMake();
            settings = new StorageSettings((IStoreSettingsParent) this);
            if( def.building.defaultStorageSettings != null )
            {
                settings.CopyFrom( def.building.defaultStorageSettings );
            }
            // TODO:  A12->A13 Valididity Check
            //settings.filter.BlockDefaultAcceptanceFilters();
            settings.filter.ResolveReferences();
        }

        public override void                SpawnSetup()
        {
            base.SpawnSetup();
            slotGroup = new SlotGroup( (ISlotGroupParent) this );
            cachedOccupiedCells = this.OccupiedRect().Cells;
        }

        public override void                ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.LookDeep<StorageSettings>(ref settings, "settings", new Object[1]{ this } );

            // Disallow quality
            //settings.filter.allowedQualitiesConfigurable = false;

            // Block default special filters
            //settings.filter.BlockDefaultAcceptanceFilters( GetParentStoreSettings() );
        }

        public override void                Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if( slotGroup != null )
            {
                slotGroup.Notify_ParentDestroying();
            }
            base.Destroy( mode );
        }

        public override IEnumerable<Gizmo>  GetGizmos()
        {
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
