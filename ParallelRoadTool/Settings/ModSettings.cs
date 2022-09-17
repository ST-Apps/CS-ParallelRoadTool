using AlgernonCommons.Keybinding;
using AlgernonCommons.XML;
using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace ParallelRoadTool.Settings
{
    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class ModSettings : SettingsXMLBase
    {
        // TODO: keybindings get reset everytime I open the panel
        #region Fields

        private const string SettingsFileName = "PRT_Settings.xml";

        #endregion

        #region Properties

        /// <summary>
        /// Tool toggle key.
        /// </summary>
        [XmlElement]
        public static readonly Keybinding KeyPaste = new(KeyCode.P, true, false, false);

        /// <summary>
        /// Increase horizontal offset key.
        /// </summary>
        [XmlElement]
        public static readonly Keybinding KeyIncreaseHorizontalOffset = new(KeyCode.Plus, true, false, false);

        /// <summary>
        /// Decrease horizontal offset key.
        /// </summary>
        [XmlElement]
        public static readonly Keybinding KeyDecreaseHorizontalOffset = new(KeyCode.Minus, true, false, false);

        /// <summary>
        /// Increase horizontal offset key.
        /// </summary>
        [XmlElement]
        public static readonly Keybinding KeyIncreaseVerticalOffset = new(KeyCode.Plus, true, true, false);

        /// <summary>
        /// Decrease horizontal offset key.
        /// </summary>
        [XmlElement]
        public static readonly Keybinding KeyDecreaseVerticalOffset = new(KeyCode.Minus, true, true, false);

        #endregion

        #region Control

        #region Internals

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFileName);

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFileName);

        #endregion

        #endregion

    }
}
