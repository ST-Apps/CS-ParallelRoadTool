using System;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        public const string Version = "0.24.0";
        public const string Branch = "dev";

        public ModInfo()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile {fileName = ParallelRoadTool.SettingsFileName});
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could not load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

#if DEBUG
        public string Name => $"[BETA] Parallel Road Tool {Version} b-{Branch}";
#else
        public string Name => $"Parallel Road Tool {Version} b-{Branch}";
#endif

        public string Description =>
            "This mod allows players to easily draw parallel roads in Cities: Skylines. ";

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
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