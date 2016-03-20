using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class GenTemperature_Extensions
    {
        
        public static float CelsiusFrom( float temp, TemperatureDisplayMode oldMode )
        {
            switch( oldMode )
            {
            case TemperatureDisplayMode.Celsius:
                return temp;
            case TemperatureDisplayMode.Fahrenheit:
                return (float) ( ( temp - 32.0 ) / 1.79999995231628 );
            case TemperatureDisplayMode.Kelvin:
                return temp - 273.15f;
            default:
                throw new InvalidOperationException();
            }
        }

    }
}
