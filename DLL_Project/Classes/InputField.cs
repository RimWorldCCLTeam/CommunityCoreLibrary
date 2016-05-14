using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.UI
{
    public abstract class LabeledInput
    {
        #region Fields

        protected string _current;
        protected string _fieldName;
        protected string _lastGoodValue;
        protected TextAnchor anchor;
        protected Color color;
        protected GameFont font;
        protected string label;
        protected string newValue;
        protected string tip;
        protected string validationError;
        protected Func<string, bool> validator;
        private static int _fieldCount = 0;

        #endregion Fields

        #region Constructors

        // for scribe
        public LabeledInput()
        {
            // set input name so we can validate on lost focus and <enter>
            _fieldName = "field_" + _fieldCount++.ToString();
        }

        public LabeledInput( string current, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : this()
        {
            // string representation of current value
            _current = _lastGoodValue = newValue = current;

            // always validate
            validator = ( x ) => true;
            validationError = "CCL.UI.InputField.DefaultValidationError";

            // aesthetics
            this.label = label;
            this.tip = tip;
            this.color = color;
            this.font = font;
            this.anchor = anchor;
        }

        #endregion Constructors

        #region Properties

        public string Value
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
                _current = _lastGoodValue = value.ToString();
            }
        }

        #endregion Properties

        #region Methods

        public virtual void Apply()
        {
            newValue = _current;
        }

        public virtual void Draw( Rect canvas, float fieldWidth )
        {
            // create rects
            Rect labelRect = canvas, fieldRect = canvas;
            labelRect.xMax -= fieldWidth;
            fieldRect.xMin = canvas.width - fieldWidth;

            // draw tooltip
            if ( !tip.NullOrEmpty() )
                TooltipHandler.TipRegion( canvas, tip );

            // draw stuff
            GUI.SetNextControlName( _fieldName );
            CCL_Widgets.Label( labelRect, label, color, font, anchor, tip );
            _current = GUI.TextField( fieldRect, _current );

            // if field doesn't have focus, or enter is pressed, validate the input
            if ( GUI.GetNameOfFocusedControl() != _fieldName || Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter )
                Validate();
        }

        public virtual void Draw( Rect canvas )
        {
            Draw( canvas, canvas.width * 1/4f );
        }

        public virtual void Validate()
        {
            if ( validator( _current ) )
            {
                _lastGoodValue = _current;
                Apply();
            }
            else
            {
                Messages.Message( validationError.Translate( _current ), MessageSound.RejectInput );
                _current = _lastGoodValue;
            }
        }

        #endregion Methods
    }

    public class LabeledInput_Color : LabeledInput
    {
        #region Fields

        private ColorPicker.ColorWrapper _wrapper;
        private new Color newValue;

        #endregion Fields

        #region Constructors

        public LabeledInput_Color( Color value, string label, string tip = "" ) : this( value, label, tip, Color.white, GameFont.Small, TextAnchor.UpperLeft )
        {
        }

        public LabeledInput_Color( Color value, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value.ToString(), label, tip, color, font, anchor )
        {
            newValue = value;
            _wrapper = new ColorPicker.ColorWrapper( value );
        }

        #endregion Constructors

        #region Properties

        public new Color Value
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
            }
        }

        #endregion Properties

        #region Methods

        public override void Draw( Rect canvas, float fieldWidth )
        {
            // get maximum allowable size for preview rect.
            float previewSize = Mathf.Min( canvas.height - 6f, fieldWidth );

            // create rects
            Rect previewRect = new Rect( canvas.xMax - previewSize - 3f, canvas.yMin + 3f, previewSize, previewSize );
            Rect labelRect = canvas;
            labelRect.xMax -= previewSize + 6f;

            // draw tooltip
            if ( !tip.NullOrEmpty() )
                TooltipHandler.TipRegion( canvas, tip );

            // draw things
            CCL_Widgets.Label( labelRect, label, color, font, anchor, tip );
            CCL_Widgets.DrawBackground( previewRect, newValue );

            // open Colorpicker when preview is clicked
            if ( Widgets.InvisibleButton( previewRect ) )
                Find.WindowStack.Add( new ColorPicker.Dialog_ColorPicker( _wrapper, () => newValue = _wrapper.Color, false, true ) { layer = WindowLayer.Super, closeOnClickedOutside = true } );
        }

        #endregion Methods
    }

    public class LabeledInput_Float : LabeledInput
    {
        #region Fields

        private new float newValue;

        #endregion Fields

        #region Constructors

        public LabeledInput_Float( float value, string label, string tip = "" ) : this( value, label, tip, Color.white, GameFont.Small, TextAnchor.UpperLeft )
        {
        }

        public LabeledInput_Float( float value, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value.ToString(), label, tip, color, font, anchor )
        {
            newValue = value;
            validator = _validator;
            validationError = "CCL.UI.InputField_Float.ValidationError";
        }

        #endregion Constructors

        #region Properties

        public new float Value
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
                _current = _lastGoodValue = value.ToString();
            }
        }

        #endregion Properties

        #region Methods

        public override void Apply()
        {
            newValue = float.Parse( _current );
        }

        private static bool _validator( string current )
        {
            float dump;
            return float.TryParse( current, out dump );
        }

        #endregion Methods
    }

    public class LabeledInput_Int : LabeledInput
    {
        #region Fields

        private new int newValue;

        #endregion Fields

        #region Constructors

        public LabeledInput_Int( int value, string label, string tip = "" ) : this( value, label, tip, Color.white, GameFont.Small, TextAnchor.UpperLeft )
        {
        }

        public LabeledInput_Int( int value, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value.ToString(), label, tip, color, font, anchor )
        {
            newValue = value;
            validator = _validator;
            validationError = "CCL.UI.InputField_Int.ValidationError";
        }

        #endregion Constructors

        #region Properties

        public new int Value
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
                _current = _lastGoodValue = value.ToString();
            }
        }

        #endregion Properties

        #region Methods

        public override void Apply()
        {
            newValue = int.Parse( _current );
        }

        private static bool _validator( string current )
        {
            int dump;
            return int.TryParse( current, out dump );
        }

        #endregion Methods
    }
}