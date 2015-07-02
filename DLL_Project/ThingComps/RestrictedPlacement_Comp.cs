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

    public class RestrictedPlacement_Comp : ThingComp
    {
        private RestrictedPlacement_Properties Restrictions
        {
            get
            {
                return (RestrictedPlacement_Properties)props;
            }
        }
    }
}

