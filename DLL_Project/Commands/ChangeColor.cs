using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Commands
{

    public class ChangeColor : Command
    {
        
        readonly CompColoredLight           parentLight;

        public override string              Desc
        {
            get
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendFormat(
                    "CommandChangeColorDesc".Translate(), 
                    parentLight.NextColorName(), 
                    parentLight.PrevColorName() );
                
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

        public                              ChangeColor( CompColoredLight light )
        {
            parentLight = light;
            icon = Icon.NextButton;
            defaultLabel = "CommandChangeColorLabel".Translate();
        }

        public override void                ProcessInput( Event ev )
        {
            base.ProcessInput( ev );

            if( ev.button == 0 )
            {
                parentLight.IncrementColorIndex();
            }
            else if( ev.button == 1 )
            {
                parentLight.DecrementColorIndex();
            }
        }

    }

}
