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

        public List< MCMInjectionSet >      ModConfigurationMenus;

        #endregion

        #region Sequenced Injectors

        public List<SequencedInjectionSet>  SequencedInjectionSets;

        #endregion

        #endregion

        [Unsaved]

        #region Instance Data

        // Used to flag xml defined (false) and auto-generated (true) for logging
        public bool                         dummy = false;

        // Used to link directly to the mod which this def controls
        public ModContentPack               mod;

        #endregion

        #region Global Static Data

        // Interfaces for different injectors
        // Use an array instead of a list to ensure order
        //public static SequencedInjector[]           SequencedInjectors;

        #endregion

        #region Constructors

        /*
        static                              ModHelperDef()
        {
            // Add the injectors to the order-specific array
            // These injectors will be validated in order
            // Actual injection happens in the Injecton Sub Controller
            SequencedInjectors = new SequencedInjector[]
            {
                new SequencedInjector_Detours(),
                new SequencedInjector_SpecialInjectors(),
                new MHD_ThingComps(),
                new MHD_ITabs(),
                new MHD_TickerSwitcher(),
                new MHD_Facilities(),
                new MHD_StockGenerators(),
                new MHD_ThingDefAvailability(),
                new SequencedInjector_Designators()
            };
        }
        */

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

                /*
                #region Injector Validation

                foreach( var injector in SequencedInjectors )
                {
                    isValid &= injector.IsValid( this, ref errors );
                }

                #endregion
                */

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

        public override void                PostLoad()
        {
            base.PostLoad();
            if( !SequencedInjectionSets.NullOrEmpty() )
            {
                foreach( var sequencedInjectionSet in SequencedInjectionSets )
                {
                    sequencedInjectionSet.PostLoad();
                }
            }
        }

        #region Injection

        /*
        public bool                         Inject( SequencedInjector injector )
        {
            if( injector.DefIsInjected( this ) )
            {
                return true;
            }

            Controller.Data.Trace_Current_Mod = this;

            var result = injector.InjectByDef( this );

            Controller.Data.Trace_Current_Mod = null;
            return result;
        }
        */

        /*
        public static SequencedInjector             GetInjector( Type injector )
        {
            return SequencedInjectors.First( i => i.GetType() == injector );
        }
        */

        #endregion

    }

}
