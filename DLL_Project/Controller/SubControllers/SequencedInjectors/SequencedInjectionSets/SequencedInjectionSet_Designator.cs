using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjectionSet_Designator : SequencedInjectionSet
    {

        public Type                         designatorClass;

        public string                       designationCategoryDef;
        public Type                         designatorNextTo;

        public bool                         reverseDesignator;

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

            if(
                ( designatorClass == null )||
                ( !designatorClass.IsSubclassOf( typeof( Designator ) ) )
            )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Unable to resolve designatorClass '{0}'", designatorClass.FullName ),
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
            if(
                ( !string.IsNullOrEmpty( designationCategoryDef ) )&&
                ( designatorNextTo != null )&&
                ( !designatorNextTo.IsSubclassOf( typeof( Designator ) ) )
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
                    
                if( designatorNextTo == null )
                {
                    // Inject the designator
                    designationCategory.ResolvedDesignators().Add( designator );
                }
                else
                {
                    // Prefers to be beside a specific designator
                    var designatorIndex = designationCategory.ResolvedDesignators().FindIndex( d => (
                        ( d.GetType() == designatorNextTo )
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
                if( !designationCategory.specialDesignatorClasses.Exists( s => s == designatorClass ) )
                {
                    if( designatorNextTo == null )
                    {
                        // Inject the designator class at the end of the list
                        designationCategory.specialDesignatorClasses.Add( designatorClass );
                    }
                    else
                    {
                        // Prefers to be beside a specific designator
                        var designatorIndex = designationCategory.specialDesignatorClasses.FindIndex( s => s == designatorNextTo );

                        if( designatorIndex < 0 )
                        {
                            // Can't find desired designator class
                            // Inject the designator at the end
                            designationCategory.specialDesignatorClasses.Add( designatorClass );
                        }
                        else
                        {
                            // Inject beside desired designator class
                            designationCategory.specialDesignatorClasses.Insert( designatorIndex + 1, designatorClass );
                        }
                    }
                }
            }

            // Add a reverse designator for it
            if( reverseDesignator )
            {
                if( ReverseDesignatorDatabase_Extensions.Find( designatorClass ) == null )
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
                designator = designators.FirstOrDefault( d => d.GetType() == designatorClass );
                if( designator != null )
                {
                    return designator;
                }
            }
            if( reverseDesignator )
            {
                designator = ReverseDesignatorDatabase_Extensions.Find( designatorClass );
                if( designator != null )
                {
                    return designator;
                }
            }
            // If no designator, instatiate a new one
            designator = (Designator) Activator.CreateInstance( designatorClass );
#if DEBUG
            if( designator == null )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "Unable to create instance of '{0}'", designatorClass.FullName ),
                    Name
                );
            }
#endif
            return designator;
        }

    }

}
