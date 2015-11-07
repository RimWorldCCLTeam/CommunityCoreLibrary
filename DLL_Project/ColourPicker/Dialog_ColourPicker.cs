using System;
using Verse;
using UnityEngine;
using System.Globalization;

namespace CommunityCoreLibrary.ColourPicker
{
    public class Dialog_ColourPicker : Window
    {
        enum controls
        {
            colourPicker,
            huePicker,
            alphaPicker,
            none
        }
        private controls    _activeControl          = controls.none;
        private Texture2D   _colourPickerBG,
                            _huePickerBG,
                            _alphaPickerBG,
                            _tempPreviewBG,
                            _previewBG,
                            _pickerAlphaBG,
                            _sliderAlphaBG,
                            _previewAlphaBG;
        private Color       _alphaBGColorA          = Color.white,
                            _alphaBGColorB          = new Color(.85f, .85f, .85f);
        private int         _pickerSize             = 300,
                            _sliderWidth            = 15,
                            _alphaBGBlockSize       = 10,
                            _previewSize            = 90, // odd multiple of alphaBGblocksize forces alternation of the background texture grid.
                            _handleSize             = 10;
        private float       _margin                 = 6f,
                            _fieldHeight            = 30f,
                            _huePosition,
                            _alphaPosition,
                            _unitsPerPixel,
                            _H                      = 0f,
                            _S                      = 1f,
                            _V                      = 1f,
                            _A                      = 1f;
        private Vector2     _position               = Vector2.zero;
        private string      _hexOut,
                            _hexIn;
        private Action      _callback;

        // reference type containing the in/out parameter
        private ColourWrapper _wrapper;
        
        // the colour we're going to pass out if requested
        public Color        Colour                  = Color.blue;

        // used in the picker only
        public Color        tempColour              = Color.white;
        
        /// <summary>
        /// Call with a ColourWrapper object containing the colour to be changed, with an optional callback which is called when Apply or OK are clicked.
        /// Setting draggable = true will break sliders for now.
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="callback"></param>
        public Dialog_ColourPicker ( ColourWrapper colour, Action callback = null )
        {
            // TODO: figure out if sliders and draggable = true can coexist.
            // using Event.current.Use() prevents further drawing of the tab and closes parent(s).
            _wrapper = colour;
            _callback = callback;
            Colour = _wrapper.Color;

            NotifyRGBUpdated();
        }

        public float UnitsPerPixel
        {
            get
            {
                if( _unitsPerPixel == 0.0f){
                    _unitsPerPixel = 1f / _pickerSize;
                }
                return _unitsPerPixel;
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
                _H = Mathf.Clamp(value, 0f, 1f);
                NotifyHSVUpdated();
                CreateColourPickerBG();
                CreateAlphaPickerBG();
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
                NotifyHSVUpdated();
                CreateAlphaPickerBG();
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
                NotifyHSVUpdated();
                CreateAlphaPickerBG();
            }
        }

        public float A
        {
            get
            {
                return _A;
            }
            set
            {
                _A = Mathf.Clamp( value, 0f, 1f );
                NotifyHSVUpdated();
                CreateColourPickerBG();
            }
        }

        public void NotifyHSVUpdated()
        {
            tempColour = HSV.ToRGBA( H, S, V );
            tempColour.a = A;
            _tempPreviewBG = CreatePreviewBG( tempColour );
            _hexOut = _hexIn = RGBtoHex( tempColour );
        }

        public void NotifyRGBUpdated()
        {
            // Set HSV from RGB
            HSV.ToHSV( tempColour, out _H, out _S, out _V );
            _A = tempColour.a;

            // rebuild textures
            CreateColourPickerBG();
            CreateHuePickerBG();
            CreateAlphaPickerBG();

            // set slider positions
            _huePosition = ( 1f - _H ) / UnitsPerPixel;
            _position.x = _S / UnitsPerPixel;
            _position.y = ( 1f - _V ) / UnitsPerPixel;
            _alphaPosition = ( 1f - _A ) / UnitsPerPixel;

            // set the colour block and update hex fields
            _tempPreviewBG = CreatePreviewBG( tempColour );
            _hexOut = _hexIn = RGBtoHex( tempColour );
        }

        public void SetColor()
        {
            Colour = tempColour;
            _previewBG = CreatePreviewBG( tempColour );
        }

        public Texture2D ColourPickerBG
        {
            get
            {
                if( _colourPickerBG == null )
                {
                    CreateColourPickerBG();
                }
                return _colourPickerBG;
            }
        }

        public Texture2D HuePickerBG
        {
            get
            {
                if( _huePickerBG == null )
                {
                    CreateHuePickerBG();
                }
                return _huePickerBG;
            }
        }

