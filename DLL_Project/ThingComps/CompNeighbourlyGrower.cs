using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using CommunityCoreLibrary.Commands;

namespace CommunityCoreLibrary
{

    public class CompNeighbourlyGrower : ThingComp
    {
        #region Gizmos

        private CommandGroupOfTouchingThingsByThingComp _GizmoGroupOfThingsByThingComp;
        private CommandGroupOfTouchingThingsByThingComp GizmoGroupOfThingsByThingComp {
            get {
                if( _GizmoGroupOfThingsByThingComp == null )
                    _GizmoGroupOfThingsByThingComp = new CommandGroupOfTouchingThingsByThingComp( this.parent, this.GetType(), "CommandChangeTouchingPlantLabel".Translate(), GroupPlantChange, GroupPlantChange );
                return _GizmoGroupOfThingsByThingComp;
            }
        }

        private CommandGroupOfTouchingThingsByLinker    _GizmoGroupOfThingsByLinker;
        private CommandGroupOfTouchingThingsByLinker    GizmoGroupOfThingsByLinker {
            get {
                if( _GizmoGroupOfThingsByLinker == null )
                    _GizmoGroupOfThingsByLinker = new CommandGroupOfTouchingThingsByLinker( this.parent, GroupPlantChange, GroupPlantChange );
                return _GizmoGroupOfThingsByLinker;
            }
        }

        private CommandGroupOfDefOrThingCompInRoom  _GizmoChangeRoommatePlant;
        private CommandGroupOfDefOrThingCompInRoom  GizmoChangeRoommatePlant {
            get {
                if( _GizmoChangeRoommatePlant == null )
                {
                    _GizmoChangeRoommatePlant = new CommandGroupOfDefOrThingCompInRoom( this.parent, this.GetType(), "CommandChangeRoommatePlantLabel".Translate() );
                    _GizmoChangeRoommatePlant.ByDefLabel = Translator.Translate( "CommandChangeRoommatePlantLClick", this.parent.def.label );
                    _GizmoChangeRoommatePlant.ByDefAction = GroupPlantChange;
                    _GizmoChangeRoommatePlant.ByThingCompLabel = "CommandChangeRoommatePlantRClick".Translate();
                    _GizmoChangeRoommatePlant.ByThingCompAction = GroupPlantChange;
                    _GizmoChangeRoommatePlant.defaultDesc = _GizmoChangeRoommatePlant.Desc;
                }
                return _GizmoChangeRoommatePlant;
            }
        }

        #endregion

        #region Base ThingComp overrides

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            // In room
            if( parent.Position.IsInRoom() )
                // In room by def or comp
                if( parent.HasRoommateByThingComp( this.GetType() ) )
                    yield return GizmoChangeRoommatePlant;

            // Has a link flag
            if( parent.def.graphicData.linkFlags != LinkFlags.None ){
                        // Touching things by link
                if( parent.HasTouchingByLinker() )
                    yield return GizmoGroupOfThingsByLinker;
            }

            // In group of touching comps
            if( parent.HasTouchingByThingComp( GetType() ) )
                yield return GizmoGroupOfThingsByThingComp;


            // No more gizmos
            yield break;
        }

        #endregion

        #region Gizmo callbacks

        // The list of things are all the growers we want to change
        private void GroupPlantChange( List< Thing > things )
        {
            var thisGrower = this.parent as Building_PlantGrower;
            if( thisGrower == null )
                return;
            
            // Now set their plant
            foreach( Thing g in things )
            {
                // Should be a Building_PlantGrower
                var grower = g as Building_PlantGrower;
                if( grower != null )
                    grower.SetPlantDefToGrow( thisGrower.GetPlantDefToGrow() );
            }
        }

        #endregion
    }
}