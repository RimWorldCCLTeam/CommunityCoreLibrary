using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public interface IHopperUser
    {
        // This property is the thing filter to program the hopper with
        ThingFilter                         ResourceFilter
        {
            get;
        }

    }

}

