//using System.Xml.Serialization;
//using ColossalFramework;
//using ColossalFramework.Globalization;
//using CSUtil.Commons;
//using ParallelRoadToolManager.Extensions;

//namespace ParallelRoadToolManager.Models
//{
//    /// <summary>
//    ///     <see
//    ///         cref="https://github.com/markusmitbrille/cities-skylines-custom-namelists/blob/master/CSLCNL/CSLCNL/NameList.cs" />
//    /// </summary>
//    public class NameList
//    {
//        [XmlArray(ElementName = "strings")] public LocalizedString[] LocalizedStrings;

//        [XmlAttribute(AttributeName = "name")] public string Name;

//        public void Apply()
//        {
//            var localeManager = SingletonLite<LocaleManager>.instance;
//            foreach (var localizedString in LocalizedStrings) localeManager.AddString(localizedString);

//            Log._Debug($"[{nameof(NameList)}.{nameof(Apply)}] Namelist {Name} applied.");
//        }
//    }
//}
