using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework.Globalization;
using CSUtil.Commons;
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

            Log.Info($"[{nameof(LocalizationManager)}.{nameof(LoadLocalization)}] Added locale change event handlers");

            // Reload the current locale once to effect changes
            LocaleManager.ForceReload();

            _loaded = true;
        }

        public static void UnloadLocalization()
        {
            // Remove post locale change event handlers
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;

            Log.Info($"[{nameof(LocalizationManager)}.{nameof(UnloadLocalization)}] Removed locale change event handlers");

            // Reload the current locale once to effect changes
            LocaleManager.ForceReload();
        }

        public static void OnLocaleChanged()
        {
            Log.Info($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Locale changed callback started.");

            var serializer = new XmlSerializer(typeof(NameList));

            var assembly = Assembly.GetExecutingAssembly();
            // HACK - [ISSUE-51] On Mac and Linux LocalManager.cultureInfo always returns 'en' regardless of game language
            var resourceName = $"{Configuration.LocalizationNamespace}.{LocaleManager.instance.language}.xml";

            if (!assembly.GetManifestResourceNames().Contains(resourceName))
                resourceName = $"{Configuration.LocalizationNamespace}.en.xml";

            Log.Info($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Trying to read {resourceName} localization file...");

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

            Log._Debug($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Namelists {resourceName} applied.");
            Log.Info($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Locale changed callback finished.");
        }
    }
}