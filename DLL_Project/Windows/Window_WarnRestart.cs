using System;
using System.Collections.Generic;
using System.Diagnostics;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class Window_WarnRestart : Window
	{

		#region Instance Data

        private DateTime OpenedAt;
        private bool okAnyway;
        private DateTime okAnywayStart;

		#endregion

		public override Vector2 InitialWindowSize
		{
			get
			{
				return new Vector2( 400f, 300f );
			}
		}

		#region Constructor

		public Window_WarnRestart()
		{
			layer = WindowLayer.GameUI;
			soundAppear = null;
			soundClose = null;
			doCloseButton = false;
			doCloseX = false;
			closeOnEscapeKey = false;
			forcePause = true;
            absorbInputAroundWindow = true;
		}

		#endregion

		#region ITab Rendering

        public override void PreOpen()
        {
            base.PreOpen();
            OpenedAt = System.DateTime.Now;
            Controller.Data.RestartWarningIsOpen = true;
            Controller.Data.ContinueWithoutRestart = false;
            okAnyway = false;
        }

        private void DrawCountDownLabel( Rect localRect, double TimeLeft )
        {
            var labelText = string.Format( "{0}", (int) TimeLeft );
            var labelSize = Text.CalcSize( labelText );
            var labelRect = new Rect(
                ( localRect.width - labelSize.x ) / 2,
                localRect.height - labelSize.y,
                labelSize.x,
                labelSize.y );
            var oldColor = GUI.color;
            GUI.color = Color.grey;
            Widgets.Label( labelRect, labelText );
            GUI.color = oldColor;
        }

		public override void DoWindowContents( Rect inRect )
        {
			Text.Font = GameFont.Small;

            var localRect = inRect.ContractedBy( 10f );
            GUI.BeginGroup( localRect );

            var warnLabel = "WarnAboutRestart".Translate();
            var warnRect = new Rect( 0, 0, localRect.width, Text.CalcHeight( warnLabel, localRect.width ) );
            Widgets.Label( warnRect, warnLabel );

#if RELEASE
            var CurrentTime = System.DateTime.Now;
            var OpenFor = ( CurrentTime - OpenedAt ).TotalSeconds;
#else
            var OpenFor = (double) 30.1;
#endif

            bool closeMe = false;
            if( OpenFor < 30.0 )
            {
                DrawCountDownLabel( localRect, 30.0 - OpenFor );
            }
            else
            {
                var continueVector = new Vector2( 0, localRect.height - 45f - 16f - 24f );
                var continueRect = new Rect( 28f, continueVector.y, localRect.width - 28f, 24f );
                Widgets.Checkbox( continueVector, ref Controller.Data.ContinueWithoutRestart );
                var continueLabel = "ContinueWithoutRestart".Translate();
                Widgets.Label( continueRect, continueLabel );
                if(
                    ( !okAnyway )&&
                    ( Controller.Data.ContinueWithoutRestart )
                )
                {
                    okAnyway = true;
                    okAnywayStart = System.DateTime.Now;
                }
                if( !Controller.Data.ContinueWithoutRestart )
                {
                    okAnyway = false;
                    var okButtonRect = new Rect( 0, localRect.height - 45f, 100f, 45f );
                    closeMe = Widgets.TextButton( okButtonRect, "Continue".Translate() );
                    var restartButtonRect = new Rect( localRect.width - 100f, localRect.height - 45f, 100f, 45f );
                    if( Widgets.TextButton( restartButtonRect, "RestartNow".Translate() ) )
                    {
                        var args = Environment.GetCommandLineArgs();
                        var commandLine = "\"" + args[ 0 ] + "\"";
                        var arguements = string.Empty;
                        for( int index = 1; index < args.GetLength( 0 ); ++index )
                        {
                            if( index > 1 )
                            {
                                arguements += " ";
                            }
                            arguements += args[ index ];
                        }
                        Log.Message( "Restarting RimWorld:\n" + commandLine + " " + arguements );
                        Process.Start( commandLine, arguements );
                        Root.Shutdown();
                    }
                }
                else
                {
#if RELEASE
                    CurrentTime = System.DateTime.Now;
                    OpenFor = ( CurrentTime - okAnywayStart ).TotalSeconds;
#endif
                    if( OpenFor < 30.0 )
                    {
                        DrawCountDownLabel( localRect, 30.0 - OpenFor );
                    }
                    else
                    {
                        var consequencesLabel = "AcceptTheConsequences".Translate();
                        var consequencesSize = Text.CalcSize( consequencesLabel );
                        consequencesSize.x += 24f;
                        var okButtonRect = new Rect( localRect.width - consequencesSize.x, localRect.height - 45f, consequencesSize.x, 45f );
                        closeMe = Widgets.TextButton( okButtonRect, consequencesLabel );
                    }
                }
            }

            GUI.EndGroup();

            if( closeMe )
            {
                this.Close();
            }
		}

        public override void PostClose()
        {
            Controller.Data.WarnedAboutRestart = true;
            base.PostClose();
            LanguageDatabase.Clear();
            LoadedModManager.ClearDestroy();
            foreach( Type genericParam in GenTypes.AllSubclasses( typeof( Def ) ) )
            {
                GenGeneric.InvokeStaticMethodOnGenericType( typeof( DefDatabase<> ), genericParam, "Clear" );
            }
            ThingCategoryNodeDatabase.Clear();
            BackstoryDatabase.Clear();
            SolidBioDatabase.Clear();
            PlayDataLoader.loaded = false;
            Controller.Data.RestartWarningIsOpen = false;
            if( Detour._PlayDataLoader.CallLoadAllPlayerDataWhenFinished )
            {
                Detour._PlayDataLoader._LoadAllPlayData( Detour._PlayDataLoader.queueRecovering );
            }
        }

		#endregion

	}

}
