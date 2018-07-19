using System;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        public const string Version = "1.0.2";
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
                // BUG: No translated strings in Option screen on Main Menu.
                var ld = new ParallelRoadToolLoader();
                ld.OnLocaleChanged();

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
