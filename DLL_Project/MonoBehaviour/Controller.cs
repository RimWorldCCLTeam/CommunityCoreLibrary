using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class Controller : MonoBehaviour
    {
        
        public readonly string              GameObjectName = "Community Core Library";
        Version                             version;

        List< CCLVersionDef >               CCLMods = new List< CCLVersionDef >();

        public virtual void                 Start()
        {
            enabled = true;

            // Check versions of mods and throw error to the user if the
            // mod version requirement is higher than the installed version

            CCLMods = DefDatabase< CCLVersionDef >.AllDefs.ToList();

            GetCCLVersion();
            CheckModVersionRequirements();

            enabled = false;
        }

        void                                GetCCLVersion ()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;
            Log.Message( "Community Core Library v" + version );
        }

        void                                CheckModVersionRequirements()
        {
            if( ( CCLMods == null )||
                ( CCLMods.Count < 1 ) )
                return;

            var throwError = false;
            var errors = "Community Core Library dependency error:";

            foreach( var CCLMod in CCLMods )
            {
                var modVersion = new Version( CCLMod.version );
                if( modVersion > version ){
                    errors += "\n\t" + CCLMod.ModName + " requires v" + modVersion;
                    throwError = true;
                }
            }

            if( throwError )
                Log.Error( errors );

        }

    }

}
