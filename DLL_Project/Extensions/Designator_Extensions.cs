using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{
    public static class Designator_Extensions
    {
        public static bool DesignatorExists( this DesignatorData designatorData )
        {
            var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( designatorData.designationCategoryDef, false );
            List<Designator> designators = designationCategory._resolvedDesignators();
            return designators.Exists( d => d.GetType() == designatorData.designatorClass );
        }
    }
}
