using System;

namespace ROMHackLib
{
    /// <summary>
    /// Called when the function attempts access an out
    /// </summary>
    public class OffsetOutOfRange : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the OffsetOutOfRange class.
        /// </summary>
        public OffsetOutOfRange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OffsetOutOfRange class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public OffsetOutOfRange(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OffsetOutOfRange class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public OffsetOutOfRange(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}