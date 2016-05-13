using System.Text;

using UnityEngine;
using Verse;


namespace CommunityCoreLibrary
{

    public interface IConfigurable : IExposable
    {

        float                               DrawMCMRegion( Rect InRect );

    }

}
