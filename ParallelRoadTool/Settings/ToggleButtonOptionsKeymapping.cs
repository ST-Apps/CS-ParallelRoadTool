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
            ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
        }

        protected sealed override InputKey KeySetting
        {
            get => ModSettings.KeyToggleTool.Encode();
            set => ModSettings.KeyToggleTool.SetKey(value);
        }
    }
}
