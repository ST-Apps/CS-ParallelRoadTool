using AlgernonCommons.Notifications;
using AlgernonCommons;
using AlgernonCommons.Patching;
using AlgernonCommons.Translation;
using ICities;
using ParallelRoadTool.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using CSUtil.Commons;
using ParallelRoadTool.Utils;
using Object = UnityEngine.Object;

namespace ParallelRoadTool
{
    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<UIOptionsPanel, PatcherBase>
    {

        #region Lifecycle


        /// <summary>
        /// Performs any actions upon successful creation of the mod.
        /// E.g. Can be used to patch any other mods.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game or editor).</param>
        protected override void CreatedActions(ILoading loading)
        {
            // Set current game mode, we can't load some stuff if we're not in game (e.g. Map Editor)
            ParallelRoadTool.IsInGameMode = loading.currentMode == AppMode.Game;
        }

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            // First step is always to load localization
            Log.Info($"[{nameof(Loading)}.{nameof(LoadedActions)}] Loading localization...");
            LocalizationManager.LoadLocalization();
            Log.Info($"[{nameof(Loading)}.{nameof(LoadedActions)}] Localization loaded");

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

            // Unload the localization
            LocalizationManager.UnloadLocalization();
        }
    }

    #endregion

}