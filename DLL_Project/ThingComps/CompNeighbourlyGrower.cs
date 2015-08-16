using System.Collections.Generic;

using RimWorld;
using Verse;
using CommunityCoreLibrary.Commands;

namespace CommunityCoreLibrary
{

    public class CompNeighbourlyGrower : ThingComp
    {

        Building_PlantGrower                Building_PlantGrower
        {
            get
            {
                return parent as Building_PlantGrower;
            }
        }

        #region Gizmos

        TouchingByThingComp                 _GizmoTouchingByThingComp;
        TouchingByThingComp                 GizmoTouchingByThingComp
        {
            get
            {
                if( _GizmoTouchingByThingComp == null )
                {
                    _GizmoTouchingByThingComp =
                        new TouchingByThingComp(
                            parent,
                            GetType(),
                            "CommandChangeTouchingPlantLabel".Translate(),
                            GroupPlantChange,
                            GroupPlantChange
                        );
                }
                return _GizmoTouchingByThingComp;
            }
        }

        TouchingByLinker                    _GizmoTouchingByLinker;
        TouchingByLinker                    GizmoTouchingByLinker
        {
            get
            {
                if( _GizmoTouchingByLinker == null )
                {
                    _GizmoTouchingByLinker =
                        new TouchingByLinker(
                            parent,
                            GroupPlantChange,
                            GroupPlantChange
                        );
                }
                return _GizmoTouchingByLinker;
            }
        }

        DefOrThingCompInRoom                _GizmoDefOrThingCompInRoom;
        DefOrThingCompInRoom                GizmoDefOrThingCompInRoom
        {
            get
            {
                if( _GizmoDefOrThingCompInRoom == null )
                {
                    _GizmoDefOrThingCompInRoom =
                        new DefOrThingCompInRoom(
                            parent,
                            GetType(),
                            "CommandChangeRoommatePlantLabel".Translate()
                        );
                    _GizmoDefOrThingCompInRoom.LabelByDef = "CommandChangeRoommatePlantLClick".Translate( parent.def.label );
                    _GizmoDefOrThingCompInRoom.ClickByDef = GroupPlantChange;
                    _GizmoDefOrThingCompInRoom.LabelByThingComp = "CommandChangeRoommatePlantRClick".Translate();
                    _GizmoDefOrThingCompInRoom.ClickByThingComp = GroupPlantChange;
                    _GizmoDefOrThingCompInRoom.defaultDesc = _GizmoDefOrThingCompInRoom.Desc;
                }
                return _GizmoDefOrThingCompInRoom;
            }
        }

        #endregion

        #region Base ThingComp overrides

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if( ( parent.Position.IsInRoom() )&&
                ( parent.IsSameThingCompInRoom( GetType() ) ) )
            {
                // In room by def or comp
                yield return GizmoDefOrThingCompInRoom;
            }

            // Has a link flag
            if( ( parent.def.graphicData.linkFlags != LinkFlags.None )&&
                ( parent.IsSameGraphicLinkerTouching() ) )
            {
                // Touching things by link
                yield return GizmoTouchingByLinker;
            }

            // In group of touching comps
            if( parent.IsSameThingCompTouching( GetType() ) )
            {
                yield return GizmoTouchingByThingComp;
            }

            // No more gizmos
            yield break;
        }

        #endregion

        #region Gizmo callbacks

        // The list of things are all the growers we want to change
        void                                GroupPlantChange( List< Thing > things )
        {
            var thisGrower = Building_PlantGrower;
#if DEBUG
            if( thisGrower == null )
            {
                Log.Error( "Community Core Library :: CompNeighbourlyGrower :: " + parent.def.defName + " unable to resolve to Building_PlantGrower!" );
                return;
            }
#endif
            
            // Now set their plant
            foreach( Thing g in things )
            {
                // Should be a Building_PlantGrower
                var grower = g as Building_PlantGrower;
#if DEBUG
                if( grower == null )
                {
                    Log.Error( "Community Core Library :: CompNeighbourlyGrower :: " + g.def.defName + " unable to resolve to Building_PlantGrower!" );
                    return;
                }
#endif
                grower.SetPlantDefToGrow( thisGrower.GetPlantDefToGrow() );
            }
        }

        #endregion

    }

}
