using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class ModHelperDef : Def
    {

        #region XML Data

        #region Mod Level Control Data

        public string                       ModName;

        public string                       minCCLVersion;

        public Verbosity                    Verbosity = Verbosity.Default;

        // TODO:  Add ability to load MCM data before anything else (default behaviour: last as per 0.13.1 and older)
        public bool                         PreloadMCMs = false;
        public List< MCMInjectionSet >      ModConfigurationMenus;

        #endregion

        #region Engine Level Injectors

        // InjectionSubController
        public List< Type >                 SpecialInjectors;
        public List< CompInjectionSet >     ThingComps;
        public List< TickerSwitcher >       tickerSwitcher;
        public List< FacilityInjectionSet > Facilities;
        public List< StockGeneratorInjectionSet > TraderKinds;
        public List< ThingDefAvailability > ThingDefAvailability;

        #endregion

        #region Game Level Injectors

        // InjectionSubController
        public List< Type >                 PostLoadInjectors;
        public List< Type >                 MapComponents;
        public List< DesignatorData >       Designators;

        #endregion

        #endregion

        [Unsaved]

        #region Instance Data

        // Used to flag xml defined (false) and auto-generated (true) for logging
        public bool                         dummy = false;

        // Used to link directly to the mod which this def controls
        public LoadedMod                    mod;

        #endregion

        #region Global Static Data

        // Interfaces for different injectors
        // Use an array instead of a list to ensure order
        public static IInjector[]           Injectors;

        #endregion

        #region Constructors

        static                              ModHelperDef()
        {
            // Add the injectors to the order-specific array
            // These injectors will be validated in order
            // Actual injection happens in the Injecton Sub Controller
            Injectors = new IInjector[]
            {
                new MHD_SpecialInjectors(),
                new MHD_ThingComps(),
                new MHD_TickerSwitcher(),
                new MHD_Facilities(),
                new MHD_TraderKinds(),
                new MHD_ThingDefAvailability(),
                new MHD_PostLoadInjectors(),
                new MHD_MapComponents(),
                new MHD_Designators()
            };
        }

        #endregion

        #region Validation

        public bool                         IsValid
        {
            get
            {
                var isValid = true;
                var errors = "";

                #region Name Validation

                if( ModName.NullOrEmpty() )
                {
                    errors += "\n\tMissing ModName";
                    isValid = false;
                }

                #endregion

                #region Version Validation

                if( minCCLVersion.NullOrEmpty() )
                {
                    errors += "\n\tNull or empty 'minCCLVersion' requirement";
                    isValid = false;
                }
                else
                {
                    var vc = Version.Compare( minCCLVersion );
                    switch( vc )
                    {
                    case Version.VersionCompare.LessThanMin:
                        errors += "\n\tUnsupported 'minCCLVersion' requirement (v" + minCCLVersion + ") minimum supported version is v" + Version.Minimum;
                        isValid = false;
                        break;
                    case Version.VersionCompare.GreaterThanMax:
                        errors += "\n\tUnsupported 'minCCLVersion' requirement (v" + minCCLVersion + ") maximum supported version is v" + Version.Current;
                        isValid = false;
                        break;
                    case Version.VersionCompare.Invalid:
                        errors += "\n\tUnable to get version from '" + minCCLVersion + "'";
                        isValid = false;
                        break;
                    }
                }

                #endregion

#if DEBUG

                #region Mod Configuration Menu Validation

                if( !ModConfigurationMenus.NullOrEmpty() )
                {
                    foreach( var mcm in ModConfigurationMenus )
                    {
                        if( !mcm.mcmClass.IsSubclassOf( typeof( ModConfigurationMenu ) ) )
                        {
                            errors += string.Format( "\n\tUnable to resolve Mod Configuration Menu '{0}'", mcm.mcmClass.ToString() );
                            isValid = false;
                        }
                    }
                }

                #endregion

                #region Injector Validation

                foreach( var injector in Injectors )
                {
                    isValid &= injector.IsValid( this, ref errors );
                }

                #endregion

#endif

                if( !isValid )
                {
                    var builder = new StringBuilder();
                    builder.Append( "ModHelperDef :: " ).Append( defName );
                    if( !ModName.NullOrEmpty() )
                    {
                        builder.Append( " :: " ).Append( ModName );
                    }
                    CCL_Log.Error( errors, builder.ToString() );
                }

                return isValid;
            }
        }

        #endregion

        #region Injection

        public bool                         Inject( IInjector injector )
        {
            if( injector.Injected( this ) )
            {
                return true;
            }

            Controller.Data.Trace_Current_Mod = this;

            var result = injector.Inject( this );

            Controller.Data.Trace_Current_Mod = null;
            return result;
        }

        public static IInjector             GetInjector( Type injector )
        {
            return Injectors.First( i => i.GetType() == injector );
        }

        #endregion

    }

}
