using ColossalFramework;
using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ParallelRoadTool.Extensions.LocaleModels
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
