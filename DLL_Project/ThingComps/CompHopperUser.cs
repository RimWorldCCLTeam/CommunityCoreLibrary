using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class CompHopperUser : ThingComp
    {
        
		private StorageSettings				resourceSettings;
		private ThingFilter					xmlResources;

        private List<IntVec3>				cachedAdjCellsCardinal;
        private List<IntVec3>				AdjCellsCardinalInBounds
        {
            get
            {
                if (this.cachedAdjCellsCardinal == null)
                {
                    this.cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal(this.parent).Where(c => c.InBounds()).ToList();
                }
                return this.cachedAdjCellsCardinal;
            }
        }

		public StorageSettings				ResourceSettings
		{
			get
			{
				if( resourceSettings == null )
				{
					var iHopperUser = parent as IHopperUser;
					if(
						( Resources == null )&&
						(
							( iHopperUser == null )||
							( iHopperUser.ResourceFilter == null )
						)
					)
					{
						// Does not contain xml resource filter
						// or (properly) implement IHopperUser
						Log.Message( parent.def.defName + " Improperly configured!" );
						return null;
					}

					// Create storage settings
					resourceSettings = new StorageSettings();

					// Set priority
					resourceSettings.Priority = StoragePriority.Important;

					// Create a new filter
					resourceSettings.filter = new ThingFilter();

					// Set the filter from the hopper user
					if( iHopperUser != null )
					{
						// Copy a filter from a building implementing IHopperUser
						resourceSettings.filter.CopyFrom( iHopperUser.ResourceFilter );
					}
					else
					{
						// Copy a filter from xml flag
						resourceSettings.filter.CopyFrom( Resources );
					}

					// Block default special filters
					resourceSettings.filter.BlockDefaultAcceptanceFilters();
                    
                    // Resolve references again
                    resourceSettings.filter.ResolveReferences();

					// Disallow quality
					resourceSettings.filter.allowedQualitiesConfigurable = false;

				}
				return resourceSettings;
			}
		}

        public ThingFilter					Resources
        {
			get
			{
				if( xmlResources == null )
				{
					xmlResources = ((CompProperties_HopperUser)props).resources;
					if( xmlResources != null )
					{
						xmlResources.ResolveReferences();
					}
				}
				return xmlResources;
			}
        }

        public override void				PostSpawnSetup()
        {
            base.PostSpawnSetup();

			if( ResourceSettings != null )
			{
				FindAndProgramHoppers();
			}
		}

		public override void				PostDeSpawn()
		{
			base.PostDeSpawn();

			// Scan for hoppers and deprogram each one
			var hoppers = FindHoppers();
			if (!hoppers.NullOrEmpty())
			{
				foreach( var hopper in hoppers )
				{
					hopper.DeprogramHopper();
				}
			}
		}
		
        public void							FindAndProgramHoppers()
        {
			// Now scan for hoppers and program each one
            var hoppers = FindHoppers();
            if (!hoppers.NullOrEmpty())
            {
                foreach (var hopper in hoppers)
                {
                    hopper.ProgramHopper( ResourceSettings );
                }
            }
        }

        public List<CompHopper>             FindHoppers()
        {
            // Find hoppers for building
            var hoppers = new List<CompHopper>();
            var occupiedCells = this.parent.OccupiedRect();
            foreach (var cell in AdjCellsCardinalInBounds)
            {
				var hopper = FindHopper( cell );
                if (
                    ( hopper != null )&&
                    ( occupiedCells.Cells.Contains( hopper.Building.Position + hopper.Building.Rotation.FacingCell ) )
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add(hopper);
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }

        public static List<CompHopper>		FindHoppers( IntVec3 thingCenter, Rot4 thingRot, IntVec2 thingSize )
        {
            // Find hoppers for near cell
            var hoppers = new List<CompHopper>();
            var occupiedCells = GenAdj.OccupiedRect( thingCenter, thingRot, thingSize );
            foreach( var cell in GenAdj.CellsAdjacentCardinal( thingCenter, thingRot, thingSize ).
                Where( c => c.InBounds() ).ToList() )
            {
				var hopper = FindHopper( cell );
                if (
                    ( hopper != null ) &&
                    ( occupiedCells.Cells.Contains( hopper.Building.Position + hopper.Building.Rotation.FacingCell ) )
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add(hopper);
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }

		private static CompHopper			FindHopper( IntVec3 cell )
		{
			// Find hopper in cell
			var thingList = cell.GetThingList();
			foreach( var thing in thingList )
			{
				var thingDef = GenSpawn.BuiltDefOf( thing.def ) as ThingDef;
				if(
					( thingDef != null )&&
					( thingDef.IsHopper() )
				)
				{
					// This thing is a hopper
					return thing.TryGetComp< CompHopper >();
				}
			}
			// No hopper found
			return null;
		}

		public CompHopper					FindBestHopperForResources()
		{
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return null;
			}
			var bestHopper = (CompHopper)null;
			var bestResource = (Thing)null;
			foreach( var hopper in hoppers )
			{
				// Find best in hopper
				var hopperResources = hopper.GetAllResources( ResourceSettings.filter );
				foreach( var resource in hopperResources )
				{
					if( resource != null )
					{
						if(
							( bestHopper == null )||
							( resource.stackCount > bestResource.stackCount )
						)
						{
							// First resource or this hopper holds more
							bestHopper = hopper;
							bestResource = resource;
						}
					}
				}
			}
			// Return the best hopper
			return bestHopper;
		}

		public bool							RemoveResourceFromHoppers( ThingDef resourceDef, int resourceCount )
		{
			if( !EnoughResourceInHoppers( resourceDef, resourceCount ) )
			{
				return false;
			}
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return false;
			}

			foreach( var hopper in hoppers )
			{
				var resource = hopper.GetResource( resourceDef );
				if( resource!= null )
				{
					if( resource.stackCount >= resourceCount )
					{
						resource.SplitOff( resourceCount );
						return true;
					}
					else
					{
						resourceCount -= resource.stackCount;
						resource.SplitOff( resource.stackCount );
					}
				}
			}
			// Should always be true...
			return ( resourceCount <= 0 );
		}

		public bool							RemoveResourcesFromHoppers( int resourceCount )
		{
			if( !EnoughResourcesInHoppers( resourceCount ) )
			{
				return false;
			}
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return false;
			}

			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( ResourceSettings.filter );
				if( !resources.NullOrEmpty() )
				{
					foreach( var resource in resources )
					{
						if( resource.stackCount >= resourceCount )
						{
							resource.SplitOff( resourceCount );
							return true;
						}
						else
						{
							resourceCount -= resource.stackCount;
							resource.SplitOff( resource.stackCount );
						}
					}
				}
			}
			// Should always be true...
			return ( resourceCount <= 0 );
		}

		public bool							EnoughResourceInHoppers( ThingDef resourceDef, int resourceCount )
		{
			return ( CountResourceInHoppers( resourceDef ) >= resourceCount );
		}

		public bool							EnoughResourcesInHoppers( int resourceCount )
		{
			return ( CountResourcesInHoppers() >= resourceCount );
		}

		public int							CountResourceInHoppers( ThingDef resourceDef )
		{
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return 0;
			}

			int availableResources = 0;
			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( resourceDef );
				if( !resources.NullOrEmpty() )
				{
					foreach( var resource in resources )
					{
						availableResources += resource.stackCount;
					}
				}
			}
			return availableResources;
		}

		public int							CountResourcesInHoppers()
		{
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return 0;
			}

			int availableResources = 0;
			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( ResourceSettings.filter );
				if( !resources.NullOrEmpty() )
				{
					foreach( var resource in resources )
					{
						availableResources += resource.stackCount;
					}
				}
			}
			return availableResources;
		}

    }

}
