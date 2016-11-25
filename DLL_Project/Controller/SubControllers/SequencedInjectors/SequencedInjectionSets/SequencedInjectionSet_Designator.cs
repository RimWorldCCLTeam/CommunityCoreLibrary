using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjectionSet_Designator : SequencedInjectionSet
    {

        public string                       designatorClass;

        public string                       designationCategoryDef;
        public string                       designatorNextTo;

        public bool                         reverseDesignator;

        [Unsaved]
        public Type                         designatorClassInt;
        [Unsaved]
        public Type                         designatorNextToInt;

        public                              SequencedInjectionSet_Designator()
        {
            injectionSequence               = InjectionSequence.GameLoad;
            injectionTiming                 = InjectionTiming.Designators;
        }

        public override Type                defType => null;
        public override InjectorTargetting  Targetting => InjectorTargetting.None;

        public override bool                IsValid()
        {
            bool valid = true;

            if( !string.IsNullOrEmpty( designatorClass ) )
            {
                designatorClassInt = GenTypes.GetTypeInAnyAssembly( designatorClass );
            }
            if(
                ( designatorClassInt == null )||
                ( !designatorClassInt.IsSubclassOf( typeof( Designator ) ) )
            )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Unable to resolve designatorClass '{0}'", designatorClass ),
                    Name
                );
                valid = false;
            }
            if(
                ( !reverseDesignator )&&
                ( string.IsNullOrEmpty( designationCategoryDef ) )
            )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    "Designator is not marked reverseDesignator and designationCategoryDef is null",
                    Name
                );
                valid = false;
            }
            if(
                ( !string.IsNullOrEmpty( designationCategoryDef ) )&&
                ( DefDatabase<DesignationCategoryDef>.GetNamed( designationCategoryDef, false ) == null )
            )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Unable to resolve designationCategoryDef '{0}'", designationCategoryDef ),
                    Name
                );
                valid = false;
            }
            if( !string.IsNullOrEmpty( designatorNextTo ) )
            {
                designatorNextToInt = GenTypes.GetTypeInAnyAssembly( designatorNextTo );
            }
            if(
                ( !string.IsNullOrEmpty( designationCategoryDef ) )&&
                ( designatorNextToInt != null )&&
                ( !designatorNextToInt.IsSubclassOf( typeof( Designator ) ) )
            )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Unable to resolve designatorNextTo '{0}'", designatorNextTo ),
                    Name
                );
                valid = false;
            }
            return valid;
        }

        public override bool                Inject()
        {
            // Find the possible existing designator or create a new one
            var designator = GetDesignator();
#if DEBUG
            if( designator == null )
            {   // Should never happen
                return false;
            }
#endif

            // Assign it to the category
            if( !string.IsNullOrEmpty( designationCategoryDef ) )
            {
                // Get the category
                var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( designationCategoryDef, false );
                    
                if( designatorNextToInt == null )
                {
                    // Inject the designator
                    designationCategory.ResolvedDesignators().Add( designator );
                }
                else
                {
                    // Prefers to be beside a specific designator
                    var designatorIndex = designationCategory.ResolvedDesignators().FindIndex( d => (
                        ( d.GetType() == designatorNextToInt )
                    ) );

                    if( designatorIndex < 0 )
                    {
                        // Other designator doesn't exist (yet?)
                        // Inject the designator at the end
                        designationCategory.ResolvedDesignators().Add( designator );
                    }
                    else
                    {
                        // Inject beside desired designator
                        designationCategory.ResolvedDesignators().Insert( designatorIndex + 1, designator );
                    }
                }
            
                // Now inject the designator class into the list of classes as a saftey net for another mod resolving the category
                if( !designationCategory.specialDesignatorClasses.Exists( s => s == designatorClassInt ) )
                {
                    if( designatorNextTo == null )
                    {
                        // Inject the designator class at the end of the list
                        designationCategory.specialDesignatorClasses.Add( designatorClassInt );
                    }
                    else
                    {
                        // Prefers to be beside a specific designator
                        var designatorIndex = designationCategory.specialDesignatorClasses.FindIndex( s => s == designatorNextToInt );

                        if( designatorIndex < 0 )
                        {
                            // Can't find desired designator class
                            // Inject the designator at the end
                            designationCategory.specialDesignatorClasses.Add( designatorClassInt );
                        }
                        else
                        {
                            // Inject beside desired designator class
                            designationCategory.specialDesignatorClasses.Insert( designatorIndex + 1, designatorClassInt );
                        }
                    }
                }
            }

            // Add a reverse designator for it
            if( reverseDesignator )
            {
                if( ReverseDesignatorDatabase_Extensions.Find( designatorClassInt ) == null )
                {
                    ReverseDesignatorDatabase.AllDesignators.Add( designator );
                }
            }

            return true;
        }

        public Designator                   GetDesignator()
        {
            Designator designator = null;
            if( !string.IsNullOrEmpty( designationCategoryDef ) )
            {
                var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( designationCategoryDef, false );
                var designators = designationCategory.ResolvedDesignators();
                designator = designators.FirstOrDefault( d => d.GetType() == designatorClassInt );
                if( designator != null )
                {
                    return designator;
                }
            }
            if( reverseDesignator )
            {
                designator = ReverseDesignatorDatabase_Extensions.Find( designatorClassInt );
                if( designator != null )
                {
                    return designator;
                }
            }
            // If no designator, instatiate a new one
            designator = (Designator) Activator.CreateInstance( designatorClassInt );
#if DEBUG
            if( designator == null )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "Unable to create instance of '{0}'", designatorClass ),
                    Name
                );
            }
#endif
            return designator;
        }

    }

}
