using System.Xml.Serialization;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     Helper class needed for serialization because NetInfo class can not be serialized
    /// </summary>
    [XmlRoot(ElementName = "NetItem")]
    public class XMLNetItem
    {
        [XmlElement] public float HorizontalOffset { get; set; }

        [XmlElement] public bool IsReversed { get; set; }

        [XmlElement] public string Name { get; set; }

        [XmlElement] public float VerticalOffset { get; set; }
    }
}
