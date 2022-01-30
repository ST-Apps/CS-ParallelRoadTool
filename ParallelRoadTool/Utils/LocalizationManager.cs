using System;
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
            if (!_loaded) return;

            // Remove post locale change event handlers
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;

            Log.Info($"[{nameof(LocalizationManager)}.{nameof(UnloadLocalization)}] Removed locale change event handlers");

            _loaded = false;
        }

        private static void OnLocaleChanged()
        {
            Log.Info($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Locale changed callback started.");

            // HACK - [ISSUE-51] On Mac and Linux LocalManager.cultureInfo always returns 'en' regardless of game language
            var resourceName = $"{Configuration.LocalizationNamespace}.{LocaleManager.instance.language}.xml";

            // As default we load English resources, then we'll overwrite them with user's language (if any)
            var englishResourceName = $"{Configuration.LocalizationNamespace}.en.xml";

            Log.Info($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Trying to read {resourceName} localization file...");

            // Load all the selected languages
            ApplyLocaleResources(englishResourceName, resourceName);

            Log._Debug($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Namelists {resourceName} applied.");
            Log.Info($"[{nameof(LocalizationManager)}.{nameof(OnLocaleChanged)}] Locale changed callback finished.");
        }

        /// <summary>
        /// Loads the provided resource as a language file and also loads additional files for other languages.
        /// This allows us to load English as a shared base, and then to add custom languages on top of it.
        /// The result is that we have all the localized strings in the provided language, while still using English as a fallback for non-localized ones.
        /// </summary>
        /// <param name="resourceName"></param>
        private static bool ApplyLocaleResources(string resourceName, params string[] additionalResourcesNames)
        {
            // Read base NameList
            var nameList = ReadNameList(resourceName);
            if (nameList == null)
            {
                Log._DebugOnlyError($"[{nameof(LocalizationManager)}.{nameof(ApplyLocaleResources)}] ApplyLocaleResource(${resourceName}) failed.");
                return false;
            }
            var localizedStringsDictionary = nameList.LocalizedStrings.ToDictionary(k => $"{k.Identifier}:{k.Key}", v => v);

            // Replace LocalizedStrings using the ones provided in additionalResourcesNames. Skip default language in this list.
            foreach (var additionalResourceName in additionalResourcesNames.Where(l => resourceName != l))
            {
                // Load the additional resource
                var additionalNameList = ReadNameList(additionalResourceName);
                if (additionalNameList == null)
                {
                    Log._Debug($"[{nameof(LocalizationManager)}.{nameof(ApplyLocaleResources)}] Failed loading additional resource {additionalResourceName}.");                    return false;
                }

                // Replace strings that already exist
                foreach (var additionalLocalizedString in additionalNameList.LocalizedStrings)
                {
                    localizedStringsDictionary[$"{additionalLocalizedString.Identifier}:{additionalLocalizedString.Key}"] = additionalLocalizedString;
                }
            }

            // Reconvert to array and apply
            nameList.LocalizedStrings = localizedStringsDictionary.Values.ToArray();
            nameList.Apply();

            return true;
        }

        /// <summary>
        /// Deserializes a <see cref="NameList"/> from a given resource name inside the assembly
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        private static NameList ReadNameList(string resourceName)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(NameList));
                var assembly = Assembly.GetExecutingAssembly();

                using var stream = assembly.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream ?? throw new InvalidOperationException($"Can't load stream for file {resourceName}"));
                using var xmlStream = XmlReader.Create(reader);

                if (serializer.CanDeserialize(xmlStream))
                {
                    return (NameList)serializer.Deserialize(xmlStream);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                // Log the exception and return false
                Log._DebugOnlyError($"[{nameof(LocalizationManager)}.{nameof(ApplyLocaleResources)}] ApplyLocaleResource(${resourceName}) failed.");
                Log.Exception(e);

                return null;
            }
        }
    }
}
