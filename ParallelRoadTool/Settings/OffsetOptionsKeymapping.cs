// <copyright file="OffsetOptionsKeymapping.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Settings;

using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using ColossalFramework;

// ReSharper disable ClassNeverInstantiated.Global
/// <summary>
///     Keycode setting control for mod's increase horizontal offset key.
/// </summary>
public class IncreaseHorizontalOffsetOptionsKeymapping : OptionsKeymapping
{
    public IncreaseHorizontalOffsetOptionsKeymapping()
    {
        Label = Translations.Translate("SELECT_KEYMAPPING_INCREASE_HORIZONTAL_OFFSET_LABEL");
        ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
    }

    public sealed override InputKey KeySetting
    {
        get => ModSettings.KeyIncreaseHorizontalOffset.Encode();
        set => ModSettings.KeyIncreaseHorizontalOffset.SetKey(value);
    }
}

/// <summary>
///     Keycode setting control for mod's decrease horizontal offset key.
/// </summary>
public class DecreaseHorizontalOffsetOptionsKeymapping : OptionsKeymapping
{
    public DecreaseHorizontalOffsetOptionsKeymapping()
    {
        Label = Translations.Translate("SELECT_KEYMAPPING_DECREASE_HORIZONTAL_OFFSET_LABEL");
        ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
    }

    public sealed override InputKey KeySetting
    {
        get => ModSettings.KeyDecreaseHorizontalOffset.Encode();
        set => ModSettings.KeyDecreaseHorizontalOffset.SetKey(value);
    }
}

/// <summary>
///     Keycode setting control for mod's increase horizontal offset key.
/// </summary>
public class IncreaseVerticalOffsetOptionsKeymapping : OptionsKeymapping
{
    public IncreaseVerticalOffsetOptionsKeymapping()
    {
        Label = Translations.Translate("SELECT_KEYMAPPING_INCREASE_VERTICAL_OFFSET_LABEL");
        ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
    }

    public sealed override InputKey KeySetting
    {
        get => ModSettings.KeyIncreaseVerticalOffset.Encode();
        set => ModSettings.KeyIncreaseVerticalOffset.SetKey(value);
    }
}

/// <summary>
///     Keycode setting control for mod's decrease Vertical offset key.
/// </summary>
public class DecreaseVerticalOffsetOptionsKeymapping : OptionsKeymapping
{
    public DecreaseVerticalOffsetOptionsKeymapping()
    {
        Label = Translations.Translate("SELECT_KEYMAPPING_DECREASE_VERTICAL_OFFSET_LABEL");
        ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
    }

    public sealed override InputKey KeySetting
    {
        get => ModSettings.KeyDecreaseVerticalOffset.Encode();
        set => ModSettings.KeyDecreaseVerticalOffset.SetKey(value);
    }
}
