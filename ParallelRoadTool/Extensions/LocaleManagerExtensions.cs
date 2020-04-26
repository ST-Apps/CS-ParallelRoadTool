using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.Globalization;
using CSUtil.Commons;
using ParallelRoadTool.Models;

namespace ParallelRoadTool.Extensions
{
    /// <summary>
    ///     <see
    ///         cref="https://github.com/markusmitbrille/cities-skylines-custom-namelists/blob/master/CSLCNL/CSLCNL/LocaleManagerExtensions.cs" />
    /// </summary>
    internal static class LocaleManagerExtensions
    {
        private const string localeFieldName = "m_Locale";
        private const string localizedStringsFieldName = "m_LocalizedStrings";
        private const string localizedStringsCountFieldName = "m_LocalizedStringsCount";

        private static readonly FieldInfo localeField =
            typeof(LocaleManager).GetField(localeFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo localizedStringsField =
            typeof(Locale).GetField(localizedStringsFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo localizedStringsCountField =
            typeof(Locale).GetField(localizedStringsCountFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        public static Locale GetLocale(this LocaleManager localeManager)
        {
            return (Locale) localeField.GetValue(localeManager);
        }

        public static Dictionary<Locale.Key, string> GetLocalizedStrings(this Locale locale)
        {
            return (Dictionary<Locale.Key, string>) localizedStringsField.GetValue(locale);
        }

        public static Dictionary<Locale.Key, int> GetLocalizedStringsCount(this Locale locale)
        {
            return (Dictionary<Locale.Key, int>) localizedStringsCountField.GetValue(locale);
        }

        public static void RemoveRange(this LocaleManager localeManager, Locale.Key id)
        {
            var locale = localeManager.GetLocale();

            // Set index to 0 so we can check for the string count
            id.m_Index = 0;

            if (!locale.Exists(id))
            {
                Log._Debug($"[{nameof(LocaleManagerExtensions)}.{nameof(RemoveRange)}] Could not remove locale range {id}; localized string {id} does not exist!");

                return;
            }

            var localizedStrings = locale.GetLocalizedStrings();
            var localizedStringsCount = locale.GetLocalizedStringsCount();

            for (int index = 0, lastIndex = locale.CountUnchecked(id); index <= lastIndex; index++, id.m_Index = index)
            {
                localizedStrings.Remove(id);
                localizedStringsCount.Remove(id);
            }

            Log._Debug($"[{nameof(LocaleManagerExtensions)}.{nameof(RemoveRange)}] Removed locale range {id.m_Identifier}[{id.m_Key}].");
        }

        public static void AddString(this LocaleManager localeManager, LocalizedString localizedString)
        {
            var locale = localeManager.GetLocale();

            // Construct 0-index id for the localized string from argument
            Locale.Key id;
            id.m_Identifier = localizedString.Identifier;
            id.m_Key = localizedString.Key;
            id.m_Index = 0;

            // Check if the id already exists; if so find next index
            if (locale.Exists(id)) id.m_Index = locale.CountUnchecked(id);

            // Add the localized string
            locale.AddLocalizedString(id, localizedString.Value);

            // Set the string counts accordingly
            var localizedStringCounts = locale.GetLocalizedStringsCount();

            // The count at the exact index appears to always be 0
            localizedStringCounts[id] = 0;

            // index = 0 appears to be a special case and indicates the count of localized strings with the same identifier and key
            var zeroIndexID = id;
            zeroIndexID.m_Index = 0;
            localizedStringCounts[zeroIndexID] = id.m_Index + 1;

            // Log message lags game on large namelists
            // Log($"Added localized string {id} = '{localizedString.Value}', count = {localizedStringCounts[zeroIndexID]}.");
        }
    }
}
