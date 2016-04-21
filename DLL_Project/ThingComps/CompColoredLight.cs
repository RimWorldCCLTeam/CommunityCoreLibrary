using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using RimWorld;
using Verse;
using CommunityCoreLibrary.Commands;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class CompColoredLight : ThingComp
    {
        #region Private vars

        int                                 ColorIndex      = -1;
        Color                               Color           = new Color( 1f, 1f, 1f, 0f );
        float                               lightRadius;
        
        CompProperties_ColoredLight         ColorProps;

        public CompGlower                   CompGlower;

        #endregion

        #region Gizmos

        ChangeColor                         _GizmoChangeColor;
        ChangeColor                         GizmoChangeColor
        {
            get
            {
                if( _GizmoChangeColor == null )
                {
                    _GizmoChangeColor = new ChangeColor(
                        this,
                        Icon.SelectLightColor,
                        ColorProps.useColorPicker
                    );
                }
                return _GizmoChangeColor;
            }
        }

        TouchingByLinker                    _GizmoTouchingByLinker;
        TouchingByLinker                    GizmoTouchingByLinker
        {
            get
            {
                if( _GizmoTouchingByLinker == null )
                {
                    _GizmoTouchingByLinker = new TouchingByLinker(
                        parent,
                        Icon.ShareLightColor,
                        GroupColorChange,
                        GroupColorChange
                    );
                }
                return _GizmoTouchingByLinker;
            }
        }

        DefOrThingCompInRoom                _GizmoDefOrThingCompInRoom;
        DefOrThingCompInRoom                GizmoDefOrThingCompInRoom
        {
            get
            {
                if( _GizmoDefOrThingCompInRoom == null )
                {
                    _GizmoDefOrThingCompInRoom = new DefOrThingCompInRoom(
                        parent,
                        GetType(),
                        "CommandChangeRoommateColorLabel".Translate(),
                        Icon.ShareLightColor
                    );
                    _GizmoDefOrThingCompInRoom.LabelByDef = Translator.Translate( "CommandChangeRoommateColorLClick", parent.def.label );
                    _GizmoDefOrThingCompInRoom.ClickByDef = GroupColorChange;
                    _GizmoDefOrThingCompInRoom.LabelByThingComp = "CommandChangeRoommateColorRClick".Translate();
                    _GizmoDefOrThingCompInRoom.ClickByThingComp = GroupColorChange;
                    _GizmoDefOrThingCompInRoom.defaultDesc = _GizmoDefOrThingCompInRoom.Desc;
                }
                return _GizmoDefOrThingCompInRoom;
            }
        }

        #endregion

        #region Base ThingComp overrides

        public override void                PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue<int>( ref ColorIndex, "ColorIndex", -1 );
            Scribe_Values.LookValue( ref Color, "Color", new Color( 1f, 1f, 1f, 0f ) );
        }

        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();
            
            // Get the default glower
            CompGlower = parent.TryGetComp< CompGlower >();
#if DEBUG
            if( CompGlower == null )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompGlower"
                );
                return;
            }
#endif

            // Get the color properties
            ColorProps = this.CompProperties_ColoredLight();
#if DEBUG
            if( ColorProps == null )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompProperties_ColoredLight"
                );
                return;
            }
