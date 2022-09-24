using System.Xml.Serialization;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     Wrapper for all the network's properties that we want to save in a preset.
    /// </summary>
    [XmlRoot(ElementName = "NetItem")]
    public class XMLNetItem
    {
        #region Properties

        /// <summary>
        ///     Horizontal offset, relative to the main network
        /// </summary>
        [XmlElement]
        public float HorizontalOffset { get; set; }

        /// <summary>
        ///     True if the direction is reversed
        /// </summary>
        [XmlElement]
        public bool IsReversed { get; set; }

        /// <summary>
        ///     Name of the network. This is in-game's basic name, not the display one.
        /// </summary>
        [XmlElement]
        public string Name { get; set; }

        /// <summary>
        ///     Vertical offset, relative to the main network
        /// </summary>
        [XmlElement]
        public float VerticalOffset { get; set; }

        #endregion
    }
}
