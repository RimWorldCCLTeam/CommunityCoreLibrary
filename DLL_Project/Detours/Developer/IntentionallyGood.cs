// This class is simply for debugging detours

#if DEVELOPER
// Enable this define for a whole bunch of debug detours to get trapped
//#define _I_AM_A_POTATO_
#endif

#if _I_AM_A_POTATO_

namespace CommunityCoreLibrary.Detour.IntentionallyGood
{

    internal class Instance
    {

        internal void               NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyGood.Instance" );
        }

        internal void               WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyGood.Instance" );
        }

        internal static void        StaticNoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticNoParameters()",
                "Detour.IntentionallyGood.Instance" );
        }

        internal static void        StaticWithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticWithParameters()",
                "Detour.IntentionallyGood.Instance" );
        }

    }

    internal static class Static
    {

        internal static void        NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyGood.Static" );
        }

        internal static void        WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyGood.Static" );
        }

        internal static void        ExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodNoParameters()",
                "Detour.IntentionallyGood.Static" );
        }

        internal static void        ExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodWithParameters()",
                "Detour.IntentionallyGood.Static" );
        }

    }

    internal class InstanceToInstance : Instance
    {

        [DetourMember]
        internal void               _NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyGood.InstanceToInstance" );
        }

        [DetourMember]
        internal void               _WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyGood.InstanceToInstance" );
        }

        [DetourMember]
        internal static void        _StaticNoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticNoParameters()",
                "Detour.IntentionallyGood.InstanceToInstance" );
        }

        [DetourMember]
        internal static void        _StaticWithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticWithParameters()",
                "Detour.IntentionallyGood.InstanceToInstance" );
        }

    }

    internal static class StaticToStatic
    {

        [DetourMember( typeof( Static ) )]
        internal static void        _NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyGood.StaticToStatic" );
        }

        [DetourMember( typeof( Static ) )]
        internal static void        _WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyGood.StaticToStatic" );
        }

        [DetourMember( typeof( Static ) )]
        internal static void        _ExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodNoParameters()",
                "Detour.IntentionallyGood.StaticToStatic" );
        }

        [DetourMember( typeof( Static ) )]
        internal static void        _ExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodWithParameters()",
                "Detour.IntentionallyGood.StaticToStatic" );
        }

    }

    internal class InstanceToStatic : Instance
    {

        [DetourMember( typeof( Static ) )]
        internal static void        _NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyGood.InstanceToStatic" );
        }

        [DetourMember( typeof( Static ) )]
        internal static void        _WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyGood.InstanceToStatic" );
        }

    }

    internal static class StaticToInstance
    {

        [DetourMember( typeof( Instance ), "NoParameters" )]
        internal static void        _ExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodNoParameters()",
                "Detour.IntentionallyGood.StaticToInstance" );
        }

        [DetourMember( typeof( Instance ), "WithParameters" )]
        internal static void        _ExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodWithParameters()",
                "Detour.IntentionallyGood.StaticToStatic" );
        }

        [DetourMember( "NoParameters" )]
        internal static void        _ImpliedExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ImpliedExtensionMethodNoParameters()",
                "Detour.IntentionallyGood.StaticToStatic" );
        }

        [DetourMember( "WithParameters" )]
        internal static void        _ImpliedExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ImpliedExtensionMethodWithParameters()",
                "Detour.IntentionallyGood.StaticToInstance" );
        }

    }

}

#endif
