using System.Text;

namespace CommunityCoreLibrary
{

    public interface IInjector
    {

#if DEBUG
        string                              InjectString { get; }
        bool                                IsValid( ModHelperDef def, ref string errors );
#endif

        bool                                Injected( ModHelperDef def );

        bool                                Inject( ModHelperDef def );

    }

}
