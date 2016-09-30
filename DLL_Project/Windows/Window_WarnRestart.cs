using System;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class Window_WarnRestart : Window
	{

		#region Instance Data

        public static string            messageKey = string.Empty;
        public static Action            callbackBeforeRestart = null;

#if RELEASE
        private DateTime PlayWithoutRestartStart;
#endif

		#endregion

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2( 600f, 500f );
			}
		}

		#region Constructor

		public Window_WarnRestart()
		{
			layer = WindowLayer.GameUI;
            soundClose = SoundDefOf.MessageAlertNegative;
			doCloseButton = false;
			doCloseX = false;
			closeOnEscapeKey = false;
			forcePause = true;
            absorbInputAroundWindow = true;
		}

		#endregion

        #region PreOpen

        public override void PreOpen()
        {
            Controller.Data.RestartWarningIsOpen = true;
            Controller.Data.PlayWithoutRestart = false;
            base.PreOpen();
        }

        #endregion

        #region PreClose, Restart

        public override void PreClose()
        {
            if( Controller.Data.PlayWithoutRestart )
            {
                Controller.PlayDataLoader.Reload();
            }
            Controller.Data.RestartWarningIsOpen = false;
            Controller.Data.WarnedAboutRestart = true;
            base.PreClose();
        }

        #endregion

		#region Window Rendering

#if RELEASE
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
#endif

		public override void DoWindowContents( Rect inRect )
        {
			Text.Font = GameFont.Small;

            var localRect = inRect.ContractedBy( 10f );
            GUI.BeginGroup( localRect );

            var warnLabel = messageKey.Translate();
            var warnRect = new Rect( 0, 0, localRect.width, Text.CalcHeight( warnLabel, localRect.width ) );
            Widgets.Label( warnRect, warnLabel );

            bool closeWindow = false;
            var continueVector = new Vector2( 0, localRect.height - 45f - 16f - 24f );
            var continueRect = new Rect( 28f, continueVector.y, localRect.width - 28f, 24f );
#if RELEASE
            var oldValue = Controller.Data.PlayWithoutRestart;
#endif
            Widgets.Checkbox( continueVector, ref Controller.Data.PlayWithoutRestart );
            var continueLabel = "ContinueWithoutRestart".Translate();
            Widgets.Label( continueRect, continueLabel );
#if RELEASE
            if(
                ( Controller.Data.PlayWithoutRestart )&&
                ( Controller.Data.PlayWithoutRestart != oldValue )
            )
            {
                PlayWithoutRestartStart = System.DateTime.Now;
            }
#endif

            if( !Controller.Data.PlayWithoutRestart )
            {
                var restartLaterButtonRect = new Rect( 0, localRect.height - 45f, 100f, 45f );
                GUI.color = Color.red;
                closeWindow = Widgets.ButtonText( restartLaterButtonRect, "RestartLater".Translate() );
                var restartNowButtonRect = new Rect( localRect.width - 100f, localRect.height - 45f, 100f, 45f );
                GUI.color = Color.green;
                if( Widgets.ButtonText( restartNowButtonRect, "RestartNow".Translate() ) )
                {
                    RestartRimWorld();
                }
            }
            else
            {
#if RELEASE
                var CurrentTime = System.DateTime.Now;
                var OpenFor = ( CurrentTime - PlayWithoutRestartStart ).TotalSeconds;
                if( OpenFor < 30.0 )
                {
                    DrawCountDownLabel( localRect, 30.0 - OpenFor );
                }
                else
                {
#endif
                    var consequencesLabel = "AcceptTheConsequences".Translate();
                    var consequencesSize = Text.CalcSize( consequencesLabel );
                    consequencesSize.x += 24f;
                    var okButtonRect = new Rect( localRect.width - consequencesSize.x, localRect.height - 45f, consequencesSize.x, 45f );
                    GUI.color = Color.red;
                    closeWindow = Widgets.ButtonText( okButtonRect, consequencesLabel );
                    GUI.color = Color.white;
#if RELEASE
                }
#endif
            }

            GUI.EndGroup();

            if( closeWindow )
            {
                this.Close();
            }
		}

		#endregion

        private void RestartRimWorld()
        {
            if( callbackBeforeRestart != null )
            {
                callbackBeforeRestart.Invoke();
            }
            Controller.RimWorld.Restart();
        }

	}

}
