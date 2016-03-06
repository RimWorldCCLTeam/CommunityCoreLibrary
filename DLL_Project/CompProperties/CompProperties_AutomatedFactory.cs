using System;
using System.Collections.Generic;

using Verse;


namespace CommunityCoreLibrary
{

    public class CompProperties_AutomatedFactory : CompProperties
    {
        
        #region XML Data

        public FactoryOutputVector          outputVector = FactoryOutputVector.Invalid;
        public FactoryProductionMode        productionMode = FactoryProductionMode.None;

        #endregion

        public CompProperties_AutomatedFactory()
        {
            compClass = typeof( CompProperties_AutomatedFactory );
        }

    }

}
