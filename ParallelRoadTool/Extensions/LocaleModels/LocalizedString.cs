using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ParallelRoadTool.Extensions.LocaleModels
{
    /// <summary>
    /// <see cref="https://github.com/markusmitbrille/cities-skylines-custom-namelists/blob/master/CSLCNL/CSLCNL/LocalizedString.cs"/>
    /// </summary>
    public struct LocalizedString
    {
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier;
        [XmlAttribute(AttributeName = "key")]
        public string Key;
        [XmlText]
        public string Value;
    }
}
