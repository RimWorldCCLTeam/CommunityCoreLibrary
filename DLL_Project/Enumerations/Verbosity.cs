namespace CommunityCoreLibrary
{

    public enum Verbosity
    {
        FatalErrors     = 0,
        NonFatalErrors  = 1,
        Validation      = 2,
        Warnings        = 3,
        Injections      = 4,
        AutoGenCreation = 5,
        StateChanges    = 6,
        Stack           = 7,

#if                     RELEASE
        Default         = Validation
#else
    #if                 DEVELOPER
        Default         = AutoGenCreation
    #else
        //              DEBUG
        Default         = Injections
    #endif
#endif
    }

}
