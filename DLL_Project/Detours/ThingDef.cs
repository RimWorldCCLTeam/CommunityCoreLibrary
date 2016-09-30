using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal class _ThingDef : ThingDef
    {

        [DetourClassProperty( typeof( ThingDef ), "IsFoodDispenser" )]
        internal bool                       IsFoodMachine
        {
            get
            {
                if( typeof( Building_NutrientPasteDispenser ).IsAssignableFrom( this.thingClass ) )
                {
                    return true;
                }
                if( typeof( Building_AutomatedFactory ).IsAssignableFrom( this.thingClass ) )
                {   // Make sure we are only return factories which are configured as food synthesizers
                    return Building_AutomatedFactory.DefOutputToPawnsDirectly( this );
                }
                return false;
            }
        }

    }

}
