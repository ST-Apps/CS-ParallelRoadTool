using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using ColossalFramework;

namespace ParallelRoadTool.Settings
{
    /// <summary>
    ///     Keycode setting control for mod's toggle key.
    /// </summary>
    public class ToggleButtonOptionsKeymapping : OptionsKeymapping
    {
        public ToggleButtonOptionsKeymapping()
        {
            Label = Translations.Translate("SELECT_KEYMAPPING_TOGGLEBUTTON_LABEL");
            ButtonLabel = Translations.Translate("PRESS_ANY_KEY");
        }

        protected override InputKey KeySetting
        {
            get => ModSettings.KeyPaste.Encode();
            set => ModSettings.KeyPaste.SetKey(value);
        }
    }
}
