using System;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Commands
{

    public class DefOrThingCompInRoom : Command
    {
        
        readonly Type                       parentType;
        readonly Thing                      parentThing;

        public string                       LabelByDef;
        public string                       LabelByThingComp;
        public Action_OnThings              ClickByDef;
        public Action_OnThings              ClickByThingComp;

        public override string              Desc
        {
            get
            {
                var addedToOutput = false;
                var stringBuilder = new StringBuilder();

                if( ( ClickByDef != null )&&
                    ( parentThing.IsSameThingDefInRoom() ) )
                {
                    stringBuilder.Append( LabelByDef );
                    addedToOutput = true;
                }

                if( ( ClickByThingComp != null )&&
                    ( parentThing.IsSameThingCompInRoom( parentType ) ) )
                {
                    if( addedToOutput )
                    {
                        stringBuilder.AppendLine( ";" );
                    }
                    stringBuilder.Append( LabelByThingComp );
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

        public                              DefOrThingCompInRoom ( Thing parent, Type RequiredType, string label )
        {
            parentThing = parent;
            icon = Icon.NextButton;

            parentType = RequiredType;

            defaultLabel = label;
        }

        public override void                ProcessInput( Event ev )
        {
            base.ProcessInput( ev );

            // Process a list of things by def or thingcomp in a room
            if( !parentThing.Position.IsInRoom() )
            {
                return;
            }

            // Left click (if assigned) all in room
            if( ( ClickByDef != null )&&
                ( ev.button == 0 )&&
                ( parentThing.IsSameThingDefInRoom() ) )
            {
                ClickByDef.Invoke( parentThing.ListSameThingDefInRoom() );
            }
            else if( ( ClickByThingComp != null )&&
                ( ev.button == 1 )&&
                ( parentThing.IsSameThingCompInRoom( parentType ) ) )
            {
                ClickByThingComp.Invoke( parentThing.ListSameThingCompInRoom( parentType ) );
            }
        }

    }

}
