using System;
using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ICities;
using ParallelRoadTool.UI;
using ParallelRoadTool.Utils;

namespace ParallelRoadTool
{
    public class ModInfo : IUserMod
    {
        private const string Version = "2.0.4";
#if DEBUG
        private const string Branch = "dev";
        public static readonly string ModName = $"[BETA] Parallel Road Tool {Version}-{Branch}";
#else
        public static readonly string ModName = $"Parallel Road Tool {Version}";
#endif

        public string Name => ModName;

        public string Description =>
            "This mod allows players to easily draw parallel/stacked roads in any combination.";

        public ModInfo()
        {
            try
            {
                // Creating setting file only if it's not existing
                if (GameSettings.FindSettingsFileByName(Configuration.SettingsFileName) == null)
                    GameSettings.AddSettingsFile(new SettingsFile {fileName = Configuration.SettingsFileName});
            }
            catch (Exception e)
            {
                Log.Info($"[{nameof(ModInfo)}.{nameof(ModInfo)}] Could not load/create the setting file.");
                Log.Exception(e);
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

                if (helper == null)
                {
                    Log.Error($"[{nameof(ModInfo)}.{nameof(OnSettingsUI)}] Failed creating settings UI (helper is null)");
                    return;
                }

                if (!(helper.AddGroup(Name) is UIHelper group))
                {
                    Log.Error($"[{nameof(ModInfo)}.{nameof(OnSettingsUI)}] Failed creating settings UI (group is null)");
                    return;
                }

                if (!(group.self is UIPanel panel))
                {
                    Log.Error($"[{nameof(ModInfo)}.{nameof(OnSettingsUI)}] Failed creating settings UI (panel is null)");
                    return;
                }

                panel.gameObject.AddComponent<OptionsKeymapping>();
                group.AddSpace(10);

                // Reset main window's position
                group.AddButton("Reset tool window position",
                                () =>
                                {
                                    if (!Singleton<ParallelRoadTool>.exists) return;
                                    Singleton<ParallelRoadTool>.instance.ResetToolWindowPosition();
                                });

                group.AddSpace(10);

                // Reset tool toggle button's position
                group.AddButton("Reset tool button position",
                                () =>
                                {
                                    if (!Singleton<ParallelRoadTool>.exists) return;
                                    Singleton<ParallelRoadTool>.instance.ResetToolButtonPosition();
                                });
            }
            catch (Exception e)
            {
                Log.Info($"[{nameof(ModInfo)}.{nameof(OnSettingsUI)}] Failed creating settings UI");
                Log.Exception(e);
            }
            finally
            {
                _loading = false;
            }
        }
    }
}
