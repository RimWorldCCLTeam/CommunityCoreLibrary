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
        Default         = Validation
    }

}