        public Texture2D AlphaPickerBG
        {
            get
            {
                if( _alphaPickerBG == null )
                {
                    CreateAlphaPickerBG();
                }
                return _alphaPickerBG;
            }
        }

        public Texture2D TempPreviewBG
        {
            get
            {
                if( _tempPreviewBG == null )
                {
                    _tempPreviewBG = CreatePreviewBG( tempColour );
                }
                return _tempPreviewBG;
            }
        }

        public Texture2D PreviewBG
        {
            get
            {
                if( _previewBG == null )
                {
                    _previewBG = CreatePreviewBG( Colour );
                }
                return _previewBG;
            }
        }

        public Texture2D PickerAlphaBG
        {
            get
            {
                if( _pickerAlphaBG == null )
                {
                    _pickerAlphaBG = CreateAlphaBG( _pickerSize, _pickerSize );
                }
                return _pickerAlphaBG;
            }
        }


        public Texture2D SliderAlphaBG
        {
            get
            {
                if( _sliderAlphaBG == null )
                {
                    _sliderAlphaBG = CreateAlphaBG( _sliderWidth, _pickerSize );
                }
                return _sliderAlphaBG;
            }
        }

        public Texture2D PreviewAlphaBG
        {
            get
            {
                if( _previewAlphaBG == null )
                {
                    _previewAlphaBG = CreateAlphaBG( _previewSize, _previewSize );
                }
                return _previewAlphaBG;
            }
        }

        private void CreateColourPickerBG()
        {
            float S, V;
            int w = _pickerSize;
            int h = _pickerSize;
            float wu = UnitsPerPixel;
            float hu = UnitsPerPixel;

            Texture2D tex = new Texture2D( w, h );

            // HSV colours, H in slider, S horizontal, V vertical.
            for( int x = 0; x < w; x++ )
            {
                for( int y = 0; y < h; y++ )
                {
                    S = x * wu;
                    V = y * hu;
                    tex.SetPixel( x, y, HSV.ToRGBA( H, S, V, A ) );
#if DEBUG
                    if (x % 50 == 0 && y % 50 == 0 )
                    {
                        Color col = tex.GetPixel(x, y);
                        Log.Message( "HSV > x: " + x + ", y: " + y + ", H: " + H + ", S: " + S + ", V:" + V );
                        Log.Message( "RGB > x: " + x + ", y: " + y + ", R: " + col.r + ", G: " + col.g + ", B:" + col.b );
                    }
#endif
                }
            }
            tex.Apply();

            _colourPickerBG = tex;
        }

        private void CreateHuePickerBG()
        {
            Texture2D tex = new Texture2D(1, _pickerSize);

            var h = _pickerSize;
            var hu = 1f / h;

            // HSV colours, S = V = 1
            for( int y = 0; y < h; y++ )
            {
                tex.SetPixel( 0, y, HSV.ToRGBA( hu * y, 1f, 1f ) );
            }
            tex.Apply();

            _huePickerBG = tex;
        }

        private void CreateAlphaPickerBG()
        {
            Texture2D tex = new Texture2D(1, _pickerSize);

            var h = _pickerSize;
            var hu = 1f / h;

            // RGB color from cache, alternate a
            for( int y = 0; y < h; y++ )
            {
                tex.SetPixel( 0, y, new Color( tempColour.r, tempColour.g, tempColour.b, y * hu ) );
            }
            tex.Apply();

            _alphaPickerBG = tex;
        }

        private Texture2D CreateAlphaBG( int width, int height )
        {
            Texture2D tex = new Texture2D(width, height);

            // initialize color arrays for blocks
            Color[] bgA = new Color[_alphaBGBlockSize * _alphaBGBlockSize];
            for( int i = 0; i < bgA.Length; i++ ) bgA[i] = _alphaBGColorA;
            Color[] bgB = new Color[_alphaBGBlockSize * _alphaBGBlockSize];
            for( int i = 0; i < bgB.Length; i++ ) bgB[i] = _alphaBGColorB;

            // set blocks of pixels at a time
            // this also sets border blocks, meaning it'll try to set out of bounds pixels. 
            int row = 0;
            for( int x = 0; x < width; x = x + _alphaBGBlockSize )
            {
                int column = row;
                for( int y = 0; y < height; y = y + _alphaBGBlockSize )
                {
                    tex.SetPixels( x, y, _alphaBGBlockSize, _alphaBGBlockSize, ( column % 2 == 0 ? bgA : bgB ) );
                    column++;
                }
                row++;
            }

            tex.Apply();
            return tex;
        }

