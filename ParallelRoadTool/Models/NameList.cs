using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Utils;

namespace ParallelRoadTool.Models
{
    /// <summary>
    /// <see cref="https://github.com/markusmitbrille/cities-skylines-custom-namelists/blob/master/CSLCNL/CSLCNL/NameList.cs"/>
    /// </summary>
    public class NameList
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name;
        [XmlArray(ElementName = "strings")]
        public LocalizedString[] LocalizedStrings;

        public void Apply()
        {
            LocaleManager localeManager = SingletonLite<LocaleManager>.instance;
            foreach (LocalizedString localizedString in LocalizedStrings)
            {
                localeManager.AddString(localizedString);
            }

            DebugUtils.Log($"Namelist {Name} applied.");
        }
    }
}
