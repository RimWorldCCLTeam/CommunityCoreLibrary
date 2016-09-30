using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_Designators : IInjector
    {

#if DEBUG
        public override string              InjectString => "Designators injected";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.Designators.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var injectionSet in def.Designators )
            {
                var designatorType = injectionSet.designatorClass;
                if(
                    ( designatorType == null )||
                    ( !designatorType.IsSubclassOf( typeof( Designator ) ) )
                )
                {
                    errors += string.Format( "Unable to resolve designatorClass '{0}'", injectionSet.designatorClass );
                    isValid = false;
                }
                if(
                    ( !injectionSet.reverseDesignator )&&
                    ( string.IsNullOrEmpty( injectionSet.designationCategoryDef ) )
                )
                {
                    errors += "Designator is not marked reverseDesignator and designationCategoryDef is null!";
                    isValid = false;
                }
                if(
                    ( !string.IsNullOrEmpty( injectionSet.designationCategoryDef ) )&&
                    ( DefDatabase<DesignationCategoryDef>.GetNamed( injectionSet.designationCategoryDef, false ) == null )
                )
                {
                    errors += string.Format( "Unable to resolve designationCategoryDef '{0}'", injectionSet.designationCategoryDef );
                    isValid = false;
                }
                if(
                    ( !string.IsNullOrEmpty( injectionSet.designationCategoryDef ) )&&
                    ( injectionSet.designatorNextTo != null )&&
                    ( !injectionSet.designatorNextTo.IsSubclassOf( typeof( Designator ) ) )
                )
                {
                    errors += string.Format( "Unable to resolve designatorNextTo '{0}'", injectionSet.designatorNextTo );
                    isValid = false;
                }
            }

            return isValid;
        }
#endif

        public override bool                DefIsInjected( ModHelperDef def )
        {
            if( def.Designators.NullOrEmpty() )
            {
                return true;
            }

            foreach( var injectionSet in def.Designators )
            {
                if( !injectionSet.DesignatorExists() )
                {
                    return false;
                }
            }

            return true;
        }

        public override bool                InjectByDef( ModHelperDef def )
        {
            if( def.Designators.NullOrEmpty() )
            {
                return true;
            }

            foreach( var injectionSet in def.Designators )
            {
                if( !string.IsNullOrEmpty( injectionSet.designationCategoryDef ) )
                {
                    // Get the category
                    var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( injectionSet.designationCategoryDef, false );

                    // First instatiate and inject the designator into the list of resolved designators
                    if ( !injectionSet.DesignatorExists() )
                    {
                        // Create the new designator
                        var designatorObject = (Designator) Activator.CreateInstance( injectionSet.designatorClass );
                        if( designatorObject == null )
                        {
                            CCL_Log.Message( string.Format( "Unable to create instance of '{0}'", injectionSet.designatorClass ) );
                            return false;
                        }
                              
                        if( injectionSet.designatorNextTo == null )
                        {
                            // Inject the designator
                            designationCategory.ResolvedDesignators().Add( designatorObject );
                        }
                        else
                        {
                            // Prefers to be beside a specific designator
                            var designatorIndex = designationCategory.ResolvedDesignators().FindIndex( d => (
                                ( d.GetType() == injectionSet.designatorNextTo )
                            ) );

                            if( designatorIndex < 0 )
                            {
                                // Other designator doesn't exist (yet?)
                                // Inject the designator at the end
                                designationCategory.ResolvedDesignators().Add( designatorObject );
                            }
                            else
                            {
                                // Inject beside desired designator
                                designationCategory.ResolvedDesignators().Insert( designatorIndex + 1, designatorObject );
                            }
                        }
                    }

                    // Now inject the designator class into the list of classes as a saftey net for another mod resolving the category
                    if( !designationCategory.specialDesignatorClasses.Exists( s => s == injectionSet.designatorClass ) )
                    {
                        if( injectionSet.designatorNextTo == null )
                        {
                            // Inject the designator class at the end of the list
                            designationCategory.specialDesignatorClasses.Add( injectionSet.designatorClass );
                        }
                        else
                        {
                            // Prefers to be beside a specific designator
                            var designatorIndex = designationCategory.specialDesignatorClasses.FindIndex( s => s == injectionSet.designatorNextTo );

                            if( designatorIndex < 0 )
                            {
                                // Can't find desired designator class
                                // Inject the designator at the end
                                designationCategory.specialDesignatorClasses.Add( injectionSet.designatorClass );
                            }
                            else
                            {
                                // Inject beside desired designator class
                                designationCategory.specialDesignatorClasses.Insert( designatorIndex + 1, injectionSet.designatorClass );
                            }
                        }
                    }
                }
                if( injectionSet.reverseDesignator )
                {
                    if( ReverseDesignatorDatabase_Extensions.Find( injectionSet.designatorClass ) == null )
                    {
                        ReverseDesignatorDatabase.AllDesignators.Add( (Designator) Activator.CreateInstance( injectionSet.designatorClass ) );
                    }
                }
            }

            return true;
        }

    }

}
