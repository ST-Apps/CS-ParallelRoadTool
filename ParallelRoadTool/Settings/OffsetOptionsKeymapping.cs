using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using ColossalFramework;

// ReSharper disable ClassNeverInstantiated.Global

namespace ParallelRoadTool.Settings
{
    /// <summary>
    ///     Keycode setting control for mod's increase horizontal offset key.
    /// </summary>
    public class IncreaseHorizontalOffsetOptionsKeymapping : OptionsKeymapping
    {
        #region Properties

        protected sealed override InputKey KeySetting
        {
            get => ModSettings.KeyIncreaseHorizontalOffset.Encode();
            set => ModSettings.KeyIncreaseHorizontalOffset.SetKey(value);
        }

        #endregion

        public IncreaseHorizontalOffsetOptionsKeymapping()
        {
            Label = Translations.Translate("SELECT_KEYMAPPING_INCREASE_HORIZONTAL_OFFSET_LABEL");
            ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
        }
    }

    /// <summary>
    ///     Keycode setting control for mod's decrease horizontal offset key.
    /// </summary>
    public class DecreaseHorizontalOffsetOptionsKeymapping : OptionsKeymapping
    {
        #region Properties

        protected sealed override InputKey KeySetting
        {
            get => ModSettings.KeyDecreaseHorizontalOffset.Encode();
            set => ModSettings.KeyDecreaseHorizontalOffset.SetKey(value);
        }

        #endregion

        public DecreaseHorizontalOffsetOptionsKeymapping()
        {
            Label = Translations.Translate("SELECT_KEYMAPPING_DECREASE_HORIZONTAL_OFFSET_LABEL");
            ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
        }
    }

    /// <summary>
    ///     Keycode setting control for mod's increase horizontal offset key.
    /// </summary>
    public class IncreaseVerticalOffsetOptionsKeymapping : OptionsKeymapping
    {
        #region Properties

        protected sealed override InputKey KeySetting
        {
            get => ModSettings.KeyIncreaseVerticalOffset.Encode();
            set => ModSettings.KeyIncreaseVerticalOffset.SetKey(value);
        }

        #endregion

        public IncreaseVerticalOffsetOptionsKeymapping()
        {
            Label = Translations.Translate("SELECT_KEYMAPPING_INCREASE_VERTICAL_OFFSET_LABEL");
            ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
        }
    }

    /// <summary>
    ///     Keycode setting control for mod's decrease Vertical offset key.
    /// </summary>
    public class DecreaseVerticalOffsetOptionsKeymapping : OptionsKeymapping
    {
        #region Properties

        protected sealed override InputKey KeySetting
        {
            get => ModSettings.KeyDecreaseVerticalOffset.Encode();
            set => ModSettings.KeyDecreaseVerticalOffset.SetKey(value);
        }

        #endregion

        public DecreaseVerticalOffsetOptionsKeymapping()
        {
            Label = Translations.Translate("SELECT_KEYMAPPING_DECREASE_VERTICAL_OFFSET_LABEL");
            ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
        }
    }
}
