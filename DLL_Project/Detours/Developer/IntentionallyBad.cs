// This class is simply for debugging detours

#if DEVELOPER
// Enable this define for a whole bunch of debug detours to get trapped
//#define _I_AM_A_POTATO_
#endif

#if _I_AM_A_POTATO_

namespace CommunityCoreLibrary.Detour.IntentionallyBad
{

    internal class Instance
    {

        internal void               NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyBad.Instance" );
        }

        internal void               WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyBad.Instance" );
        }

        internal static void        StaticNoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticNoParameters()",
                "Detour.IntentionallyBad.Instance" );
        }

        internal static void        StaticWithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticWithParameters()",
                "Detour.IntentionallyBad.Instance" );
        }

    }

    internal static class Static
    {

        internal static void        NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyBad.Static" );
        }

        internal static void        WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyBad.Static" );
        }

        internal static void        ExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodNoParameters()",
                "Detour.IntentionallyBad.Static" );
        }

        internal static void        ExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodWithParameters()",
                "Detour.IntentionallyBad.Static" );
        }

    }

    internal class InstanceToInstance : Instance
    {

        [DetourMember( "WithParameters" )]
        internal void               _NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyBad.InstanceToInstance" );
        }

        [DetourMember( "StaticNoParameters" )]
        internal void               _WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyBad.InstanceToInstance" );
        }

        [DetourMember( "StaticWithParameters" )]
        internal static void        _StaticNoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticNoParameters()",
                "Detour.IntentionallyBad.InstanceToInstance" );
        }

        [DetourMember( "WithParameters" )]
        internal static void        _StaticWithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "StaticWithParameters()",
                "Detour.IntentionallyBad.InstanceToInstance" );
        }

    }

    internal static class StaticToStatic
    {

        [DetourMember( typeof( Static ), "WithParameters" )]
        internal static void        _NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyBad.StaticToStatic" );
        }

        [DetourMember( typeof( Static ), "ExtensionMethodNoParameters" )]
        internal static void        _WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyBad.StaticToStatic" );
        }

        [DetourMember( typeof( Static ), "WithParameters" )]
        internal static void        _ExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodNoParameters()",
                "Detour.IntentionallyBad.StaticToStatic" );
        }

        [DetourMember( typeof( Static ), "NoParameters" )]
        internal static void        _ExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodWithParameters()",
                "Detour.IntentionallyBad.StaticToStatic" );
        }

    }

    internal class InstanceToStatic : Instance
    {

        [DetourMember( typeof( Static ), "WithParameters" )]
        internal static void        _NoParameters()
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "NoParameters()",
                "Detour.IntentionallyBad.InstanceToStatic" );
        }

        [DetourMember( typeof( Static ), "NoParameters" )]
        internal static void        _WithParameters( int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "WithParameters()",
                "Detour.IntentionallyBad.InstanceToStatic" );
        }

    }

    internal static class StaticToInstance
    {

        [DetourMember( typeof( Instance ), "WithParameters" )]
        internal static void        _ExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodNoParameters()",
                "Detour.IntentionallyBad.StaticToInstance" );
        }

        [DetourMember( typeof( Instance ), "NoParameters" )]
        internal static void        _ExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ExtensionMethodWithParameters()",
                "Detour.IntentionallyBad.StaticToStatic" );
        }

        [DetourMember( "WithParameters" )]
        internal static void        _ImpliedExtensionMethodNoParameters( this Instance i )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ImpliedExtensionMethodNoParameters()",
                "Detour.IntentionallyBad.StaticToStatic" );
        }

        [DetourMember( "NoParameters" )]
        internal static void        _ImpliedExtensionMethodWithParameters( this Instance i, int foo, float bar )
        {
            CCL_Log.Trace(
                Verbosity.Injections,
                "ImpliedExtensionMethodWithParameters()",
                "Detour.IntentionallyBad.StaticToInstance" );
        }

    }

}

#endif
