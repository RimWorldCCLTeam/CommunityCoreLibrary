using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _ThingDef
    {

        internal static void _PostLoad( this ThingDef obj )
        {
#if DEVELOPER
            CCL_Log.Write( "ThingDef( " + obj.defName + " ).PostLoad()" );
#endif
            List<VerbProperties> verbs = typeof( ThingDef ).GetField( "verbs", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( obj ) as List<VerbProperties>;

            if( obj.graphicData != null )
            {
                if( obj.graphicData.shaderType == ShaderType.None )
                {
                    obj.graphicData.shaderType = ShaderType.Cutout;
                }
                obj.graphic = obj.graphicData.Graphic;
            }
            if(
                ( verbs != null )&&
                ( verbs.Count == 1 )
            )
            {
                verbs[ 0 ].label = obj.label;
            }

            // base.PostLoad() // BuildableDef.PostLoad()
            #region BuildableDef PostLoad()

            // base.PostLoad() // Def.PostLoad()
            #region Def PostLoad()

            // base.PostLoad() // Entity.PostLoad()
            #region Entity PostLoad()
            #endregion

            ShortHashGiver.GiveShortHash( obj );

            #endregion

            if( !obj.uiIconPath.NullOrEmpty() )
            {
                obj.uiIcon = ContentFinder<Texture2D>.Get( obj.uiIconPath, true );
            }
            else
            {
                if(
                    ( obj.DrawMatSingle != null )&&
                    ( obj.DrawMatSingle != BaseContent.BadMat )
                )
                {
                    obj.uiIcon = (Texture2D) obj.DrawMatSingle.mainTexture;
                }
            }

            #endregion

            if(
                ( obj.category == ThingCategory.Building )&&
                ( obj.building == null )
            )
            {
                obj.building = new BuildingProperties();
            }
            for( int index = 0; index < obj.inspectorTabs.Count; ++index )
            {
                if( obj.inspectorTabsResolved == null )
                {
                    obj.inspectorTabsResolved = new List<ITab>();
                }
                obj.inspectorTabsResolved.Add( ITabManager.GetSharedInstance( obj.inspectorTabs[ index ] ) );
            }
            if(
                ( obj.passability == Traversability.Impassable )||
                (
                    ( obj.thingClass == typeof( Building_Door ) )||
                    ( obj.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                )
            )
            {
                obj.regionBarrier = true;
            }
            if( obj.building != null )
            {
                obj.building.PostLoadSpecial( obj );
            }
            if( obj.plant != null )
            {
                obj.plant.PostLoadSpecial( obj );
            }
            if( obj.ingestible != null )
            {
                obj.ingestible.PostLoadSpecial( obj );
            }
        }

    }

}
