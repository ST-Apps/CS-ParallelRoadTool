using System;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.UI;
using ParallelRoadTool.Utils;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        private const string Version = "1.2.2";
#if DEBUG
        private const string Branch = "ISSUE-64";
        public static readonly string ModName = $"[BETA] Parallel Road Tool {Version}-{Branch}";
#else
        public static readonly string ModName = $"Parallel Road Tool {Version}";
#endif

        public string Name => ModName;

        public string Description =>
            "This mod allows players to easily draw parallel/stacked roads in Cities: Skylines.";

        public ModInfo()
        {
            try
            {
                // Creating setting file 
                GameSettings.AddSettingsFile(new SettingsFile {fileName = Configuration.SettingsFileName});
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could not load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        private static bool _loading;

        public void OnSettingsUI(UIHelperBase helper)
        {
            // HACK - We need this to prevent multiple initializations that cause a CTD on activation
            if (_loading) return;
            _loading = true;

            try
            {
                // HACK - [ISSUE-51] We need to force localization loading or we won't see any localized string in mod's option while being in main menu
                LocalizationManager.LoadLocalization();

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

            _loading = false;
        }
    }
}