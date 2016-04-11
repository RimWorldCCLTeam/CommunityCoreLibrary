using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _CompInspectStringExtra
    {

        internal static PropertyInfo        _CompShearable_Active;
        internal static PropertyInfo        _CompShearable_ResourceDef;
        internal static PropertyInfo        _CompMilkable_Active;
        internal static PropertyInfo        _CompMilkable_ResourceDef;

        #region Reflected Methods

        #region CompShearable

        internal static bool                CompShearable_Active( this CompShearable obj )
        {
            if( _CompShearable_Active == null )
            {
                _CompShearable_Active = typeof( CompShearable ).GetProperty( "Active", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool) _CompShearable_Active.GetValue( obj, null );
        }

        internal static ThingDef            CompShearable_ResourceDef( this CompShearable obj )
        {
            if( _CompShearable_ResourceDef == null )
            {
                _CompShearable_ResourceDef = typeof( CompShearable ).GetProperty( "ResourceDef", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (ThingDef) _CompShearable_ResourceDef.GetValue( obj, null );
        }

        #endregion

        #region CompMilkable

        internal static bool                CompMilkable_Active( this CompMilkable obj )
        {
            if( _CompMilkable_Active == null )
            {
                _CompMilkable_Active = typeof( CompMilkable ).GetProperty( "Active", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool) _CompMilkable_Active.GetValue( obj, null );
        }

        internal static ThingDef            CompMilkable_ResourceDef( this CompMilkable obj )
        {
            if( _CompMilkable_ResourceDef == null )
            {
                _CompMilkable_ResourceDef = typeof( CompMilkable ).GetProperty( "ResourceDef", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (ThingDef) _CompMilkable_ResourceDef.GetValue( obj, null );
        }

        #endregion

        #endregion

        #region Detoured Methods

        internal static string _CompShearable( this CompShearable obj )
        {
            if( !obj.CompShearable_Active() )
                return (string) null;
            var str = obj.CompShearable_ResourceDef().LabelCap;
            str += " ";
            str += "CompShearableGrowth".Translate();
            str += ": ";
            str += obj.Fullness.ToStringPercent();
            return str;
        }

        internal static string _CompMilkable( this CompMilkable obj )
        {
            if( !obj.CompMilkable_Active() )
                return (string) null;
            var str = obj.CompMilkable_ResourceDef().LabelCap;
            str += " ";
            str += "CompMilkableFullness".Translate();
            str += ": ";
            str += obj.Fullness.ToStringPercent();
            return str;
        }

        #endregion

    }

}
