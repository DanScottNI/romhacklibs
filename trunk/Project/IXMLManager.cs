using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ROMHackLib.Project
{
    /// <summary>
    /// Interface for the individual parts of a project to implement, so they
    /// are persist their settings.
    /// </summary>
    /// <remarks>
    /// Based on DahrkDaiz's IXMLIO interface.
    /// </remarks>
    public interface IXMLProject
    {
        /// <summary>
        /// Creates an XML element.
        /// </summary>
        /// <returns>A XElement object that is used to persist the project's settings.</returns>
        XElement CreateItem();

        /// <summary>
        /// Loads a project item from an XElement object.
        /// </summary>
        /// <param name="item">The XElement object to load the project item from.</param>
        void LoadItem(XElement item);
    }
}
