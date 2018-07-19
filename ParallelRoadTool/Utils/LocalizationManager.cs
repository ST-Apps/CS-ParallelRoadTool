using ColossalFramework.Globalization;
using ParallelRoadTool.Extensions.LocaleModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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

            XmlSerializer serializer = new XmlSerializer(typeof(NameList));

            var assembly = Assembly.GetExecutingAssembly();
            // HACK - [ISSUE-51] On Mac and Linux LocalManager.cultureInfo always returns 'en' regardless of game language
            var resourceName = $"ParallelRoadTool.Localization.{LocaleManager.instance.language}.xml";

            if (!assembly.GetManifestResourceNames().Contains(resourceName))
            {
                // Fallback to english
                resourceName = "ParallelRoadTool.Localization.en.xml";
            }

            DebugUtils.Log($"Trying to read {resourceName} localization file...");
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                using (XmlReader xmlStream = XmlReader.Create(reader))
                {
                    if (serializer.CanDeserialize(xmlStream))
                    {
                        NameList nameList = (NameList)serializer.Deserialize(xmlStream);
                        nameList.Apply();
                    }
                }
            }

            DebugUtils.Log($"Namelists {resourceName} applied.");

            DebugUtils.Log("Locale changed callback finished.");
        }

    }
}
