using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public static class List_Extensions
    {
        
        public static void AddUnique<T>( this List<T> list, T item )
        {
            if(
                ( list == null )||
                ( list.Contains( item ) )
            )
            {
                return;
            }
            list.Add( item );
        }

        public static void AddRangeUnique<T>( this List<T> list, IEnumerable<T> items )
        {
            if(
                ( list == null )||
                ( items == null )
            )
            {
                return;
            }
            foreach( var item in items )
            {
                if( !list.Contains( item ) )
                {
                    list.Add( item );
                }
            }
        }

    }
}
