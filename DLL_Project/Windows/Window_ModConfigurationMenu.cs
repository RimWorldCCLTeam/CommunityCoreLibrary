using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class Window_ModConfigurationMenu : Window
	{

		private class MenuWorker : IExposable
		{
			public string Label;
			public ModConfigurationMenu worker;

            public bool OpenedThisSession;

            private string _key;
            public string key
            {
                get
                {
                    if( string.IsNullOrEmpty( _key ) )
                    {
                        _key = Label.Replace( " ", "_" );
                    }
                    return _key;
                }
            }

			public MenuWorker()
			{
				this.Label = "?";
				this.worker = null;
                this._key = "";
                this.OpenedThisSession = false;
			}

			public MenuWorker( string Label, ModConfigurationMenu worker )
			{
				this.Label = Label;
				this.worker = worker;
                this._key = "";
                this.OpenedThisSession = false;
			}

			public void ExposeData()
			{
				if( worker == null )
				{
					CCL_Log.Trace(
						Verbosity.FatalErrors,
						string.Format( "worker is null in MenuWorkers for {0}", Label ),
						"Mod Configuration Menu" );
					return;
				}
				// Call the worker expose data
				worker.ExposeData();
			}

		}

		#region Control Constants

		public const string ConfigFilePrefix = "MCM_Data_";
		public const string ConfigFileSuffix = ".xml";

		public const float MinListWidth = 200f;

		public const float Margin = 6f; // 15 is way too much.
		public const float EntryHeight = 30f;

		#endregion

		#region Instance Data

		protected Rect SelectionRect;
		protected Rect DisplayRect;

		protected Vector2 SelectionScrollPos = default( Vector2 );
		protected Vector2 DisplayScrollPos = default( Vector2 );

		public float SelectionHeight = 9999f;
		public float ContentHeight = 9999f;

		private MenuWorker SelectedMenu;
        private MenuWorker PreviouslySelectedMenu;

		private static string _filterString = "";
		private string _lastFilterString = "";
		private int _lastFilterTick;
		private bool _filtered;
		private List<MenuWorker> filteredMenus;

		private static List<MenuWorker> allMenus;

		public static bool AnyMenus
		{
			get
			{
				return !allMenus.NullOrEmpty();
			}
		}

		#endregion

		public override Vector2 InitialWindowSize
		{
			get
			{
				return new Vector2( 600f, 600f );
			}
		}

		#region Constructor

		public static bool InitializeMCMs( bool preload = false )
		{
            if( preload )
            {
			    allMenus = new List<MenuWorker>();
            }

			// Get the mods with config menus
			foreach( var mhd in Controller.Data.ModHelperDefs )
			{
                if( // Filter out preload during non-preload and non-preload during preload
                    (
                        ( preload )&&
                        ( mhd.PreloadMCMs )
                    )||
                    (
                        ( !preload )&&
                        ( !mhd.PreloadMCMs )
                    )
                )
                {   // Create all the menus for it
    				if( !mhd.ModConfigurationMenus.NullOrEmpty() )
    				{
    					foreach( var mcm in mhd.ModConfigurationMenus )
    					{
                            var menu = allMenus.Find( m => m.worker.InjectionSet == mcm );
                            if( menu != null )
                            {   // MCM already created....?
                                CCL_Log.TraceMod(
                                    mhd,
                                    Verbosity.Warnings,
                                    string.Format( "{0} - Tried to create an MCM when an MCM already exists", mcm.mcmClass.ToString() )
                                );
                                continue;
                            }
    						menu = new MenuWorker();
    						menu.Label = mcm.label;
    						menu.worker = (ModConfigurationMenu)Activator.CreateInstance( mcm.mcmClass );
    						if( menu.worker == null )
    						{
                                CCL_Log.Error( string.Format( "Unable to create instance of {0}", mcm.mcmClass.ToString() ) );
                                return false;
    						}
    						else
    						{   // Initialize, add it to the menu list and then load it's data
                                menu.worker.InjectionSet = mcm;
                                menu.worker.Initialize();
    							allMenus.Add( menu );
    							LoadMCMData( menu );
    						}
    					}
    				}
                }
			}
            return true;
		}

		public Window_ModConfigurationMenu()
		{
			layer = WindowLayer.GameUI;
			soundAppear = null;
			soundClose = null;
			doCloseButton = false;
			doCloseX = true;
			closeOnEscapeKey = true;
			forcePause = true;
			filteredMenus = allMenus.ListFullCopy();
		}

		#endregion

		#region Load/Save MCM Data

		static string MCMFilePath( MenuWorker menu )
		{
			// Generate the config file name
			string filePath = Path.Combine( GenFilePaths.ConfigFolderPath, ConfigFilePrefix );
            filePath += menu.key;
			filePath += ConfigFileSuffix;
			return filePath;
		}

		static void LoadMCMData( MenuWorker menu )
		{
			var filePath = MCMFilePath( menu );

			if( !File.Exists( filePath ) )
			{
				return;
			}

			try
			{
				// Open it for reading
				Scribe.InitLoading( filePath );
				if( Scribe.mode == LoadSaveMode.LoadingVars )
				{
					// Version check
					string version = "";
					Scribe_Values.LookValue<string>( ref version, "ccl_version" );

					bool okToLoad = true;
					var result = Version.Compare( version );
					if( result == Version.VersionCompare.GreaterThanMax )
					{
						CCL_Log.Trace(
							Verbosity.NonFatalErrors,
							string.Format( "Data for {0} is newer ({1}) than the version you are using ({2}).", menu.Label, version, Version.Current.ToString() ),
							"Mod Configuration Menu" );
						okToLoad = false;
					}
					else if( result == Version.VersionCompare.Invalid )
					{
						CCL_Log.Trace(
							Verbosity.NonFatalErrors,
							string.Format( "Data for {0} is corrupt and will be discarded", menu.Label ),
							"Mod Configuration Menu" );
						okToLoad = false;
					}

					if( okToLoad )
					{
						// Call the worker scribe
						var args = new object[]
						{
							menu.Label,
							menu.worker
						};
						Scribe_Deep.LookDeep<MenuWorker>( ref menu, menu.key, args );
					}
				}
			}
			catch( Exception e )
			{
				CCL_Log.Trace(
					Verbosity.NonFatalErrors,
					string.Format( "Unexpected error scribing data for mod {0}\n{1}", menu.Label, e.ToString() ),
					"Mod Configuration Menu" );
			}
			finally
			{
				// Finish
				Scribe.FinalizeLoading();
				Scribe.mode = LoadSaveMode.Inactive;
			}
		}

		public override void PreClose()
		{
            if( SelectedMenu != null )
            {
                SelectedMenu.worker.PostClose();
            }
			base.PreClose();

			for( int index = 0; index < allMenus.Count; ++index )
			{
				// Get menu to work with
				var menu = allMenus[ index ];

                if( menu.OpenedThisSession )
                {
    				var filePath = MCMFilePath( menu );

    				// Open it for writing
    				try
    				{
    					Scribe.InitWriting( filePath, "ModConfigurationData" );
    					if( Scribe.mode == LoadSaveMode.Saving )
    					{
    						// Write this library version as the one saved with
    						string version = Version.Current.ToString();
    						Scribe_Values.LookValue<string>( ref version, "ccl_version" );

    						// Call the worker scribe
    						Scribe_Deep.LookDeep<MenuWorker>( ref menu, menu.key );
    					}
    				}
    				catch( Exception e )
    				{
    					CCL_Log.Trace(
    						Verbosity.NonFatalErrors,
    						string.Format( "Unexpected error scribing data for mod {0}\n{1}", menu.Label, e.ToString() ),
    						"Mod Configuration Menu" );
    				}
    				finally
    				{
    					// Finish
    					Scribe.FinalizeWriting();
    					Scribe.mode = LoadSaveMode.Inactive;
                        Messages.Message( "ModConfigurationSaved".Translate( menu.Label ), MessageSound.Standard );
    				}
                }
                menu.OpenedThisSession = false;
			}

		}

		#endregion

		#region Filter

		private void _filterUpdate()
		{
			// filter after a short delay.
			// Log.Message(_filterString + " | " + _lastFilterTick + " | " + _filtered);
			if( _filterString != _lastFilterString )
			{
				_lastFilterString = _filterString;
				_lastFilterTick = 0;
				_filtered = false;
			}
			else if( !_filtered )
			{
				if( _lastFilterTick > 60 )
				{
					Filter();
				}
				_lastFilterTick++;
			}
		}

		public void Filter()
		{
			filteredMenus.Clear();
			foreach( var menu in allMenus )
			{
				if( string.IsNullOrEmpty( _filterString ) )
				{
					filteredMenus.Add( menu );
				}
				else if( menu.Label.Contains( _filterString ) )
				{
					filteredMenus.Add( menu );
				}
			}
			_filtered = true;
		}

		public void ResetFilter()
		{
			_filterString = "";
			_lastFilterString = "";
			Filter();
		}

		#endregion

		#region Window Rendering

		public override void DoWindowContents( Rect rect )
		{
			if( Game.Mode == GameMode.Entry )
			{
				absorbInputAroundWindow = true;
			}
			else
			{
				absorbInputAroundWindow = false;
			}

			Text.Font = GameFont.Small;

			GUI.BeginGroup( rect );

			SelectionRect = new Rect( 0f, 0f, MinListWidth, rect.height );
			DisplayRect = new Rect(
				SelectionRect.width + Margin, 0f,
				rect.width - SelectionRect.width - Margin, rect.height
			);

			DrawSelectionArea( SelectionRect );
			DrawDisplayArea( DisplayRect );

			GUI.EndGroup();
		}

		#endregion

		#region Selection Area Rendering

		void DrawSelectionArea( Rect rect )
		{
			Widgets.DrawMenuSection( rect );

			_filterUpdate();
			var filterRect = new Rect( rect.xMin + Margin, rect.yMin + Margin, rect.width - 3 * Margin - 30f, 30f );
			var clearRect = new Rect( filterRect.xMax + Margin + 3f, rect.yMin + Margin + 3f, 24f, 24f );
			_filterString = Widgets.TextField( filterRect, _filterString );
			if( _filterString != "" )
			{
				if( Widgets.ImageButton( clearRect, Widgets.CheckboxOffTex ) )
				{
					ResetFilter();
				}
			}

			Rect outRect = rect;
			outRect.yMin += 40f;
			outRect.xMax -= 2f; // some spacing around the scrollbar

			float viewWidth = SelectionHeight > outRect.height ? outRect.width - 16f : outRect.width;
			var viewRect = new Rect( 0f, 0f, viewWidth, SelectionHeight );

			GUI.BeginGroup( outRect );
			Widgets.BeginScrollView( outRect.AtZero(), ref SelectionScrollPos, viewRect );

			if( !filteredMenus.NullOrEmpty() )
			{
				Vector2 cur = Vector2.zero;

				foreach( var menu in filteredMenus )
				{
					if( DrawModEntry( ref cur, viewRect, menu ) )
					{
						SelectedMenu = menu;
					}
				}

				SelectionHeight = cur.y;
			}

			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		private bool DrawModEntry( ref Vector2 cur, Rect view, MenuWorker menu )
		{
			float width = view.width - cur.x - Margin;
			float height = EntryHeight;
			string label = menu.Label;

			if( Text.CalcHeight( label, width ) > EntryHeight )
			{
				Text.Font = GameFont.Tiny;
				float height2 = Text.CalcHeight( label, width );
				height = Mathf.Max( height, height2 );
			}

			Text.Anchor = TextAnchor.MiddleLeft;
			Rect labelRect = new Rect( cur.x + Margin, cur.y, width - Margin, height );
			Widgets.Label( labelRect, label );
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;

			// full viewRect width for overlay and button
			Rect buttonRect = view;
			buttonRect.yMin = cur.y;
			cur.y += height;
			buttonRect.yMax = cur.y;
			GUI.color = Color.grey;
			Widgets.DrawLineHorizontal( view.xMin, cur.y, view.width );
			GUI.color = Color.white;
			if( SelectedMenu == menu )
			{
				Widgets.DrawHighlightSelected( buttonRect );
			}
			else
			{
				Widgets.DrawHighlightIfMouseover( buttonRect );
			}
			return Widgets.InvisibleButton( buttonRect );
		}

		#endregion

		#region Mod Configuration Menu Rendering

		void DrawDisplayArea( Rect rect )
		{
			Widgets.DrawMenuSection( rect );

			if( SelectedMenu == null )
			{
				return;
			}
            if(
                ( PreviouslySelectedMenu != null )&&
                ( PreviouslySelectedMenu != SelectedMenu )
            )
            {
                PreviouslySelectedMenu.worker.PostClose();
            }
            if( PreviouslySelectedMenu != SelectedMenu )
            {
                SelectedMenu.OpenedThisSession = true;
                SelectedMenu.worker.PreOpen();
            }
            PreviouslySelectedMenu = SelectedMenu;

			Text.Font = GameFont.Medium;
			Text.WordWrap = false;
			var titleRect = new Rect( rect.xMin, rect.yMin, rect.width, 60f );
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label( titleRect, SelectedMenu.Label );

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;

			Rect outRect = rect.ContractedBy( Margin );
			outRect.yMin += 60f;
			Rect viewRect = outRect;
			viewRect.width -= 16f;
			viewRect.height = ContentHeight;

			GUI.BeginGroup( outRect );
			Widgets.BeginScrollView( outRect.AtZero(), ref DisplayScrollPos, viewRect.AtZero() );

            bool userError = false;
            string userErrorStr = string.Empty;
            try
            {
                ContentHeight = SelectedMenu.worker.DoWindowContents( viewRect.AtZero() );
            }
            catch( Exception e )
            {
                userError = true;
                userErrorStr = e.ToString();
            }

			Widgets.EndScrollView();
			GUI.EndGroup();

            if( userError )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    userErrorStr,
                    "Mod Configuration Menu"
                );
            }
		}

		#endregion

	}

}
