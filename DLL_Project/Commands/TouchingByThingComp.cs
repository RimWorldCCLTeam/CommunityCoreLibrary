using System;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Commands
{

    public class TouchingByThingComp : Command
    {

        readonly Type                       parentType;
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
                    ( parentThing.Position.IsInRoom() ) )
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

        public                              TouchingByThingComp(
            Thing parent,
            Type RequiredType,
            string label,
            Texture2D designatorIcon,
            Action_OnThings LeftClick = null,
            Action_OnThings RightClick = null
        )
        {
            parentThing = parent;
            icon = designatorIcon;

            parentType = RequiredType;

            ClickLeft = LeftClick;
            ClickRight = RightClick;

            defaultDesc = Desc;
            defaultLabel = label;
        }

        public override void                ProcessInput( Event ev )
        {
            base.ProcessInput( ev );

            // Process a list of touching things by def
            if( !parentThing.IsSameThingCompTouching( parentType ) )
            {
                return;
            }

            // Left click (if assigned) all in room
            if( ( ClickLeft != null )&&
                ( ev.button == 0 )&&
                ( parentThing.Position.IsInRoom() ) )
            {
                ClickLeft.Invoke( parentThing.ListSameThingCompTouching( parentType, true ) );
            }
            // right click (if assigned) all on map
            else if( ( ClickRight != null )&&
                ( ev.button == 1 ) )
            {
                ClickRight.Invoke( parentThing.ListSameThingCompTouching( parentType, false ) );
            }
        }

    }

}
