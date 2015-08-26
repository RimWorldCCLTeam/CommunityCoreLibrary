using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ResearchProjectDef_Extensions
    {

        public static bool                  IsLockedOut( this ResearchProjectDef r )
        {
            if( ( r == null )||
                ( r.prerequisites == null )||
                ( r.prerequisites.Count == 0 ) )
            {
                return false;
            }
            foreach( var p in r.prerequisites )
            {
                if( p.defName == r.defName )
                {
                    // Self-prerequisite means potential lock-out

                    // Check for possible unlock
                    if( !ResearchController.AdvancedResearch.Exists( a => (
                        ( a.IsResearchToggle )&&
                        ( !a.HideDefs )&&
                        ( a.effectedResearchDefs.Contains( p ) )
                    ) ) )
                    {
                        // No unlockers, always locked out
                        return true;
                    }
                }
                else if( p.IsLockedOut() )
                {
                    // Any of the research parents locked out?
                    return true;
                }
            }
            return false;
        }

    }

}
