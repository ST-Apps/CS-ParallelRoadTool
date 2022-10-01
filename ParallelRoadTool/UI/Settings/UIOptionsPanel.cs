using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Settings;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace ParallelRoadTool.UI.Settings
{
    /// <summary>
    ///     Container for all the user settings related to this mod.
    /// </summary>
    public class UIOptionsPanel : UIPanel
    {
        public UIOptionsPanel()
        {
            // Main
            padding             = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutStart     = LayoutStart.TopLeft;
            autoLayout          = true;

            // Helper needed to fill the options groups
            var helper = new UIHelper(this);

            // Main group used just as header
            var topGroup = (UIHelper)helper.AddGroup(Mod.Instance.Name);
            ((UIPanel)topGroup.self).padding.left = 0;


#if DEBUG

            // Main/Warning
            _warningText                   = ((UIPanel)topGroup.self).AddUIComponent<UILabel>();
            _warningText.text              = Translations.Translate("BETA_WARNING_TEXT");
            _warningText.textColor         = Color.magenta;
            _warningText.textAlignment     = UIHorizontalAlignment.Center;
            _warningText.verticalAlignment = UIVerticalAlignment.Middle;

            topGroup.AddSpace(UIConstants.TinySize / 2);

            // Main/RenderDebugCheckbox
            topGroup.AddCheckbox(Translations.Translate("CHECKBOX_RENDER_DEBUG_LABEL"), ModSettings.RenderDebugOverlay,
                                 value => ModSettings.RenderDebugOverlay = value);

            topGroup.AddSpace(UIConstants.TinySize / 2);
#endif

            // Main/Language
            _languageDropdown = (UIDropDown)topGroup.AddDropdown(Translations.Translate("CHOOSE_LANGUAGE"), Translations.LanguageList,
                                                                 Translations.Index, index =>
                                                                                     {
                                                                                         Translations.Index = index;
                                                                                         OptionsPanelManager<UIOptionsPanel>.LocaleChanged();
                                                                                     });

            // Main/Keybindings
            var keyBindingsGroup = (UIHelper)helper.AddGroup(Translations.Translate("GROUP_KEY_BINDINGS_NAME"));
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<ToggleButtonOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<IncreaseHorizontalOffsetOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<DecreaseHorizontalOffsetOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<IncreaseVerticalOffsetOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<DecreaseVerticalOffsetOptionsKeymapping>();

            // Main/Buttons
            var buttonsGroupHelper = (UIHelper)helper.AddGroup(Translations.Translate("GROUP_BUTTONS_NAME"));
            _buttonsGroup                     = (UIPanel)buttonsGroupHelper.self;
            _buttonsGroup.autoLayoutDirection = LayoutDirection.Horizontal;
            _buttonsGroup.autoLayoutPadding   = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);

            buttonsGroupHelper.AddButton(Translations.Translate("BUTTON_RESET_TOOL_WINDOW_POSITION_LABEL"), () =>
                                         {
                                             if (!Singleton<UIController>.exists) return;
                                             Singleton<UIController>.instance.ResetToolWindowPosition();
                                         });
            buttonsGroupHelper.AddButton(Translations.Translate("BUTTON_RESET_TOOL_BUTTON_POSITION_LABEL"), () =>
                                         {
                                             if (!Singleton<UIController>.exists) return;
                                             Singleton<UIController>.instance.ResetToolButtonPosition();
                                         });
        }

        public override void Start()
        {
            base.Start();

            // Try to fit everything to our parent
            this.FitWidth(parent, UIConstants.Padding);
#if DEBUG
            _warningText.FitWidth(this, 2 * UIConstants.Padding);
#endif
            _languageDropdown.FitWidth(this, UIConstants.Padding);
            _buttonsGroup.FitWidth(this, UIConstants.Padding);
        }

        #region Unity

        #region Components

#if DEBUG
        private readonly UILabel _warningText;
#endif

        private readonly UIDropDown _languageDropdown;
        private readonly UIPanel    _buttonsGroup;

        #endregion

        #region Lifecycle

        #endregion

        #endregion
    }
}
