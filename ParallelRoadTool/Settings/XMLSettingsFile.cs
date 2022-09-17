using AlgernonCommons.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ParallelRoadTool.Settings
{
    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class XMLSettingsFile : SettingsXMLBase
    {
        // TODO: how to store/load keybindings?
        #region Fields

        private const string SettingsFileName = "PRT_Settings.xml";

        #endregion

        #region Control

        #region Internals

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<XMLSettingsFile>(SettingsFileName);

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<XMLSettingsFile>(SettingsFileName);

        #endregion

        #endregion

    }
}
