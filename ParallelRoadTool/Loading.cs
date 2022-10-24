// <copyright file="Loading.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool;

using AlgernonCommons.Patching;
using ColossalFramework;
using CSUtil.Commons;
using ICities;
using Managers;
using System.Collections.Generic;
using UI.Settings;
using UnityEngine;

/// <summary>
///     Main loading class: the mod runs from here.
/// </summary>
public sealed class Loading : PatcherLoadingBase<UIOptionsPanel, PatcherBase>
{
    /// <summary>
    ///     Gets a list of permitted loading modes.
    /// </summary>
    protected override List<AppMode> PermittedModes => new () { AppMode.Game, AppMode.MapEditor, AppMode.AssetEditor };

    public override void OnReleased()
    {
        Object.DestroyImmediate(Singleton<ParallelRoadToolManager>.instance);
    }

    /// <summary>
    ///     Performs any actions upon successful creation of the mod.
    ///     E.g. Can be used to patch any other mods.
    /// </summary>
    /// <param name="loading">Loading mode (e.g. game or editor).</param>
    protected override void CreatedActions(ILoading loading)
    {
        // Set current game mode, we can't load some stuff if we're not in game (e.g. Map Editor)
        ParallelRoadToolManager.IsInGameMode = loading.currentMode == AppMode.Game;

        // Register settings file
        GameSettings.AddSettingsFile(new SettingsFile { fileName = Constants.SettingsFileName });
    }

    /// <summary>
    ///     Performs any actions upon successful level loading completion.
    /// </summary>
    /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
    protected override void LoadedActions(LoadMode mode)
    {
        Log.Info($"[{nameof(Loading)}.{nameof(LoadedActions)}] Starting mod...");

        if (!Singleton<ParallelRoadToolManager>.exists)
        {
            Singleton<ParallelRoadToolManager>.Ensure();
        }
        else
        {
            Singleton<ParallelRoadToolManager>.instance.Start();
        }

        Log.Info($"[{nameof(Loading)}.{nameof(LoadedActions)}] Mod started");
    }
}
