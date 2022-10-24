// <copyright file="ToggleButtonOptionsKeymapping.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Settings;

using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using ColossalFramework;

// ReSharper disable ClassNeverInstantiated.Global
/// <summary>
///     Keycode setting control for mod's toggle key.
/// </summary>
public class ToggleButtonOptionsKeymapping : OptionsKeymapping
{
    public ToggleButtonOptionsKeymapping()
    {
        Label = Translations.Translate("SELECT_KEYMAPPING_TOGGLE_BUTTON_LABEL");
        ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
    }

    public sealed override InputKey KeySetting
    {
        get => ModSettings.KeyToggleTool.Encode();
        set => ModSettings.KeyToggleTool.SetKey(value);
    }
}
