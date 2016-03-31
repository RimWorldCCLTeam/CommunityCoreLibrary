using System;
using System.Collections;
using System.Collections.Generic;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class CompWatchBuildingParticipantCellDisplay : ThingComp
    {
        // This adds a gizmo for joy giver buildings to display the participant cells

        public bool                         showCells = false;

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            var command = new Command_Toggle();
            command.defaultDesc = "Hide or show participant area.";
            if( showCells )
            {
                command.defaultLabel = "Hide area";
            }
            else
            {
                command.defaultLabel = "Show area";
            }
            command.icon = ContentFinder<Texture2D>.Get( "UI/Icons/Commands/ToggleParticipantCells", true );
            command.isActive = () =>
            {
                return showCells;
            };
            command.toggleAction = () =>
            {
                showCells = !showCells;
            };
            yield return command;
            yield break;
        }

        public override void                PostDraw()
        {
            if( showCells )
            {
                var cells = parent.GetParticipantCells( false );
                GenDraw.DrawFieldEdges( cells, Color.white );
                cells = parent.GetParticipantCells( true );
                GenDraw.DrawFieldEdges( cells, Color.yellow );
            }
        }

    }

}
