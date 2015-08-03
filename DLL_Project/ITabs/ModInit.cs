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

    public class ModInit : ITab
    {
        protected GameObject    gameObject = null;

        public ModInit()
        {
            if( gameObject == null ){
                gameObject = new GameObject( "CCLController" );
                gameObject.AddComponent< Controller >();
            }
        }

        protected override void FillTab()
        {
        }
    }
}

