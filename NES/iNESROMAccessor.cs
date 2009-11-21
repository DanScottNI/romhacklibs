using System;

namespace ROMClass.NES
{
    /// <summary>
    /// This allows all classes that descend from this class to
    /// access the same ROM image.
    /// </summary>
    public class INESROMAccessor
    {
        /// <summary>
        /// An INESROMImage.
        /// </summary>
        protected static INESROMImage ROM;

        /// <summary>
        /// Initializes a new instance of the INESROMAccessor class.
        /// </summary>
        public INESROMAccessor()
        {
        }
    }
}
