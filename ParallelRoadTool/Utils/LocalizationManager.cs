using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework.Globalization;
using ParallelRoadTool.Models;

namespace ParallelRoadTool.Utils
{
    public static class LocalizationManager
    {
        private static bool _loaded;

        public static void LoadLocalization()
        {
            if (_loaded) return;

            // Add post locale change event handlers
            LocaleManager.eventLocaleChanged += OnLocaleChanged;

            DebugUtils.Log("Added locale change event handlers.");

            // Reload the current locale once to effect changes
            LocaleManager.ForceReload();

            _loaded = true;
        }

        public static void UnloadLocalization()
        {
            // Remove post locale change event handlers
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;

            DebugUtils.Log("Removed locale change event handlers.");

            // Reload the current locale once to effect changes
            LocaleManager.ForceReload();
        }

        public static void OnLocaleChanged()
        {
            DebugUtils.Log("Locale changed callback started.");

            var serializer = new XmlSerializer(typeof(NameList));

            var assembly = Assembly.GetExecutingAssembly();
            // HACK - [ISSUE-51] On Mac and Linux LocalManager.cultureInfo always returns 'en' regardless of game language
            var resourceName = $"{Configuration.LocalizationNamespace}.{LocaleManager.instance.language}.xml";

            if (!assembly.GetManifestResourceNames().Contains(resourceName))
                resourceName = $"{Configuration.LocalizationNamespace}.en.xml";

            DebugUtils.Log($"Trying to read {resourceName} localization file...");
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                using (var xmlStream = XmlReader.Create(reader))
                {
                    if (serializer.CanDeserialize(xmlStream))
                    {
                        var nameList = (NameList) serializer.Deserialize(xmlStream);
                        nameList.Apply();
                    }
                }
            }

            DebugUtils.Log($"Namelists {resourceName} applied.");

            DebugUtils.Log("Locale changed callback finished.");
        }
    }
}