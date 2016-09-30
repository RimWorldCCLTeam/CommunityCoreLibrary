using System;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary.Controller
{

    internal static class SubControllers
    {
        
        internal static bool                Create()
        {
            // Find all sub-controllers
            var subControllerClasses = typeof( SubController ).AllSubclasses();
            var subControllerCount = subControllerClasses.Count();
            if( subControllerCount == 0 )
            {
                CCL_Log.Error(
                    "Unable to find types of class 'SubController'",
                    "SubControllers.Create"
                );
                return false;
            }

            var subControllers = new SubController[ subControllerCount ];
            for( int index = 0; index < subControllerCount; ++index )
            {
                var subControllerType = subControllerClasses.ElementAt( index );
                var subController = (SubController) Activator.CreateInstance( subControllerType );
                if( subController == null )
                {
                    CCL_Log.Error(
                        string.Format(
                            "Unable to create SubController '{0}'",
                            subControllerType.Name ),
                        "SubControllers.Create"
                    );
                    return false;
                }
                else
                {
                    subControllers[ index ] = subController;
                }
            }
            Controller.Data.SubControllers = subControllers;
            return true;
        }

        internal static bool                Valid()
        {
            // Validate all subs-systems
            var subControllers = Controller.Data.SubControllers.ToList();
            subControllers.Sort( (x,y) => ( x.ValidationPriority > y.ValidationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( subsys.ValidationPriority != SubController.DontProcessThisPhase )
                {
                    if( !subsys.Validate() )
                    {
                        CCL_Log.Error(
                            subsys.strReturn,
                            subsys.Name + " :: Validation"
                        );
                        return false;
                    }
                    if( subsys.strReturn != string.Empty )
                    {
                        CCL_Log.Message(
                            subsys.strReturn,
                            subsys.Name + " :: Validation"
                        );
                    }
                }
                else
                {
                    subsys.State = SubControllerState.Validated;
                }
            }
            return true;
        }

        internal static bool                Initialize()
        {
            // Initialize all sub-systems
            var subControllers = Controller.Data.SubControllers.ToList();
            subControllers.Sort( (x,y) => ( x.InitializationPriority > y.InitializationPriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( subsys.InitializationPriority != SubController.DontProcessThisPhase )
                {
                    if( !subsys.Initialize() )
                    {
                        CCL_Log.Error(
                            subsys.strReturn,
                            subsys.Name + " :: Initialization"
                        );
                        return false;
                    }
                    if( subsys.strReturn != string.Empty )
                    {
                        CCL_Log.Message(
                            subsys.strReturn,
                            subsys.Name + " :: Initialization"
                        );
                    }
                }
                else
                {
                    subsys.State = SubControllerState.Ok;
                }
            }
            return true;
        }

        internal static void                Update()
        {
            // Update all sub-systems
            var subControllers = Controller.Data.SubControllers.ToList();
            subControllers.Sort( (x,y) => ( x.UpdatePriority > y.UpdatePriority ) ? -1 : 1 );
            foreach( var subsys in subControllers )
            {
                if( subsys.UpdatePriority != SubController.DontProcessThisPhase )
                {
                    if(
                        ( subsys.State == SubControllerState.Ok )&&
                        ( subsys.IsHashIntervalTick( Controller.Data.LibraryTicks ) )
                    )
                    {
                        if( !subsys.Update() )
                        {
                            CCL_Log.Error( subsys.strReturn, subsys.Name + " :: Update" );
                            return;
                        }
                        if( subsys.strReturn != string.Empty )
                        {
                            CCL_Log.Message( subsys.strReturn, subsys.Name + " :: Update" );
                        }
                    }
                }
            }

        }

    }

}
