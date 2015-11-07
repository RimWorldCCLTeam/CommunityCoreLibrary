using UnityEngine;

namespace CommunityCoreLibrary.ColourPicker
{
    /// <summary>
    /// This class exists only to have a reference type for Color.
    /// </summary>
    public class ColourWrapper
    {
        public Color Color { get; set; }

        public ColourWrapper( Color color )
        {
            Color = color;
        }
    }
}
