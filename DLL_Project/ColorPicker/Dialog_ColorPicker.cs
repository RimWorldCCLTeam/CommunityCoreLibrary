using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.ColorPicker
{
    public class Dialog_ColorPicker : Window
    {
        #region Fields

        public static ColorPresets presets;

        // the color we're going to pass out if requested
        public Color        Color                  = Color.blue;

        public Vector2      initialPosition         = Vector2.zero,
                            windowSize              = Vector2.zero;

        public int          numPresets              = 0,
                            pickerSize              = 300,
                            sliderWidth             = 15,
                            alphaBGBlockSize        = 10,
                            previewSize             = 90, // odd multiple of alphaBGblocksize forces alternation of the background texture grid.
                            handleSize              = 10;

        // used in the picker only
        public Color        tempColor              = Color.white;

        private controls    _activeControl          = controls.none;

        private Color       _alphaBGColorA          = Color.white,
                            _alphaBGColorB          = new Color(.85f, .85f, .85f);

        private Action      _callback;

        private Texture2D   _colorPickerBG,
                            _huePickerBG,
                            _alphaPickerBG,
                            _tempPreviewBG,
                            _previewBG,
                            _pickerAlphaBG,
                            _sliderAlphaBG,
                            _previewAlphaBG;

        private string      _hexOut,
                            _hexIn;

        private float       _margin                 = 6f,
                            _fieldHeight            = 30f,
                            _huePosition,
                            _alphaPosition,
                            _unitsPerPixel,
                            _H                      = 0f,
                            _S                      = 1f,
                            _V                      = 1f,
                            _A                      = 1f;

        private Vector2     _pickerPosition         = Vector2.zero;

        private bool        _preview            = true,
                            _autoApply              = false;

        // reference type containing the in/out parameter
        private ColorWrapper _wrapper;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Call with a ColorWrapper object containing the color to be changed, with an optional callback which is called when Apply or OK are clicked.
        /// Setting draggable = true will break sliders for now.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="callback"></param>
        public Dialog_ColorPicker( ColorWrapper color, Action callback = null, bool preview = true, bool autoApply = false )
        {
            // TODO: figure out if sliders and draggable = true can coexist.
            // using Event.current.Use() prevents further drawing of the tab and closes parent(s).
            _wrapper     = color;
            _callback    = callback;
            _preview     = preview;
            _autoApply   = autoApply;
            Color        = _wrapper.Color;
            tempColor    = _wrapper.Color;

            Notify_RGBUpdated();
        }

        #endregion Constructors

        #region Enums

        private enum controls
        {
            colorPicker,
            huePicker,
            alphaPicker,
            none
        }

        #endregion Enums

        #region Properties

        public float A
        {
            get
            {
                return _A;
            }
            set
            {
                _A = Mathf.Clamp( value, 0f, 1f );
                Notify_HSVUpdated();
                CreateColorPickerBG();
            }
        }

        public Texture2D AlphaPickerBG
        {
            get
            {
                if ( _alphaPickerBG == null )
                {
                    CreateAlphaPickerBG();
                }
                return _alphaPickerBG;
            }
        }

        public Texture2D ColorPickerBG
        {
            get
            {
                if ( _colorPickerBG == null )
                {
                    CreateColorPickerBG();
                }
                return _colorPickerBG;
            }
        }

        public float H
        {
            get
            {
                return _H;
            }
            set
            {
                _H = Mathf.Clamp( value, 0f, 1f );
                Notify_HSVUpdated();
                CreateColorPickerBG();
                CreateAlphaPickerBG();
            }
        }

        public Texture2D HuePickerBG
        {
            get
            {
                if ( _huePickerBG == null )
                {
                    CreateHuePickerBG();
                }
                return _huePickerBG;
            }
        }

        public Texture2D PickerAlphaBG
        {
            get
            {
                if ( _pickerAlphaBG == null )
                {
                    _pickerAlphaBG = CreateAlphaBG( pickerSize, pickerSize );
                }
                return _pickerAlphaBG;
            }
        }

        public Texture2D PreviewAlphaBG
        {
            get
            {
                if ( _previewAlphaBG == null )
                {
                    _previewAlphaBG = CreateAlphaBG( previewSize, previewSize );
                }
                return _previewAlphaBG;
            }
        }

        public Texture2D PreviewBG
        {
            get
            {
                if ( _previewBG == null )
                {
                    _previewBG = CreatePreviewBG( Color );
                }
                return _previewBG;
            }
        }

        public float S
        {
            get
            {
                return _S;
            }
            set
            {
                _S = Mathf.Clamp( value, 0f, 1f );
                Notify_HSVUpdated();
                CreateAlphaPickerBG();
            }
        }

        public Texture2D SliderAlphaBG
        {
            get
            {
                if ( _sliderAlphaBG == null )
                {
                    _sliderAlphaBG = CreateAlphaBG( sliderWidth, pickerSize );
                }
                return _sliderAlphaBG;
            }
        }

        public Texture2D TempPreviewBG
        {
            get
            {
                if ( _tempPreviewBG == null )
                {
                    _tempPreviewBG = CreatePreviewBG( tempColor );
                }
                return _tempPreviewBG;
            }
        }

        public float UnitsPerPixel
        {
            get
            {
                if ( _unitsPerPixel == 0.0f )
                {
                    _unitsPerPixel = 1f / pickerSize;
                }
                return _unitsPerPixel;
            }
        }

        public float V
        {
            get
            {
                return _V;
            }
            set
            {
                _V = Mathf.Clamp( value, 0f, 1f );
                Notify_HSVUpdated();
                CreateAlphaPickerBG();
            }
        }

        public Vector2 WindowSize
        {
            get
            {
                return windowSize;
            }
            set
            {
                windowSize = value;
                SetWindowSize( windowSize );
            }
        }

        #endregion Properties

        #region Methods

        public void AlphaAction( float pos )
        {
            // only changing one value, property should work fine
            A = 1 - UnitsPerPixel * pos;
            _alphaPosition = pos;
        }

        public void Apply()
        {
            _wrapper.Color = tempColor;
            if ( _callback != null )
            {
                _callback();
            }
        }

        public Texture2D CreatePreviewBG( Color col )
        {
            return SolidColorMaterials.NewSolidColorTexture( col );
        }

        public override void DoWindowContents( Rect inRect )
        {
            // set up rects
            // pickers & sliders
            Rect pickerRect = new Rect( inRect.xMin, inRect.yMin, pickerSize, pickerSize );
            Rect hueRect = new Rect( pickerRect.xMax + _margin, inRect.yMin, sliderWidth, pickerSize );
            Rect alphaRect = new Rect( hueRect.xMax + _margin, inRect.yMin, sliderWidth, pickerSize );

            // previews
            Rect previewRect = new Rect( alphaRect.xMax + _margin, inRect.yMin, previewSize, previewSize );
            Rect previewOldRect = new Rect( previewRect.xMax, inRect.yMin, previewSize, previewSize );

            // buttons and textfields
            Rect okRect = new Rect( alphaRect.xMax + _margin, inRect.yMax - _fieldHeight, previewSize * 2, _fieldHeight );
            Rect applyRect = new Rect( alphaRect.xMax + _margin, inRect.yMax - 2 * _fieldHeight - _margin, previewSize - _margin / 2, _fieldHeight );
            Rect cancelRect = new Rect( applyRect.xMax + _margin, applyRect.yMin, previewSize - _margin / 2, _fieldHeight );
            Rect hexRect = new Rect( alphaRect.xMax + _margin, inRect.yMax - 3 * _fieldHeight - 2 * _margin, previewSize * 2, _fieldHeight );

            // move ok/cancel buttons for the simple view with buttons
            if ( !_preview && !_autoApply )
            {
                cancelRect = new Rect( inRect.xMin, pickerRect.yMax + _margin, ( pickerSize - _margin ) / 2, _fieldHeight );
                okRect = cancelRect;
                okRect.x += ( pickerSize + _margin ) / 2;
            }

            // draw transparency backgrounds
            GUI.DrawTexture( pickerRect, PickerAlphaBG );
            GUI.DrawTexture( alphaRect, SliderAlphaBG );
            if ( _preview )
            {
                GUI.DrawTexture( previewRect, PreviewAlphaBG );
                GUI.DrawTexture( previewOldRect, PreviewAlphaBG );
            }

            // draw picker foregrounds
            GUI.DrawTexture( pickerRect, ColorPickerBG );
            GUI.DrawTexture( hueRect, HuePickerBG );
            GUI.DrawTexture( alphaRect, AlphaPickerBG );
            if ( _preview )
            {
                GUI.DrawTexture( previewRect, TempPreviewBG );
                GUI.DrawTexture( previewOldRect, PreviewBG );
            }

            // draw slider handles
            Rect hueHandleRect = new Rect( hueRect.xMin - 3f, hueRect.yMin + _huePosition - handleSize / 2, sliderWidth + 6f, handleSize );
            Rect pickerHandleRect = new Rect( pickerRect.xMin + _pickerPosition.x - handleSize / 2, pickerRect.yMin + _pickerPosition.y - handleSize / 2, handleSize, handleSize );
            Rect alphaHandleRect = new Rect( alphaRect.xMin - 3f, alphaRect.yMin + _alphaPosition - handleSize / 2, sliderWidth + 6f, handleSize );
            GUI.DrawTexture( hueHandleRect, TempPreviewBG );
            GUI.DrawTexture( pickerHandleRect, TempPreviewBG );
            GUI.DrawTexture( alphaHandleRect, TempPreviewBG );

            // border on slider handles
            GUI.color = Color.gray;
            Widgets.DrawBox( hueHandleRect );
            Widgets.DrawBox( pickerHandleRect );
            Widgets.DrawBox( alphaHandleRect );
            GUI.color = Color.white;

            #region UI interactions

            // reset active control on mouseup
            if ( Input.GetMouseButtonUp( 0 ) )
            {
                _activeControl = controls.none;
            }

            // colorpicker interaction
            if ( Mouse.IsOver( pickerRect ) )
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    _activeControl = controls.colorPicker;
                }
                if ( _activeControl == controls.colorPicker )
                {
                    Vector2 MousePosition = Event.current.mousePosition;
                    Vector2 PositionInRect = MousePosition - new Vector2( pickerRect.xMin, pickerRect.yMin );

                    PickerAction( PositionInRect );
                }
            }

            // hue picker interaction
            if ( Mouse.IsOver( hueRect ) )
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    _activeControl = controls.huePicker;
                }
                if ( Event.current.type == EventType.ScrollWheel )
                {
                    H -= Event.current.delta.y * UnitsPerPixel;
                    _huePosition = Mathf.Clamp( _huePosition + Event.current.delta.y, 0f, pickerSize );
                    Event.current.Use();
                }
                if ( _activeControl == controls.huePicker )
                {
                    float MousePosition = Event.current.mousePosition.y;
                    float PositionInRect = MousePosition - hueRect.yMin;

                    HueAction( PositionInRect );
                }
            }

            // alpha picker interaction
            if ( Mouse.IsOver( alphaRect ) )
            {
                if ( Input.GetMouseButtonDown( 0 ) )
                {
                    _activeControl = controls.alphaPicker;
                }
                if ( Event.current.type == EventType.ScrollWheel )
                {
                    A -= Event.current.delta.y * UnitsPerPixel;
                    _alphaPosition = Mathf.Clamp( _alphaPosition + Event.current.delta.y, 0f, pickerSize );
                    Event.current.Use();
                }
                if ( _activeControl == controls.alphaPicker )
                {
                    float MousePosition = Event.current.mousePosition.y;
                    float PositionInRect = MousePosition - alphaRect.yMin;

                    AlphaAction( PositionInRect );
                }
            }

            if ( !_autoApply )
            {
                // buttons and text field
                // for some reason scrolling sometimes changes text size
                Text.Font = GameFont.Small;
                if ( Widgets.ButtonText( okRect, "OK" ) )
                {
                    Apply();
                    this.Close();
                }
                if ( Widgets.ButtonText( applyRect, "Apply" ) )
                {
                    Apply();
                    SetColor();
                }
                if ( Widgets.ButtonText( cancelRect, "Cancel" ) )
                {
                    this.Close();
                }
            }

            if ( _preview )
            {
                if ( _hexIn != _hexOut )
                {
                    if ( ColorHelper.TryHexToRGB( _hexIn, ref tempColor ) )
                    {
                        Notify_RGBUpdated();
                    }
                    else
                    {
                        GUI.color = Color.red;
                    }
                }
                _hexIn = Widgets.TextField( hexRect, _hexIn );
            }
            GUI.color = Color.white;

            #endregion UI interactions
        }

        public void HueAction( float pos )
        {
            // only changing one value, property should work fine
            H = 1 - UnitsPerPixel * pos;
            _huePosition = pos;
        }

        public void Notify_HSVUpdated()
        {
            tempColor = ColorHelper.HSVtoRGB( H, S, V );
            tempColor.a = A;
            _tempPreviewBG = CreatePreviewBG( tempColor );

            if ( _preview )
                _hexOut = _hexIn = ColorHelper.RGBtoHex( tempColor );

            if ( _autoApply )
                Apply();
        }

        public void Notify_RGBUpdated()
        {
            // Set HSV from RGB
            ColorHelper.RGBtoHSV( tempColor, out _H, out _S, out _V );
            _A = tempColor.a;

            // rebuild textures
            CreateColorPickerBG();
            CreateHuePickerBG();
            if ( _preview )
                CreateAlphaPickerBG();

            // set slider positions
            _huePosition = ( 1f - _H ) / UnitsPerPixel;
            _pickerPosition.x = _S / UnitsPerPixel;
            _pickerPosition.y = ( 1f - _V ) / UnitsPerPixel;
            if ( _preview )
                _alphaPosition = ( 1f - _A ) / UnitsPerPixel;

            // set the color block and update hex fields
            _tempPreviewBG = CreatePreviewBG( tempColor );
            if ( _preview )
                _hexOut = _hexIn = ColorHelper.RGBtoHex( tempColor );

            // call callback for auto-apply
            if ( _autoApply )
                Apply();
        }

        public void PickerAction( Vector2 pos )
        {
            // if we set S, V via properties textures will be rebuilt twice.
            _S = UnitsPerPixel * pos.x;
            _V = 1 - UnitsPerPixel * pos.y;

            // rebuild textures
            CreateAlphaPickerBG();
            Notify_HSVUpdated();
            _pickerPosition = pos;
        }

        public override void PostOpen()
        {
            // allow explicit setting of window position
            if ( initialPosition != Vector2.zero )
            {
                windowRect.x = initialPosition.x;
                windowRect.y = initialPosition.y;
            }

            // set the windowsize
            if ( windowSize == Vector2.zero ) // not specifically set in construction, calculate size from elements.
            {
                // default window size.
                float width, height;
                // size of main picker + the standard window margins.
                width = height = pickerSize + Window.StandardMargin * 2;

                // width of two sliders (hue and alpha) + margins
                width += ( sliderWidth + _margin ) * 2;

                if ( _preview )
                {
                    // add 2 preview rects
                    width += _margin * 2 + previewSize * 2;
                }
                else
                {
                    if ( !_autoApply )
                    {
                        // if this is not auto applied, we need a place to but the buttons.
                        height += _fieldHeight + _margin;
                    }
                }

                // that should do it
                SetWindowSize( new Vector2( width, height ) );
            }
            else
            {
                // allow explicit specification of window size
                // NOTE: elements do not actually adapt to this size.
                SetWindowSize( windowSize );
            }

            // init sliders
            Notify_RGBUpdated();
            _alphaPosition = Color.a / UnitsPerPixel;
        }

        public void SetColor()
        {
            Color = tempColor;
            _previewBG = CreatePreviewBG( tempColor );
        }

        public void SetWindowLocation( Vector2 location )
        {
            windowRect.xMin = location.x;
            windowRect.yMin = location.y;
        }

        public void SetWindowRcet( Rect rect )
        {
            windowRect = rect;
        }

        public void SetWindowSize( Vector2 size )
        {
            windowRect.width = size.x;
            windowRect.height = size.y;
        }

        private Texture2D CreateAlphaBG( int width, int height )
        {
            Texture2D tex = new Texture2D( width, height );

            // initialize color arrays for blocks
            Color[] bgA = new Color[alphaBGBlockSize * alphaBGBlockSize];
            for ( int i = 0; i < bgA.Length; i++ )
                bgA[i] = _alphaBGColorA;
            Color[] bgB = new Color[alphaBGBlockSize * alphaBGBlockSize];
            for ( int i = 0; i < bgB.Length; i++ )
                bgB[i] = _alphaBGColorB;

            // set blocks of pixels at a time
            // this also sets border blocks, meaning it'll try to set out of bounds pixels.
            int row = 0;
            for ( int x = 0; x < width; x = x + alphaBGBlockSize )
            {
                int column = row;
                for ( int y = 0; y < height; y = y + alphaBGBlockSize )
                {
                    tex.SetPixels( x, y, alphaBGBlockSize, alphaBGBlockSize, ( column % 2 == 0 ? bgA : bgB ) );
                    column++;
                }
                row++;
            }

            tex.Apply();
            return tex;
        }

        private void CreateAlphaPickerBG()
        {
            if ( _alphaPickerBG == null )
                _alphaPickerBG = new Texture2D( 1, pickerSize );

            var h = pickerSize;
            var hu = 1f / h;

            // RGB color from cache, increasing a
            for ( int y = 0; y < h; y++ )
            {
                _alphaPickerBG.SetPixel( 0, y, new Color( tempColor.r, tempColor.g, tempColor.b, y * hu ) );
            }
            _alphaPickerBG.Apply();
        }

        private void CreateColorPickerBG()
        {
            if ( _colorPickerBG == null )
                _colorPickerBG = new Texture2D( pickerSize, pickerSize );

            float S, V;
            int w = pickerSize;
            int h = pickerSize;
            float wu = UnitsPerPixel;
            float hu = UnitsPerPixel;

            // HSV colors, H in slider, S horizontal, V vertical.
            for ( int x = 0; x < w; x++ )
            {
                for ( int y = 0; y < h; y++ )
                {
                    S = x * wu;
                    V = y * hu;
                    _colorPickerBG.SetPixel( x, y, ColorHelper.HSVtoRGB( H, S, V, A ) );
                }
            }
            _colorPickerBG.Apply();
        }

        private void CreateHuePickerBG()
        {
            if ( _huePickerBG == null )
                _huePickerBG = new Texture2D( 1, pickerSize );

            var h = pickerSize;
            var hu = UnitsPerPixel;

            // HSV colors, S = V = 1
            for ( int y = 0; y < h; y++ )
            {
                _huePickerBG.SetPixel( 0, y, ColorHelper.HSVtoRGB( hu * y, 1f, 1f ) );
            }
            _huePickerBG.Apply();
        }

        #endregion Methods

        #region Classes

        public class ColorPresets
        {
            #region Fields

            // share 'presets' across instances.
            private static List<Color>             _presets         = new List<Color>();

            private float                          _minimumBoxSize  = 10f;
            private Dialog_ColorPicker            _parent;
            private int                            _size            = 10;

            #endregion Fields

            #region Constructors

            public ColorPresets( Dialog_ColorPicker parent, int size = 10 )
            {
                _parent = parent;
                _size   = size;
            }

            #endregion Constructors

            #region Properties

            // only allow read.
            public List<Color> presets
            {
                get
                {
                    return _presets;
                }
            }

            #endregion Properties

            #region Methods

            public void Add( Color col )
            {
                // First in, first out.
                // add latest element to the back of the list
                _presets.Add( col );

                // pop elements from the front until the list is short enough (should really only be once).
                while ( _presets.Count > _size )
                    _presets.RemoveAt( 0 );
            }

            // draw presets and interactivity.
            public void DrawPresetBoxes()
            {
                // TODO: this.
            }

            #endregion Methods
        }

        #endregion Classes
    }
}