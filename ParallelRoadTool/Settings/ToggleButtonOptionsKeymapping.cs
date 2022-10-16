using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using ColossalFramework;

// ReSharper disable ClassNeverInstantiated.Global

namespace ParallelRoadTool.Settings
{
    /// <summary>
    ///     Keycode setting control for mod's toggle key.
    /// </summary>
    public class ToggleButtonOptionsKeymapping : OptionsKeymapping
    {
        #region Properties

        public sealed override InputKey KeySetting
        {
            get => ModSettings.KeyToggleTool.Encode();
            set => ModSettings.KeyToggleTool.SetKey(value);
        }

        #endregion

        public ToggleButtonOptionsKeymapping()
        {
            Label       = Translations.Translate("SELECT_KEYMAPPING_TOGGLE_BUTTON_LABEL");
            ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", KeySetting);
        }
    }
}
