using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Commands
{

    public class CommandGroupOfDefOrThingCompInRoom : Command
    {
        private Type            parentType;
        private Thing           parentThing;

        public string           ByDefLabel;
        public string           ByThingCompLabel;
        public Action_OnThings  ByDefAction = null;
        public Action_OnThings  ByThingCompAction = null;

        public override string Desc {
            get {
                bool addedTo = false;
                StringBuilder stringBuilder = new StringBuilder();

                if( ( ByDefAction != null )&&( parentThing.HasRoommateByDef() ) )
                {
                    stringBuilder.Append( ByDefLabel );
                    addedTo = true;
                }

                if( ( ByThingCompAction != null )&&( parentThing.HasRoommateByThingComp( parentType ) ) )
                {
                    if( addedTo == true )
                        stringBuilder.AppendLine( ";" );
                    stringBuilder.Append( ByThingCompLabel );
                }
                return stringBuilder.ToString();
            }
        }

        public override SoundDef CurActivateSound
        {
            get {
                return SoundDefOf.Click;
            }
        }

        public CommandGroupOfDefOrThingCompInRoom (
            Thing parent, 
            Type RequiredType, 
            string label )
        {
            parentThing = parent;
            icon = Icon.NextButton;

            parentType = RequiredType;

            defaultLabel = label;
        }

        public override void ProcessInput( Event ev ) {
            base.ProcessInput( ev );

            // Process a list of things by def or thingcomp in a room
            if( !parentThing.Position.IsInRoom() )
                return;

            // Left click (if assigned) all in room
            if( ( ByDefAction != null )&&
                ( ev.button == 0 )&&
                ( parentThing.HasRoommateByDef() ) )
                ByDefAction.Invoke( parentThing.GetGroupOfByDefInRoom() );
            else if( ( ByThingCompAction != null )&&
                ( ev.button == 1 )&&
                ( parentThing.HasRoommateByThingComp( parentType ) ) )
                ByThingCompAction.Invoke( parentThing.GetGroupOfByThingCompInRoom( parentType ) );
        }
    }
}
