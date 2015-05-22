using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public struct colorName
    {
        public string               name;
        public ColorInt             value;

        public colorName ( string n, ColorInt v )
        {
            name = n;
            value = v;
        }
    }

    public class CompProperties_ColoredLight : CompProperties
    {
        // These may be defined in xml or left as the default
        public string               requiredResearch = "ColoredLights";
        public int                  Default = 0;
        public List< colorName >    color;

        public CompProperties_ColoredLight ()
        {
            this.compClass = typeof( CompProperties_ColoredLight );
        }
    }
}

