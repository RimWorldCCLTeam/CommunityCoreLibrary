using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _GhostDrawer
    {

        internal static Dictionary<int, Graphic> __ghostGraphics;
        internal static Dictionary<int, Graphic> _ghostGraphics
        {
            get
            {
                if( __ghostGraphics == null )
                {
                    __ghostGraphics = typeof( GhostDrawer ).GetField( "ghostGraphics", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( null ) as Dictionary<int, Graphic>;
                }
                return __ghostGraphics;
            }
        }

        internal static Graphic _GhostGraphicFor( Graphic baseGraphic, ThingDef thingDef, Color ghostCol )
        {
            int key = baseGraphic.GetHashCode() * 399 ^ thingDef.GetHashCode() * 391 ^ ghostCol.GetHashCode() * 415;
            Graphic graphic;
            if( !_GhostDrawer._ghostGraphics.TryGetValue( key, out graphic ) )
            {
                graphic =
                    ( thingDef.graphicData.Linked )||
                    (
                        ( thingDef.thingClass == typeof( Building_Door ) )||
                        ( thingDef.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                    )
                    ? GraphicDatabase.Get<Graphic_Single>( thingDef.uiIconPath, ShaderDatabase.Transparent, thingDef.graphicData.drawSize, ghostCol )
                    : (
                        ( baseGraphic == null )
                        ? thingDef.graphic.GetColoredVersion( ShaderDatabase.Transparent, ghostCol, Color.white )
                        : baseGraphic.GetColoredVersion( ShaderDatabase.Transparent, ghostCol, Color.white )
                    );
                _GhostDrawer._ghostGraphics.Add( key, graphic );
            }
            return graphic;
        }

    }

}
