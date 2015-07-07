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

    public class CommandGroupOfTouchingThingsByThingComp : Command
    {

        private Type            parentType;
        private Thing           parentThing;
        private Action_OnThings _laction = null;
        private Action_OnThings _raction = null;

        public override string Desc {
            get {
                bool addedTo = false;
                StringBuilder stringBuilder = new StringBuilder();

                if( ( _laction != null )&&( parentThing.Position.IsInRoom() ) )
                {
                    stringBuilder.Append( "CommandGroupOfThingsLClick".Translate() );
                    addedTo = true;
                }

                if( _raction != null )
                {
                    if( addedTo == true )
                        stringBuilder.Append( ";\n" );
                    stringBuilder.Append( "CommandGroupOfThingsRClick".Translate() );
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

        public CommandGroupOfTouchingThingsByThingComp( Thing parent, Type RequiredType, string label, Action_OnThings lAction = null, Action_OnThings rAction = null )
        {
            parentThing = parent;
            icon = Icon.NextButton;

            parentType = RequiredType;

            _laction = lAction;
            _raction = rAction;

            defaultDesc = Desc;
            defaultLabel = label;
        }

        public override void ProcessInput( Event ev ) {
            base.ProcessInput( ev );

            // Process a list of touching things by def
            if( !parentThing.HasTouchingByThingComp( parentType ) )
                return;
            
            // Left click (if assigned) all in room
            if( ( _laction != null )&&
                ( ev.button == 0 )&&
                ( parentThing.Position.IsInRoom() ))
                _laction.Invoke( parentThing.GetGroupOfTouchingByThingComp( parentType, true ) );
            // right click (if assigned) all on map
            else if( ( _raction != null )&&
                ( ev.button == 1 ) )
                _raction.Invoke( parentThing.GetGroupOfTouchingByThingComp( parentType, false ) );
        }
    }
}
