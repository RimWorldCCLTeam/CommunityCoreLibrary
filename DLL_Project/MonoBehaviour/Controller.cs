using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

    public class Controller : MonoBehaviour
    {
        public static readonly string   GameObjectName = "Community Core Library";
        public Version                  version;

        List< CCLVersionDef >           cclModVersion = new List< CCLVersionDef >();

        public virtual void Start()
        {
            this.enabled = true;

            // Check versions of mods and throw errors to the user is the
            // mod version requirement is higher than the installed version

            GetCCLVersion();
            GetModVersionRequirements();

            this.enabled = false;
        }

        private void GetCCLVersion ()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;
            Log.Message( "Community Core Library v" + version );
        }

        private void GetModVersionRequirements()
        {
            cclModVersion = DefDatabase< CCLVersionDef >.AllDefs.ToList();
            if( ( cclModVersion == null )||
                ( cclModVersion.Count < 1 ) )
                return;

            foreach( var mv in cclModVersion ){
                var modVersion = new Version( mv.version );
                if( modVersion > version )
                    Log.Error( "Mod " + mv.ModName + " requires CCL version " + modVersion );
            }

        }
    }
}
