using AlgernonCommons.Patching;
using ColossalFramework;
using CSUtil.Commons;
using ICities;
using ParallelRoadTool.UI;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<UIOptionsPanel, PatcherBase>
    {
        #region Lifecycle

        /// <summary>
        ///     Performs any actions upon successful creation of the mod.
        ///     E.g. Can be used to patch any other mods.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game or editor).</param>
        protected override void CreatedActions(ILoading loading)
        {
            // Set current game mode, we can't load some stuff if we're not in game (e.g. Map Editor)
            ParallelRoadTool.IsInGameMode = loading.currentMode == AppMode.Game;
            
            // Register settings file
            GameSettings.AddSettingsFile(new SettingsFile
            {
                fileName = Configuration.SettingsFileName
            });
        }

        /// <summary>
        ///     Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            Log.Info($"[{nameof(Loading)}.{nameof(LoadedActions)}] Starting mod...");

            if (!Singleton<ParallelRoadTool>.exists)
                Singleton<ParallelRoadTool>.Ensure();
            else
                Singleton<ParallelRoadTool>.instance.Start();

            Log.Info($"[{nameof(Loading)}.{nameof(LoadedActions)}] Mod started");
        }

        public override void OnReleased()
        {
            Object.DestroyImmediate(Singleton<ParallelRoadTool>.instance);
        }
    }

    #endregion
}
