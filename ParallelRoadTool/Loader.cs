using ColossalFramework;
using ICities;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Mod's launcher.
    /// </summary>
    public class Loader : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            // Set current game mode, we can't load some stuff if we're not in game (e.g. Map Editor)
            ParallelRoadTool.IsInGameMode = loading.currentMode == AppMode.Game;
        }

        public override void OnReleased()
        {
            Object.DestroyImmediate(Singleton<ParallelRoadTool>.instance);

            // Unload the localization
            LocalizationManager.UnloadLocalization();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            // First step is always to load localization
            LocalizationManager.LoadLocalization();

            if (!Singleton<ParallelRoadTool>.exists)
                Singleton<ParallelRoadTool>.Ensure();
            else
                Singleton<ParallelRoadTool>.instance.Start();
        }
    }
}
