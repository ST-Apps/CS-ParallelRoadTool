using System.Xml.Serialization;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     <see
    ///         cref="https://github.com/markusmitbrille/cities-skylines-custom-namelists/blob/master/CSLCNL/CSLCNL/LocalizedStringKey.cs" />
    /// </summary>
    public struct LocalizedStringKey
    {
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier;

        [XmlAttribute(AttributeName = "key")] public string Key;
    }
}
