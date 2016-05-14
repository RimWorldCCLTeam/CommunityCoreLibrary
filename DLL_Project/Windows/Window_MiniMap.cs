using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	// TODO:  Handle right-click on minimap icon for float menu options (MiniMap.GetFloatMenuOptions())

	public class Window_MiniMap : Window
	{

		#region Fields

		private float iconMargin = 6f;
		private float iconSize = 24f;

		// the magic value of 20f here comes from Verse.CameraMap.ScreenDollyEdgeWidth
		// it's unlikely to change, didn't want to bother with a reflected value. -- Fluffy.
		private float screenEdgeDollyWidth = 20f;

		public static Rect windowRect;

		#endregion Fields

		#region Constructors

		public Window_MiniMap()
		{
            layer = WindowLayer.GameUI;
			absorbInputAroundWindow = false;
			closeOnClickedOutside = false;
			closeOnEscapeKey = false;
		}

		public Window_MiniMap( Rect canvas ) : this()
		{
			currentWindowRect = canvas;
		}

		#endregion Constructors

		#region Properties

		// remove window padding so the minimap fills the entire available space
		protected override float WindowPadding
		{
			get
			{
				return 0f;
			}
		}

		#endregion Properties

		#region Methods

		public override void DoWindowContents( Rect inRect )
		{
			// draw all minimaps
			foreach( var overlay in MiniMapController.visibleMiniMaps )
			{
				overlay.DrawOverlays( inRect );
			}

			// handle minimap click & drag
			// we're using a combination of IsOver and GetMouseButton
			// because Click only handles the mouse up event, we want
			// to be able to drag the camera.
			if( Mouse.IsOver( inRect ) && Input.GetMouseButton( 0 ) )
			{
				// conveniently, Unity returns the mouse position _within the rect!_
				var mouse = Event.current.mousePosition;

				// inconveniently, the origin of the mouse position is topleft, whereas that of the map is bottomleft.
				// flip vertical axis
				var position = new Vector2( mouse.x, inRect.height - mouse.y );

				// calculate scale
				var scale = new Vector2( Find.Map.Size.x / inRect.width, Find.Map.Size.z / inRect.height );

				// jump map
				Find.CameraMap.JumpTo( new Vector3( position.x * scale.x, 0f, position.y * scale.y ) );
			}

			// TODO: draw additional UI stuff
		}

		public override void ExtraOnGUI()
		{
			// get minimaps we should draw toggles for
			var minimaps = Controller.Data.MiniMaps.Where( overlay => !overlay.miniMapDef.alwaysVisible ).ToArray();

			// how many overlays can we draw on a single line?
			// note that we don't want to draw in the complete outer edge, because that will trigger map movement, which is annoying as fuck.
			int iconsPerRow = Mathf.FloorToInt( ( MiniMapController.windowSize.x - screenEdgeDollyWidth ) / ( iconSize + iconMargin ) );

			// draw a button for each minimap
			for( int i = 0; i < minimaps.Count(); i++ )
			{
				// calculate x, y position to spread over rows
				var minimap = minimaps[ i ];
				int x = i % iconsPerRow;
				int y = i / iconsPerRow;

				// create the rect - right to left, top to bottom
				var iconRect = new Rect(
					currentWindowRect.xMax - screenEdgeDollyWidth - iconSize - x * ( iconMargin + iconSize ),
					currentWindowRect.yMax + iconMargin + y * ( iconMargin + iconSize ),
					iconSize, iconSize );


				// Draw tooltip
				TooltipHandler.TipRegion( iconRect, minimap.ToolTip );

				// grey out draw color if hidden
				GUI.color = minimap.Hidden ? Color.grey : Color.white;

				// handle mouse clicks
				if( Widgets.ImageButton( iconRect, minimap.Icon ) )
				{
					// toggle on LMB
					if( Event.current.button == 0 )
						minimap.Hidden = !minimap.Hidden;

					// if there's any options, open float menu on RMB
					else if( Event.current.button == 1 )
					{
						var options = minimap.GetFloatMenuOptions();
						if( !options.NullOrEmpty() )
						{
							Find.WindowStack.Add( new FloatMenu( options, minimap.LabelCap ) );
						}
					}
				}
			}
			GUI.color = Color.white;
		}

		#endregion Methods

	}

}
