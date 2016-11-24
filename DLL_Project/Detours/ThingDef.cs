using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal class _ThingDef : ThingDef
    {

        [DetourMember]
        internal bool                       _IsFoodDispenser
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
