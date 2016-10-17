using System;

namespace CommunityCoreLibrary
{
    
    public enum InjectionSequence
    {
        GameLoad =  1,
        MainLoad =  2,
        DLLLoad  =  3,
        Never    = -1
    }

}
