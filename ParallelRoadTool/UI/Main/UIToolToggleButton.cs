﻿// <copyright file="UIToolToggleButton.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Main;

using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using UnityEngine;
using Utils;

/// <summary>
///     This is the main toggle button for the mod.
///     Its default position is just besides the upgrade road tool.
///     It's also draggable using the right mouse button.
/// </summary>
public class UIToolToggleButton : UIRightDragHandle
{
    private static readonly SavedInt SavedToggleX = new ("toggleX", Constants.SettingsFileName, -1000, true);

    private static readonly SavedInt SavedToggleY = new ("toggleY", Constants.SettingsFileName, -1000, true);

    private readonly UICheckBox _toggleCheckBox;

    public UIToolToggleButton()
    {
        // HACK - [ISSUE-26] Tool's main button must be draggable to prevent overlapping other mods buttons.
        // Main
        size = new Vector2(UIConstants.MediumSize, UIConstants.MediumSize);
        relativePosition = Vector3.zero;

        // Main/Checkbox
        var toggleAtlas = UITextures.LoadQuadSpriteAtlas("PRT-Toggle");
        _toggleCheckBox = UICheckBoxes.AddIconToggle(
            this,
            0,
            0,
            toggleAtlas.name,
            "pressed",
            "normal",
            backgroundSprite: null,
            tooltip: Translations.Translate("TOOLTIP_TOOL_TOGGLE_BUTTON"),
            height: UIConstants.MediumSize,
            width: UIConstants.MediumSize);

        target = _toggleCheckBox;
    }

    public event PropertyChangedEventHandler<bool> EventCheckChanged
    {
        add => _toggleCheckBox.eventCheckChanged += value;
        remove => _toggleCheckBox.eventCheckChanged -= value;
    }

    public bool IsChecked
    {
        get => _toggleCheckBox.isChecked;
        set => _toggleCheckBox.isChecked = value;
    }

    public override void Start()
    {
        base.Start();

        AttachToEvents();

        // Restore saved position, if any, otherwise reset it to default
        if (SavedToggleX != -1000 && SavedToggleY != -1000)
        {
            absolutePosition = new Vector3(SavedToggleX, SavedToggleY);
        }
        else
        {
            ResetPosition();
        }
    }

    public override void OnDestroy()
    {
        DetachFromEvents();

        base.OnDestroy();
    }

    /// <summary>
    ///     Reset position by setting it as the latest button in tool mode <see cref="UITabstrip" />, right after the upgrade
    ///     tool.
    /// </summary>
    public void ResetPosition()
    {
        // We need to have both the following components to set our position
        var tsBar = GetUIView()?.FindUIComponent("TSBar");
        if (tsBar == null)
        {
            Log.Info(@$"[{nameof(UIToolToggleButton)}.{nameof(ResetPosition)}] Couldn't find ""TSBar"", aborting.");
            return;
        }

        var toolModeBar = tsBar.Find<UITabstrip>("ToolMode");
        if (toolModeBar == null)
        {
            Log.Info(@$"[{nameof(UIToolToggleButton)}.{nameof(ResetPosition)}] Couldn't find ""ToolMode"", aborting.");
            return;
        }

        // We can now set the absolute position at the right of the toolbar
        absolutePosition = new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1, toolModeBar.absolutePosition.y);

        // We also update the saved position
        UpdateSavedPosition();
    }

    private void UIToolToggleButton_eventPositionChanged(UIComponent component, Vector2 value)
    {
        UpdateSavedPosition();
    }

    private void AttachToEvents()
    {
        eventPositionChanged += UIToolToggleButton_eventPositionChanged;
    }

    private void DetachFromEvents()
    {
        eventPositionChanged -= UIToolToggleButton_eventPositionChanged;
    }

    private void UpdateSavedPosition()
    {
        SavedToggleX.value = (int)absolutePosition.x;
        SavedToggleY.value = (int)absolutePosition.y;
    }
}
