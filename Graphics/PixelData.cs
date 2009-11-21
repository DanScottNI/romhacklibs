using System;

namespace ROMHackLib.Graphics
{
    /// <summary>
    /// A struct used to simplify access to the pixels.
    /// </summary>
    public struct PixelData
    {
        /// <summary>
        /// The alpha component of the pixel.
        /// </summary>
        public byte Alpha;
        
        /// <summary>
        /// The red component of the pixel.
        /// </summary>
        public byte Red;
        
        /// <summary>
        /// The green component of the pixel.
        /// </summary>
        public byte Green;
        
        /// <summary>
        /// The blue component of the pixel.
        /// </summary>
        public byte Blue;
    }
}