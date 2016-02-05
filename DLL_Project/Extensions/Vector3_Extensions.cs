using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityCoreLibrary
{
    public static class Vector3_Extensions
    {
        public static float Sum( this Vector3 vec )
        {
            return vec.x + vec.y + vec.z;
        }

        public static float Max( this Vector3 vec )
        {
            return Mathf.Max( vec.x, Mathf.Max( vec.y, vec.z ) );
        }

        public static float Min( this Vector3 vec )
        {
            return Mathf.Min( vec.x, Mathf.Min( vec.y, vec.z ) );
        }

        public static Vector3 Subtract( this Vector3 vec, float minus )
        {
            return new Vector3( vec.x - minus, vec.y - minus, vec.z - minus );
        }
    }
}
