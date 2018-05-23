using System;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        public const string Version = "0.0.1";

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

        public string Name => "Parallel Road Tool " + Version;

        public string Description =>
            "This mod adds the ability to automatically create a parallel network of any kind while drawing a road";

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