namespace CommunityCoreLibrary
{
    
    public enum InjectionTiming
    {
        Priority_0              =  0,
        Priority_1              =  1,
        Priority_2              =  2,
        Priority_3              =  3,
        Priority_4              =  4,
        Priority_5              =  5,
        Priority_6              =  6,
        Priority_7              =  7,
        Priority_8              =  8,
        Priority_9              =  9,
        Priority_10             = 10,
        Priority_11             = 11,
        Priority_12             = 12,
        Priority_13             = 13,
        Priority_14             = 14,
        Priority_15             = 15,
        Priority_16             = 16,
        Priority_17             = 17,
        Priority_18             = 18,
        Priority_19             = 19,
        Priority_20             = 20,
        Priority_21             = 21,
        Priority_22             = 22,
        Priority_23             = 23,
        Priority_24             = 24,
        Priority_25             = 25,
        // Default InjectionSequence.MainLoad
        ThingDefAvailability    = Priority_2,
        StockGenerators         = Priority_5,
        Facilities              = Priority_8,
        TickerSwitcher          = Priority_11,
        ITabs                   = Priority_14,
        ThingComps              = Priority_17,
        SpecialInjectors        = Priority_20,
        Detours                 = Priority_23,
        // Default InjectionSequence.GameLoad
        Designators             = Priority_12,
        MapComponents           = Priority_24,
        // InjectionSequence.Never
        Never                   = -1,
        // InjectionSequence.Any
        All                     = 65536
    }

}
