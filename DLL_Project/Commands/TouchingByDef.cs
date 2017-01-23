using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Commands
{

    public class TouchingByDef : Command
    {

        readonly Thing                      parentThing;
        readonly Action_OnThings            ClickLeft = null;
        readonly Action_OnThings            ClickRight = null;

        public override string              Desc
        {
            get
            {
                var addedToOutput = false;
                var stringBuilder = new StringBuilder();

                if( ( ClickLeft != null )&&
                    ( parentThing.Position.IsInRoom(  parentThing.Map ) ) )
                {
                    stringBuilder.Append( "CommandGroupOfThingsLClick".Translate() );
                    addedToOutput = true;
                }

                if( ClickRight != null )
                {
                    if( addedToOutput )
                    {
                        stringBuilder.AppendLine( ";" );
                    }
                    stringBuilder.Append( "CommandGroupOfThingsRClick".Translate() );
                }
                return stringBuilder.ToString();
            }
        }

        public override SoundDef            CurActivateSound
        {
            get
            {
                return SoundDefOf.Click;
            }
        }

        public                              TouchingByDef(
            Thing parent,
            Texture2D designatorIcon,
            Action_OnThings LeftClick = null,
            Action_OnThings RightClick = null
        )
        {
            parentThing = parent;
            icon = designatorIcon;

            ClickLeft = LeftClick;
            ClickRight = RightClick;

            defaultDesc = Desc;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat( "CommandGroupOfThingsByDefLabel".Translate(), parent.def.label );
            defaultLabel = stringBuilder.ToString();
        }

        public override void                ProcessInput( Event ev )
        {
            base.ProcessInput( ev );

            // Process a list of touching things by def
            if( !parentThing.IsSameThingDefTouching() )
            {
                return;
            }

            // Left click (if assigned) all in room
            if( ( ClickLeft != null )&&
                ( ev.button == 0 )&&
                ( parentThing.Position.IsInRoom( parentThing.Map ) ) )
            {
                ClickLeft.Invoke( parentThing.ListSameThingDefTouching( true ) );
            }
            // right click (if assigned) all on map
            else if( ( ClickRight != null )&&
                ( ev.button == 1 ) )
            {
                ClickRight.Invoke( parentThing.ListSameThingDefTouching( false ) );
            }
        }

    }

}
