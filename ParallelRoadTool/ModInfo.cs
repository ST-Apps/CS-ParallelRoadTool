using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Extensions.LocaleModels;
using UnityEngine;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        public const string Version = "1.0.0";
#if DEBUG
        public const string Branch = "dev";
#endif

        public ModInfo()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile { fileName = ParallelRoadTool.SettingsFileName });
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could not load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

#if DEBUG
        public string Name => $"[BETA] Parallel Road Tool {Version}-{Branch}";
#else
        public string Name => $"Parallel Road Tool {Version}";
#endif

        public string Description =>
            "This mod allows players to easily draw parallel/stacked roads in Cities: Skylines.";

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(NameList));

                var assembly = Assembly.GetExecutingAssembly();
                // BUG: on Mac and Linux LocalManager.cultureInfo always returns 'en' regardless of game language
                //var resourceName = $"ParallelRoadTool.Localization.{LocaleManager.cultureInfo.TwoLetterISOLanguageName}.xml";
                var lang = LocaleManager.instance.language;
                var resourceName = $"ParallelRoadTool.Localization.{lang}.xml";

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

                var group = helper.AddGroup(Name) as UIHelper;
                var panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }
    }
}