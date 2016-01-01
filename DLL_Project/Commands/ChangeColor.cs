using System;
using System.Text;
using CommunityCoreLibrary.ColorPicker;
using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Commands
{

    public class ChangeColor : Command
    {

        readonly CompColoredLight           parentLight;
        bool                                usePicker;

        public override string              Desc
        {
            get
            {
                var stringBuilder = new StringBuilder();

                if ( usePicker )
                {
                    stringBuilder.AppendFormat( "CommandChangeColorLabel".Translate() );
                }
                else
                {
                    stringBuilder.AppendFormat(
                        "CommandChangeColorDesc".Translate(),
                        parentLight.NextColorName(),
                        parentLight.PrevColorName() );
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

        public                              ChangeColor(
            CompColoredLight light,
            Texture2D designatorIcon,
            bool usePicker = false
        )
        {
            parentLight = light;
            icon = designatorIcon;
            this.usePicker = usePicker;
            defaultLabel = "CommandChangeColorLabel".Translate();
        }

        public override void                ProcessInput( Event ev )
        {
            base.ProcessInput( ev );

            if ( usePicker )
            {
                Color col = parentLight.CompGlower.props.glowColor.ToColor;
                // set alpha to 1 (lights use alpha zero, but that won't show in the picker)
                ColorWrapper _color = new ColorWrapper( new Color( col.r, col.g, col.b ) );
                
                Find.WindowStack.Add( new Dialog_ColorPicker( _color, delegate
                {
                    
                    parentLight.ChangeColor( _color.Color );
                }, ev.button == 1, true ) );
            }
            else
            {
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
}
