using System.Collections.Generic;
using System.Text;

using RimWorld;
using Verse;
using CommunityCoreLibrary.Commands;

namespace CommunityCoreLibrary
{

    public class CompColoredLight : ThingComp
    {
        #region Private vars

        int                                 ColorIndex = -1;
        float                               lightRadius;
        
        CompProperties_ColoredLight         ColorProps;
        
        CompGlower                          CompGlower
        {
            get
            {
                return parent.TryGetComp< CompGlower >();
            }
        }

        #endregion

        #region Gizmos

        ChangeColor                 _GizmoChangeColor;
        ChangeColor                 GizmoChangeColor
        {
            get
            {
                if( _GizmoChangeColor == null )
                {
                    _GizmoChangeColor = new ChangeColor(
                        this,
                        Icon.SelectLightColor
                    );
                }
                return _GizmoChangeColor;
            }
        }

        TouchingByLinker            _GizmoTouchingByLinker;
        TouchingByLinker            GizmoTouchingByLinker
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

        DefOrThingCompInRoom        _GizmoDefOrThingCompInRoom;
        DefOrThingCompInRoom        GizmoDefOrThingCompInRoom
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
        }

        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();
            
            // Get the default glower
#if DEBUG
            if( CompGlower == null )
            {
                CCL_Log.Error( "CompColoredLight requires CompGlower!", parent.def.defName );
                return;
            }
#endif

            // Get the color properties
            ColorProps = this.CompProperties_ColoredLight();
#if DEBUG
            if( ColorProps == null )
            {
                CCL_Log.Error( "CompColoredLight requires CompProperties_ColoredLight!", parent.def.defName );
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
            lightRadius = CompGlower.props.glowRadius;

            // Set the light color
            ChangeColor( ColorIndex );
        }

        public override string              CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append( "CompColorLightInspect".Translate() + ColorProps.color[ ColorIndex ].name );
            return stringBuilder.ToString();
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
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
        void                                GroupColorChange( List< Thing > things )
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
                        ( otherColor > -1 )&&
                        ( DefDatabase< ResearchProjectDef >.GetNamed( otherLight.ColorProps.requiredResearch ).IsFinished )
                    )
                    {    // Change it's color
                        otherLight.ChangeColor( otherColor );
                    }
                }
            }
        }

        // Replace the comp props with a new one with different values
        // must replace comp props as comps share props for things of the
        // same class.  We need to make a unique copy for the building.
        public void                         ChangeColor( int index )
        {
            ColorInt color = ColorProps.color[ index ].value;
            ColorIndex = index;
            GizmoChangeColor.defaultDesc = GizmoChangeColor.Desc;

            // Get glower
            var glower = CompGlower;

            // Current lit state of glower
            bool wasLit = glower.Lit;

            // Turn off glower
            glower.Lit = false;

            // New glower properties
            var newProps = new CompProperties();
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
            Find.GlowGrid.MarkGlowGridDirty( parent.Position );
            Find.MapDrawer.MapMeshDirty( parent.Position, MapMeshFlag.GroundGlow );
            Find.MapDrawer.MapMeshDirty( parent.Position, MapMeshFlag.Things );

            // Turn light on if appropriate
            glower.Lit = wasLit;
        }

        #endregion
    }

}
