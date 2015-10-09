using Verse;

namespace CommunityCoreLibrary
{
    
    public class VerbProperties_Extended : VerbProperties
    {
        
        public class VerbExperience
        {
            
            public float                    min;
            public float                    mid;
            public float                    max;

            public VerbExperience()
            {
                // Vanilla default values
                min = 10;
                mid = 50;
                max = 240;
            }
        }

        // XML stuff
        public int                          pelletCount = 1;
        public VerbExperience               experienceGain = new VerbExperience();
    }

}
