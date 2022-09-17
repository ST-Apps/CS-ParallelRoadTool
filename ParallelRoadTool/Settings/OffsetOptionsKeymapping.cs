using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using ColossalFramework;

namespace ParallelRoadTool.Settings
{
    /// <summary>
    ///     Keycode setting control for mod's increase horizontal offset key.
    /// </summary>
    public class IncreaseHorizontalOffsetOptionsKeymapping : OptionsKeymapping
    {
        public IncreaseHorizontalOffsetOptionsKeymapping()
        {
            Label = Translations.Translate("SELECT_KEYMAPPING_INCREASE_HORIZONTAL_OFFSET_LABEL");
            ButtonLabel = Translations.Translate("PRESS_ANY_KEY");
        }

        protected override InputKey KeySetting
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
            ButtonLabel = Translations.Translate("PRESS_ANY_KEY");
        }

        protected override InputKey KeySetting
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
            ButtonLabel = Translations.Translate("PRESS_ANY_KEY");
        }

        protected override InputKey KeySetting
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
            ButtonLabel = Translations.Translate("PRESS_ANY_KEY");
        }

        protected override InputKey KeySetting
        {
            get => ModSettings.KeyDecreaseVerticalOffset.Encode();
            set => ModSettings.KeyDecreaseVerticalOffset.SetKey(value);
        }
    }
}