        public Texture2D CreatePreviewBG( Color col )
        {
            return SolidColorMaterials.NewSolidColorTexture( col );
        }

        public void PickerAction( Vector2 pos )
        {
            // if we set S, V via properties these will be called twice. 
            _S = UnitsPerPixel * pos.x;
            _V = 1 - UnitsPerPixel * pos.y;

            CreateAlphaPickerBG();
            NotifyHSVUpdated();
            _position = pos;
        }

        public void HueAction( float pos )
        {
            // only changing one value, property should work fine
            H = 1 - UnitsPerPixel * pos;
            _huePosition = pos;
        }

        public void AlphaAction( float pos )
        {
            // only changing one value, property should work fine
            A = 1 - UnitsPerPixel * pos;
            _alphaPosition = pos;
        }

        public override void PreOpen()
        {
            NotifyHSVUpdated();
            _alphaPosition = Colour.a / UnitsPerPixel;
        }

        public static string RGBtoHex( Color col )
        {
            int r = (int)Mathf.Clamp(col.r * 256f, 0, 255);
            int g = (int)Mathf.Clamp(col.g * 256f, 0, 255);
            int b = (int)Mathf.Clamp(col.b * 256f, 0, 255);
            int a = (int)Mathf.Clamp(col.a * 256f, 0, 255);

            return "#" + r.ToString( "X2" ) + g.ToString( "X2" ) + b.ToString( "X2" ) + a.ToString( "X2" );
        }

        public static bool TryGetColorFromHex( string hex, out Color col )
        {
            Color clr = new Color(0,0,0);
            if( hex != null && hex.Length == 9 )
            {
                try
                {
                    string str = hex.Substring(1, hex.Length - 1);
                    clr.r = int.Parse( str.Substring( 0, 2 ), NumberStyles.AllowHexSpecifier ) / 255.0f;
                    clr.g = int.Parse( str.Substring( 2, 2 ), NumberStyles.AllowHexSpecifier ) / 255.0f;
                    clr.b = int.Parse( str.Substring( 4, 2 ), NumberStyles.AllowHexSpecifier ) / 255.0f;
                    if( str.Length == 8 )
                        clr.a = int.Parse( str.Substring( 6, 2 ), NumberStyles.AllowHexSpecifier ) / 255.0f;
                    else clr.a = 1.0f;
                }
                catch( Exception e )
                {
#if DEBUG
                    Log.Message("Falied to convert from" + hex + "\n" + e);
#endif
                    col = Color.white;
                    return false;
                }
                col = clr;
                return true;
            }
            col = Color.white;
            return false;
        }

