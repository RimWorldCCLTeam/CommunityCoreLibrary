using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

using CommunityCoreLibrary.Controller;

/*
    TODO:  Alpha 13 API change

    Can't change yet otherwise existing saves will get null errors or name clashes

    Remove completely, all functionality is now handled by a sub controller off the
    main MonoBehaviour.

    For now, just an empty class which does nothing but keeps existing save games from
    throwing a null exception error.

*/

namespace CommunityCoreLibrary
{
	
    public class ResearchController : MapComponent
	{
	}

}
