using UnityEngine;
using System.Globalization;

namespace CommunityCoreLibrary.ColorPicker
{
    
    public class ColorHelper
    {
        
        /// <summary>
        /// From http://answers.unity3d.com/questions/701956/hsv-to-rgb-without-editorguiutilityhsvtorgb.html
        /// </summary>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="V"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color HSVtoRGB( float H, float S, float V, float A = 1f )
        {
            if( S == 0f )
                return new Color( V, V, V, A );
            else if( V == 0f )
                return new Color( 0f, 0f, 0f, A );
            else
            {
                Color col = Color.black;
                float Hval = H * 6f;
                int sel = Mathf.FloorToInt(Hval);
                float mod = Hval - sel;
                float v1 = V * (1f - S);
                float v2 = V * (1f - S * mod);
                float v3 = V * (1f - S * (1f - mod));
                switch( sel + 1 )
                {
                    case 0:
                        col.r = V;
                        col.g = v1;
                        col.b = v2;
                        break;
                    case 1:
                        col.r = V;
                        col.g = v3;
                        col.b = v1;
                        break;
                    case 2:
                        col.r = v2;
                        col.g = V;
                        col.b = v1;
                        break;
                    case 3:
                        col.r = v1;
                        col.g = V;
                        col.b = v3;
                        break;
                    case 4:
                        col.r = v1;
                        col.g = v2;
                        col.b = V;
                        break;
                    case 5:
                        col.r = v3;
                        col.g = v1;
                        col.b = V;
                        break;
                    case 6:
                        col.r = V;
                        col.g = v1;
                        col.b = v2;
                        break;
                    case 7:
                        col.r = V;
                        col.g = v3;
                        col.b = v1;
                        break;
                }
                col.r = Mathf.Clamp( col.r, 0f, 1f );
                col.g = Mathf.Clamp( col.g, 0f, 1f );
                col.b = Mathf.Clamp( col.b, 0f, 1f );
                col.a = Mathf.Clamp( A, 0f, 1f );
                return col;
            }
        }

        /// <summary>
        /// From http://answers.unity3d.com/comments/865281/view.html
        /// </summary>
        /// <param name="rgbColor"></param>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="V"></param>
        public static void RGBtoHSV( Color rgbColor, out float H, out float S, out float V )
        {
            if( rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r )
            {
                RGBtoHSV_Helper( 4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V );
            }
            else
            {
                if( rgbColor.g > rgbColor.r )
                {
                    RGBtoHSV_Helper( 2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V );
                }
                else
                {
                    RGBtoHSV_Helper( 0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V );
                }
            }
        }

        // From http://answers.unity3d.com/comments/865281/view.html
        private static void RGBtoHSV_Helper( float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V )
        {
            V = dominantcolor;
            if( V != 0f )
            {
                float num = 0f;
                if( colorone > colortwo )
                {
                    num = colortwo;
                }
                else
                {
                    num = colorone;
                }
                float num2 = V - num;
                if( num2 != 0f )
                {
                    S = num2 / V;
                    H = offset + ( colorone - colortwo ) / num2;
                }
                else
                {
                    S = 0f;
                    H = offset + ( colorone - colortwo );
                }
                H /= 6f;
                if( H < 0f )
                {
                    H += 1f;
                }
            }
            else
            {
                S = 0f;
                H = 0f;
            }
        }

        public static string RGBtoHex( Color col )
        {
            // this is RGBA, which seems to be common in some parts.
            // ARGB is also common, but oh well.
            int r = (int)Mathf.Clamp(col.r * 256f, 0, 255);
            int g = (int)Mathf.Clamp(col.g * 256f, 0, 255);
            int b = (int)Mathf.Clamp(col.b * 256f, 0, 255);
            int a = (int)Mathf.Clamp(col.a * 256f, 0, 255);

            return "#" + r.ToString( "X2" ) + g.ToString( "X2" ) + b.ToString( "X2" ) + a.ToString( "X2" );
        }


        /// <summary>
        /// Attempt to get a numerical representation of an RGB(A) hexademical color string.
        /// </summary>
        /// <param name="hex">7 or 9 long string (including hashtag)</param>
        /// <param name="col">updated with the parsed color on succes</param>
        /// <returns>bool success</returns>
        public static bool TryHexToRGB( string hex, ref Color col )
        {
            Color clr = new Color(0,0,0);
            if(
                ( hex != null )&&
                (
                    ( hex.Length == 9 )||
                    ( hex.Length == 7 )
                )
            )
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
                catch
                {
                    return false;
                }
                col = clr;
                return true;
            }
            return false;
        }

    }

}
