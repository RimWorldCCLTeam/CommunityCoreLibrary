using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using RimWorld;
using Verse;
using UnityEngine;
using CommunityCoreLibrary;

namespace CommunityCoreLibrary.UI
{
    public abstract class LabeledInput
    {
        private static int _fieldCount = 0;
        protected string _fieldName;

        protected string _current;
        protected string _lastGoodValue;
        protected string newValue;

        protected Func<string, bool> validator;
        protected string validationError;

        protected string label;
        protected string tip;
        protected Color color;
        protected GameFont font;
        protected TextAnchor anchor;

        public LabeledInput( string current, string label, string tip, Color color, GameFont font, TextAnchor anchor )
        {
            // set input name so we can validate on lost focus and <enter>
            _fieldName = "field_" + _fieldCount++.ToString();

            // string representation of current value
            _current = _lastGoodValue = newValue = current;

            // always validate
            validator = ( x ) => true;
            validationError = "CCL.UI.InputField.DefaultValidationError".Translate();

            // aesthetics
            this.label = label;
            this.tip = tip;
            this.color = color;
            this.font = font;
            this.anchor = anchor;
        }

        public virtual void Draw( Rect canvas, float fieldWidth )
        {
            // create rects
            Rect labelRect = canvas, fieldRect = canvas;
            labelRect.xMax -= fieldWidth;
            fieldRect.xMin += fieldWidth;

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
                _current = _lastGoodValue;
                Messages.Message( validationError, MessageSound.RejectInput );
            }
        }

        public virtual void Apply()
        {
            newValue = _current;
        }

        public string Value => newValue;
    }

    public class LabeledInput_Int : LabeledInput
    {
        private new int newValue;

        private static bool _validator( string current )
        {
            int dump;
            return int.TryParse( current, out dump );
        }

        public override void Apply()
        {
            newValue = int.Parse( _current );
        }

        public LabeledInput_Int( int value, string label ) : this( value, label, "", Color.white, GameFont.Small, TextAnchor.UpperLeft ) { }

        public LabeledInput_Int( int value, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value.ToString(), label, tip, color, font, anchor )
        {
            newValue = value;
            validator = _validator;
            validationError = "CCL.UI.InputField_Int.ValidationError".Translate();
        }

        public new int Value => newValue;
    }

    public class LabeledInput_Float : LabeledInput
    {
        private new float newValue;

        private static bool _validator( string current )
        {
            float dump;
            return float.TryParse( current, out dump );
        }

        public override void Apply()
        {
            newValue = float.Parse( _current );
        }

        public LabeledInput_Float( float value, string label ) : this( value, label, "", Color.white, GameFont.Small, TextAnchor.UpperLeft ) { }

        public LabeledInput_Float( float value, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value.ToString(), label, tip, color, font, anchor )
        {
            newValue = value;
            validator = _validator;
            validationError = "CCL.UI.InputField_Float.ValidationError".Translate();
        }

        public new float Value => newValue;
    }

    public class LabeledInput_Color : LabeledInput
    {
        private new Color newValue;

        private ColorPicker.ColorWrapper _wrapper;
        
        public LabeledInput_Color( Color value, string label ) : this( value, label, "", Color.white, GameFont.Small, TextAnchor.UpperLeft ) { }

        public LabeledInput_Color( Color value, string label, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value.ToString(), label, tip, color, font, anchor )
        {
            newValue = value;
            _wrapper = new ColorPicker.ColorWrapper( value );
        }

        public override void Draw( Rect canvas, float fieldWidth )
        {
            // get maximum allowable size for preview rect.
            float previewSize = Mathf.Min( canvas.height - 6f, fieldWidth );

            // create rects
            Rect previewRect = new Rect( canvas.xMax - previewSize - 3f, 3f, previewSize, previewSize );
            Rect labelRect = canvas;
            labelRect.xMax -= previewSize + 6f;

            // draw things
            CCL_Widgets.Label( labelRect, label, color, font, anchor, tip );
            CCL_Widgets.DrawBackground( canvas, newValue );

            // open Colorpicker when preview is clicked
            if ( Widgets.InvisibleButton( previewRect ) )
                Find.WindowStack.Add( new ColorPicker.Dialog_ColorPicker( _wrapper, () => newValue = _wrapper.Color, false, true ) );
        }

        public new Color Value => newValue;
    }
}
