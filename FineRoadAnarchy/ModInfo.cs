using ICities;

using System;
using System.Linq;

using ColossalFramework;
using ColossalFramework.UI;

using ColossalFramework.Steamworks;

namespace FineRoadAnarchy
{
    public class ModInfo : IUserMod
    {
        public ModInfo()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FineRoadAnarchy.settingsFileName } });
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public string Name
        {
            get { return "Fine Road Anarchy " + version; }
        }

        public string Description
        {
            get { return "Goodbye Sharp Junction Angle"; }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                // Unsubscribe from SJA
                PublishedFileId SJA = new PublishedFileId(553184329);
                if (Steam.active && Steam.workshop.GetSubscribedItems().Contains(SJA))
                {
                    Steam.workshop.Unsubscribe(SJA);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public const string version = "1.1.4";
    }
}
