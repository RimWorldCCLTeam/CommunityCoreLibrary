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

    public class CommandGroupOfTouchingThingsByDef : Command
    {

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
                        stringBuilder.AppendLine( ";" );
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

        public CommandGroupOfTouchingThingsByDef( Thing parent, Action_OnThings lAction = null, Action_OnThings rAction = null )
        {
            parentThing = parent;
            icon = Icon.NextButton;

            _laction = lAction;
            _raction = rAction;

            defaultDesc = Desc;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat( "CommandGroupOfThingsByDefLabel".Translate(), parent.def.label );
            defaultLabel = stringBuilder.ToString();
        }

        public override void ProcessInput( Event ev ) {
            base.ProcessInput( ev );

            // Process a list of touching things by def
            if( !parentThing.HasTouchingByDef() )
                return;
            
            // Left click (if assigned) all in room
            if( ( _laction != null )&&
                ( ev.button == 0 )&&
                ( parentThing.Position.IsInRoom() ) )
                _laction.Invoke( parentThing.GetGroupOfTouchingByDef( true ) );
            // right click (if assigned) all on map
            else if( ( _raction != null )&&
                ( ev.button == 1 ) )
                _raction.Invoke( parentThing.GetGroupOfTouchingByDef( false ) );
        }
    }
}
