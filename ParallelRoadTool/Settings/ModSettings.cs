using System.Xml.Serialization;
using AlgernonCommons.Keybinding;
using AlgernonCommons.XML;
using UnityEngine;

namespace ParallelRoadTool.Settings
{
    /// <summary>
    ///     Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class ModSettings : SettingsXMLBase
    {
        #region Fields

        /// <summary>
        ///     Filename for our custom settings file
        /// </summary>
        private const string SettingsFileName = "PRT_Settings.xml";

        /// <summary>
        ///     Tool toggle key.
        /// </summary>
        [XmlIgnore] public static readonly Keybinding KeyToggleTool = new(KeyCode.P, true, false, false);

        /// <summary>
        ///     Increase horizontal offset key.
        /// </summary>
        [XmlIgnore] public static readonly Keybinding KeyIncreaseHorizontalOffset = new(KeyCode.Equals, true, false, false);

        /// <summary>
        ///     Decrease horizontal offset key.
        /// </summary>
        [XmlIgnore] public static readonly Keybinding KeyDecreaseHorizontalOffset = new(KeyCode.Minus, true, false, false);

        /// <summary>
        ///     Increase horizontal offset key.
        /// </summary>
        [XmlIgnore] public static readonly Keybinding KeyIncreaseVerticalOffset = new(KeyCode.Equals, true, true, false);

        /// <summary>
        ///     Decrease horizontal offset key.
        /// </summary>
        [XmlIgnore] public static readonly Keybinding KeyDecreaseVerticalOffset = new(KeyCode.Minus, true, true, false);

        #endregion

        #region Serializable

        /// <summary>
        ///     Tool toggle key.
        /// </summary>
        [XmlElement(nameof(KeyToggleTool))]
        public Keybinding XMLKeyToggleTool
        {
            get => KeyToggleTool;
            set => KeyToggleTool.SetKey(value.Encode());
        }

        /// <summary>
        ///     Increase horizontal offset key.
        /// </summary>
        [XmlElement(nameof(KeyIncreaseHorizontalOffset))]
        public Keybinding XMLKeyIncreaseHorizontalOffset
        {
            get => KeyIncreaseHorizontalOffset;
            set => KeyIncreaseHorizontalOffset.SetKey(value.Encode());
        }

        /// <summary>
        ///     Decrease horizontal offset key.
        /// </summary>
        [XmlElement(nameof(KeyDecreaseHorizontalOffset))]
        public Keybinding XMLKeyDecreaseHorizontalOffset
        {
            get => KeyDecreaseHorizontalOffset;
            set => KeyDecreaseHorizontalOffset.SetKey(value.Encode());
        }

        /// <summary>
        ///     Increase horizontal offset key.
        /// </summary>
        [XmlElement(nameof(KeyIncreaseVerticalOffset))]
        public Keybinding XMLKeyIncreaseVerticalOffset
        {
            get => KeyIncreaseVerticalOffset;
            set => KeyIncreaseVerticalOffset.SetKey(value.Encode());
        }

        /// <summary>
        ///     Decrease horizontal offset key.
        /// </summary>
        [XmlElement(nameof(KeyDecreaseVerticalOffset))]
        public Keybinding XMLKeyDecreaseVerticalOffset
        {
            get => KeyDecreaseVerticalOffset;
            set => KeyDecreaseVerticalOffset.SetKey(value.Encode());
        }

        #endregion

        #region Control

        #region Internals

        /// <summary>
        ///     Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            XMLFileUtils.Load<ModSettings>(SettingsFileName);
        }

        /// <summary>
        ///     Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            XMLFileUtils.Save<ModSettings>(SettingsFileName);
        }

        #endregion

        #endregion
    }
}
