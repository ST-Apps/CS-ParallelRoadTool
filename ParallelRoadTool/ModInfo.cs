using ICities;

using System;
using System.Linq;

using ColossalFramework;
using ColossalFramework.UI;

using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        public ModInfo()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = ParallelRoadTool.settingsFileName } });
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public string Name
        {
            get { return "Parallel Road Tool " + version; }
        }

        public string Description
        {
            // TODO: write a description that actually makes sense
            get { return "This mod adds the ability to automatically create a parallel network of any kind while drawing a road"; }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public const string version = "0.0.1";
    }
}
