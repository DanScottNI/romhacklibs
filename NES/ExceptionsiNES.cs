using System;

namespace ROMHackLib
{
    /// <summary>
    /// A custom exception that is called when the iNESImage isn't valid.
    /// </summary>
    public class NotValidiNESImage : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotValidiNESImage class.
        /// </summary>
        public NotValidiNESImage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotValidiNESImage class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public NotValidiNESImage(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotValidiNESImage class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public NotValidiNESImage(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when a function that attempts to access CHR-ROM discovers there isn't
    /// any.
    /// </summary>
    public class CHRROMNotPresent : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the CHRROMNotPresent class.
        /// </summary>
        public CHRROMNotPresent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CHRROMNotPresent class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public CHRROMNotPresent(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CHRROMNotPresent class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public CHRROMNotPresent(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}