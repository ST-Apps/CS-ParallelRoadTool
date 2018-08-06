using System.Xml.Serialization;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     <see
    ///         cref="https://github.com/markusmitbrille/cities-skylines-custom-namelists/blob/master/CSLCNL/CSLCNL/LocalizedString.cs" />
    /// </summary>
    public struct LocalizedString
    {
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier;

        [XmlAttribute(AttributeName = "key")] public string Key;
        [XmlText] public string Value;
    }
}