using UnityEngine;

namespace CommunityCoreLibrary.ColorPicker
{
    
    /// <summary>
    /// This class exists only to have a reference type for Color.
    /// </summary>
    public class ColorWrapper
    {
        
        public Color Color { get; set; }

        public ColorWrapper( Color color )
        {
            Color = color;
        }

    }

}