        public override void DoWindowContents( Rect inRect )
        {
            // set up rects
            Rect pickerRect = new Rect(inRect.xMin, inRect.yMin, _pickerSize, _pickerSize);
            Rect hueRect = new Rect(pickerRect.xMax + _margin, inRect.yMin, _sliderWidth, _pickerSize);
            Rect alphaRect = new Rect(hueRect.xMax + _margin, inRect.yMin, _sliderWidth, _pickerSize);
            Rect previewRect = new Rect(alphaRect.xMax + _margin, inRect.yMin, _previewSize, _previewSize);
            Rect previewOldRect = new Rect(previewRect.xMax, inRect.yMin, _previewSize, _previewSize);
            Rect doneRect = new Rect(alphaRect.xMax + _margin, inRect.yMax - _fieldHeight, _previewSize * 2, _fieldHeight );
            Rect setRect = new Rect(alphaRect.xMax + _margin, inRect.yMax - 2 * _fieldHeight - _margin, _previewSize - _margin / 2, _fieldHeight);
            Rect cancelRect = new Rect(setRect.xMax + _margin, setRect.yMin, _previewSize - _margin / 2, _fieldHeight);
            Rect hexRect = new Rect(alphaRect.xMax + _margin, inRect.yMax - 3 * _fieldHeight - 2 * _margin, _previewSize * 2, _fieldHeight);

            // draw transparency backgrounds
            GUI.DrawTexture( pickerRect, PickerAlphaBG );
            GUI.DrawTexture( alphaRect, SliderAlphaBG );
            GUI.DrawTexture( previewRect, PreviewAlphaBG );
            GUI.DrawTexture( previewOldRect, PreviewAlphaBG );

            // draw picker foregrounds
            GUI.DrawTexture( pickerRect, ColourPickerBG );
            GUI.DrawTexture( hueRect, HuePickerBG );
            GUI.DrawTexture( alphaRect, AlphaPickerBG );
            GUI.DrawTexture( previewRect, TempPreviewBG );
            GUI.DrawTexture( previewOldRect, PreviewBG );

            // draw slider handles
            // TODO: get HSV from RGB for init of handles.
            Rect hueHandleRect = new Rect(hueRect.xMin - 3f , hueRect.yMin + _huePosition - _handleSize / 2, _sliderWidth + 6f, _handleSize);
            Rect alphaHandleRect = new Rect(alphaRect.xMin - 3f, alphaRect.yMin + _alphaPosition - _handleSize / 2, _sliderWidth + 6f, _handleSize);
            Rect pickerHandleRect = new Rect(pickerRect.xMin + _position.x - _handleSize / 2, pickerRect.yMin + _position.y - _handleSize / 2, _handleSize, _handleSize);
            GUI.DrawTexture( hueHandleRect, TempPreviewBG );
            GUI.DrawTexture( alphaHandleRect, TempPreviewBG );
            GUI.DrawTexture( pickerHandleRect, TempPreviewBG );

            GUI.color = Color.gray;
            Widgets.DrawBox( hueHandleRect );
            Widgets.DrawBox( alphaHandleRect );
            Widgets.DrawBox( pickerHandleRect );
            GUI.color = Color.white;

            // reset active control on mouseup
            if (Input.GetMouseButtonUp( 0 ) )
            {
                _activeControl = controls.none;
            }

            // colourpicker interaction
            if (Mouse.IsOver( pickerRect ) )
            {
                if( Input.GetMouseButtonDown( 0 ) )
                {
                    _activeControl = controls.colourPicker;
                }
                if( _activeControl == controls.colourPicker )
                {
                    Vector2 MousePosition = Event.current.mousePosition;
                    Vector2 PositionInRect = MousePosition - new Vector2(pickerRect.xMin, pickerRect.yMin);

                    PickerAction( PositionInRect );
                }
            }

            // hue picker interaction
            if (Mouse.IsOver( hueRect ) )
            {
                if( Input.GetMouseButtonDown( 0 ) )
                {
                    _activeControl = controls.huePicker;
                }
                if( Event.current.type == EventType.ScrollWheel )
                {
                    H -= Event.current.delta.y * UnitsPerPixel;
                    _huePosition = Mathf.Clamp(_huePosition + Event.current.delta.y, 0f, _pickerSize);
                    Event.current.Use();
                }
                if( _activeControl == controls.huePicker )
                {
                    float MousePosition = Event.current.mousePosition.y;
                    float PositionInRect = MousePosition - hueRect.yMin;

                    HueAction( PositionInRect );
                }
            }

            // alpha picker interaction
            if (Mouse.IsOver( alphaRect ) )
            {
                if( Input.GetMouseButtonDown( 0 ) )
                {
                    _activeControl = controls.alphaPicker;
                }
                if( Event.current.type == EventType.ScrollWheel )
                {
                    A -= Event.current.delta.y * UnitsPerPixel;
                    _alphaPosition = Mathf.Clamp( _alphaPosition + Event.current.delta.y, 0f, _pickerSize );
                    Event.current.Use();
                }
                if( _activeControl == controls.alphaPicker )
                {
                    float MousePosition = Event.current.mousePosition.y;
                    float PositionInRect = MousePosition - alphaRect.yMin;

                    AlphaAction( PositionInRect );
                }
            }

            // buttons and text field
            // for some reason scrolling sometimes changes text size
            Text.Font = GameFont.Small;
            if( Widgets.TextButton( doneRect, "OK" ) )
            {
                _wrapper.Color = tempColour;
                if (_callback != null )
                {
                    _callback();
                }
                this.Close();
            }
            if( Widgets.TextButton( setRect, "Apply" ) )
            {
                _wrapper.Color = tempColour;
                if( _callback != null )
                {
                    _callback();
                }
                SetColor();
            }
            if( Widgets.TextButton( cancelRect, "Cancel" ) )
            {
                this.Close();
            }

            if( _hexIn != _hexOut )
            {
                Color inputColor = tempColour;
                if( TryGetColorFromHex( _hexIn, out inputColor ) )
                {
                    tempColour = inputColor;
                    NotifyRGBUpdated();
                }
                else
                {
                    GUI.color = Color.red;
                }
            }
            _hexIn = Widgets.TextField( hexRect, _hexIn );
            GUI.color = Color.white;
        }

        public override Vector2 InitialWindowSize
        {
            get
            {
                // calculate window size to accomodate all elements
                return new Vector2( _pickerSize + 3 * _margin + 2 * _sliderWidth + 2 * _previewSize + Window.StandardMargin * 2, _pickerSize + Window.StandardMargin * 2 );
            }
        }
    }
}
