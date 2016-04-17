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
            int key = Gen.HashCombineStruct<Color>( Gen.HashCombine<ThingDef>( Gen.HashCombine<Graphic>( 0, baseGraphic ), thingDef ), ghostCol );
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
                        ? GraphicDatabase.Get( thingDef.graphic.GetType(), thingDef.graphic.path, ShaderDatabase.Transparent, thingDef.graphic.drawSize, ghostCol, Color.white )
                        : GraphicDatabase.Get( baseGraphic.GetType(), baseGraphic.path, ShaderDatabase.Transparent, baseGraphic.drawSize, ghostCol, Color.white )
                    );
                _GhostDrawer._ghostGraphics.Add( key, graphic );
            }
            return graphic;
        }

    }

}
