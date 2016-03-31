using System;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _SectionLayer_IndoorMask
    {

        internal static MethodInfo          _HideRainPrimary;

        internal static void _Regenerate( this object obj )
        {
            if( !MatBases.SunShadow.shader.isSupported )
            {
                return;
            }

            Section section = typeof( SectionLayer ).GetField( "section", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( obj ) as Section;
            var sectionLayer = obj as SectionLayer;

            LayerSubMesh subMesh = sectionLayer.GetSubMesh( MatBases.IndoorMask );
            subMesh.Clear( MeshParts.All );
            Building[] innerArray = Find.EdificeGrid.InnerArray;
            CellRect cellRect = new CellRect(
                section.botLeft.x,
                section.botLeft.z,
                17,
                17 );
            cellRect.ClipInsideMap();
            subMesh.verts.Capacity = cellRect.Area * 2;
            subMesh.tris.Capacity = cellRect.Area * 4;
            float y = Altitudes.AltitudeFor( AltitudeLayer.MetaOverlays );
            for( int index1 = cellRect.minX; index1 <= cellRect.maxX; ++index1 )
            {
                for( int index2 = cellRect.minZ; index2 <= cellRect.maxZ; ++index2 )
                {
                    IntVec3 c1 = new IntVec3(index1, 0, index2);
                    if( !(bool)_HideRainPrimary.Invoke( obj, new System.Object[] { c1 } ) )
                    {
                        bool flag1 = GridsUtility.Roofed( c1 );
                        bool flag2 = false;
                        if( flag1 )
                        {
                            for( int index3 = 0; index3 < 8; ++index3 )
                            {
                                IntVec3 c2 = c1 + GenAdj.AdjacentCells[ index3 ];
                                if(
                                    ( GenGrid.InBounds( c2 ) )&&
                                    ( !(bool)_HideRainPrimary.Invoke( obj, new System.Object[] { c2 } ) )
                                )
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                        if(
                            ( !flag1 )||
                            ( !flag2 )
                        )
                        {
                            continue;
                        }
                    }
                    Thing thing = innerArray[ CellIndices.CellToIndex( index1, index2 ) ];
                    float num =
                        ( thing == null )||
                        ( thing.def.passability != Traversability.Impassable )&&
                        (
                            ( thing.def.thingClass != typeof( Building_Door ) )||
                            ( !thing.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                        )
                        ? 0.16f : 0.0f;
                    subMesh.verts.Add( new Vector3( (float)   index1       - num, y, (float)   index2       - num ) );
                    subMesh.verts.Add( new Vector3( (float)   index1       - num, y, (float) ( index2 + 1 ) + num ) );
                    subMesh.verts.Add( new Vector3( (float) ( index1 + 1 ) + num, y, (float) ( index2 + 1 ) + num ) );
                    subMesh.verts.Add( new Vector3( (float) ( index1 + 1 ) + num, y, (float)   index2       - num ) );
                    int count = subMesh.verts.Count;
                    subMesh.tris.Add( count - 4 );
                    subMesh.tris.Add( count - 3 );
                    subMesh.tris.Add( count - 2 );
                    subMesh.tris.Add( count - 4 );
                    subMesh.tris.Add( count - 2 );
                    subMesh.tris.Add( count - 1 );
                }
            }
            if( subMesh.verts.Count <= 0 )
            {
                return;
            }
            subMesh.FinalizeMesh( MeshParts.Verts | MeshParts.Tris );
        }

    }

}