#endif

            // Set default palette if none is specified
            if( ColorProps.color == null )
            {
                ColorProps.color = Light.Color;
            }

            // Set default
            if( ( ColorIndex < 0 )||
                ( ColorIndex >= ColorProps.color.Count ) )
            {
                ColorIndex = ColorProps.Default;
            }

            // Get the glow radius
            lightRadius = CompGlower.Props.glowRadius;

            // Set the light color
            if ( ColorProps.useColorPicker )
            {
                ChangeColor( Color, false );
            }
            else
            {
                ChangeColor( ColorIndex );
            }
        }

        public override string              CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append( "CompColorLightInspect".Translate() );
            if ( ColorProps.useColorPicker )
            {
                return stringBuilder.ToString();
            }
            stringBuilder.Append( ColorProps.color[ ColorIndex ].name );
            return stringBuilder.ToString();
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if ( ColorProps.hideGizmos )
            {
                yield break;
            }
            
            // Color change gizmo (only if research is complete)
            if( DefDatabase< ResearchProjectDef >.GetNamed( ColorProps.requiredResearch ).IsFinished )
            {
                yield return GizmoChangeColor;
            }

            // In room
            if( parent.Position.IsInRoom() )
            {
                // In room by def or comp
                if( parent.IsSameThingCompInRoom( GetType() ) )
                {
                    yield return GizmoDefOrThingCompInRoom;
                }
            }

            // Has a link flag
            if( parent.def.graphicData.linkFlags != LinkFlags.None )
            {
                // Touching things by link
                if( parent.IsSameGraphicLinkerTouching() )
                {
                    yield return GizmoTouchingByLinker;
                }
            }

            // No more gizmos
            yield break;
        }

        #endregion

        #region Public functions

        public void                         DecrementColorIndex()
        {
            int index = ( ColorIndex + ColorProps.color.Count - 1 ) % ColorProps.color.Count;
            ChangeColor( index );
        }

        public void                         IncrementColorIndex()
        {
            int index = ( ColorIndex + 1 ) % ColorProps.color.Count;
            ChangeColor( index );
        }

        public int                          GetColorByName( string name )
        {
            for( int i = 0, count = ColorProps.color.Count; i < count; i++ )
            {
                var cv = ColorProps.color[ i ];
                if( cv.name == name )
                {
                    return i;
                }
            }
            return -1;
        }

        public string                       PrevColorName()
        {
            int index = ( ColorIndex + ColorProps.color.Count - 1 ) % ColorProps.color.Count;
            return ColorProps.color[ index ].name;
        }

        public string                       NextColorName()
        {
            int index = ( ColorIndex + 1 ) % ColorProps.color.Count;
            return ColorProps.color[ index ].name;
        }

        #endregion

        #region Gizmo callbacks

        // The list of things are all the lights we want to change
        public void                                GroupColorChange( List< Thing > things )
        {
            // Now set their color (if *their* research is complete)
            foreach( Thing l in things )
            {
                // Get it's colored light comp
                var otherLight = l.TryGetComp< CompColoredLight >();
                if( otherLight != null )
                {
                    // Do they even have this color?
                    int otherColor = otherLight.GetColorByName( ColorProps.color[ ColorIndex ].name );

                    // If it does, check it's research
                    if(
                        ( otherColor > -1  || otherLight.ColorProps.useColorPicker && ColorProps.useColorPicker ) &&
                        ( DefDatabase< ResearchProjectDef >.GetNamed( otherLight.ColorProps.requiredResearch ).IsFinished )
                    )
                    {    
                        // Change it's color
                        if ( ColorProps.useColorPicker && otherLight.ColorProps.useColorPicker )
                        {
                            otherLight.ChangeColor( CompGlower.Props.glowColor );
                        }
                        else
                        {
                            otherLight.ChangeColor( otherColor );
                        }
                    }
                }
            }
        }

        public void                         ChangeColor( int index )
        {
            ColorInt color = ColorProps.color[index].value;
            ColorIndex = index;
            GizmoChangeColor.defaultDesc = GizmoChangeColor.Desc;

            ChangeColor( color );
        }

        public void                         ChangeColor( Color color, bool zeroAlpha = true )
        {
            Color _color = color;
            if ( zeroAlpha )
            {
                _color.a = 0f;
            }
            ChangeColor( new ColorInt( _color ) );
        }

        // Replace the comp props with a new one with different values
        // must replace comp props as comps share props for things of the
        // same class.  We need to make a unique copy for the building.
        public void                         ChangeColor( ColorInt color )
        { 
            // Get glower
            var glower = CompGlower;

            // New glower properties
            var newProps = new CompProperties_Glower();
            if( newProps == null )
            {
                CCL_Log.Error( "CompColoredLight unable to create new CompProperties!", parent.def.defName );
                return;
            }

            // Set the new properties values
            newProps.compClass = typeof( CompGlower );
            newProps.glowColor = color;
            newProps.glowRadius = lightRadius;

            // Initialize comp with new properties
            glower.Initialize( newProps );

            // Update glow grid
            //glower.UpdateLit(); <-- Only works if the light changes state (on<->off)
            Find.GlowGrid.MarkGlowGridDirty( parent.Position );
        }

        #endregion
    }

}
