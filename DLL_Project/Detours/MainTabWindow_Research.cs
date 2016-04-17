using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _MainTabWindow_Research
    {

        #region Detoured Methods

        internal static bool _NotFinishedNotLockedOut( ResearchProjectDef project )
        {
            return ( !project.IsFinished )&&( !project.IsLockedOut() );
        }

        #endregion

    }

}
