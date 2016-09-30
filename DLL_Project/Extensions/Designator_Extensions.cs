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
            bool rVal = true;
            if( !string.IsNullOrEmpty( designatorData.designationCategoryDef ) )
            {
                var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( designatorData.designationCategoryDef, false );
                List<Designator> designators = designationCategory.ResolvedDesignators();
                rVal &= designators.Exists( d => d.GetType() == designatorData.designatorClass );
            }
            if( designatorData.reverseDesignator )
            {
                rVal &= ReverseDesignatorDatabase_Extensions.Find( designatorData.designatorClass ) != null;
            }
            return rVal;
        }
    }
}
