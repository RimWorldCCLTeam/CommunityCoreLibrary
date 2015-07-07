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

    public class CommandChangeColor : Command
    {
        private CompColoredLight    _light;

        public override string Desc {
            get {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(
                    "CommandChangeColorDesc".Translate(), 
                    _light.NextColorName(), 
                    _light.PrevColorName() );
                return stringBuilder.ToString();
            }
        }

        public override SoundDef CurActivateSound
        {
            get {
                return SoundDefOf.Click;
            }
        }

        public CommandChangeColor( CompColoredLight light )
        {
            _light = light;
            icon = Icon.NextButton;
            defaultLabel = "CommandChangeColorLabel".Translate();
        }

        public override void ProcessInput( Event ev ) {
            base.ProcessInput( ev );

            if( ev.button == 0 )
                _light.IncrementColorIndex();
            else if (ev.button == 1)
                _light.DecrementColorIndex();
        }
    }

}

