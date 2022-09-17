using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Settings;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UIOptionsPanel : UIPanel
    {
        public UIOptionsPanel()
        {
            // Main
            padding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayout = true;

            // Helper needed to fill the options groups
            var helper = new UIHelper(this);

            // Main group used just as header
            var topGroup = (UIHelper)helper.AddGroup(Mod.Instance.Name);

            // Main/Warning
#if DEBUG
            var warningText = ((UIPanel)topGroup.self).AddUIComponent<UILabel>();
            warningText.text = Translations.Translate("BETA_WARNING_TEXT");
            warningText.textColor = Color.magenta;
            warningText.textAlignment = UIHorizontalAlignment.Center;
            warningText.verticalAlignment = UIVerticalAlignment.Middle;
#endif

            // Main/Language
            var languageDropdown = (UIDropDown)topGroup.AddDropdown(Translations.Translate("CHOOSE_LANGUAGE"), Translations.LanguageList,
                                                                    Translations.Index, index =>
                                                                                        {
                                                                                            Translations.Index = index;
                                                                                            OptionsPanelManager<UIOptionsPanel>.LocaleChanged();
                                                                                        });

            // Main/Keybindings
            var keyBindingsGroup = (UIHelper)helper.AddGroup(Translations.Translate("GROUP_KEYBINDINGS_NAME"));
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<ToggleButtonOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<IncreaseHorizontalOffsetOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<DecreaseHorizontalOffsetOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<IncreaseVerticalOffsetOptionsKeymapping>();
            ((UIPanel)keyBindingsGroup.self).gameObject.AddComponent<DecreaseVerticalOffsetOptionsKeymapping>();

            // Main/Buttons
            var buttonsGroup = (UIHelper)helper.AddGroup(Translations.Translate("GROUP_BUTTONS_NAME"));
            var buttonsGroupAsPanel = (UIPanel)buttonsGroup.self;
            buttonsGroupAsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            buttonsGroupAsPanel.autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);

            var windowResetButton = buttonsGroup.AddButton(Translations.Translate("BUTTON_RESET_TOOL_WINDOW_POSITION_LABEL"), () =>
                                                         {
                                                             if (!Singleton<UIController>.exists) return;
                                                             Singleton<UIController>.instance
                                                                                    .ResetToolWindowPosition();
                                                         });
            var toolResetButton = buttonsGroup.AddButton(Translations.Translate("BUTTON_RESET_TOOL_BUTTON_POSITION_LABEL"),
                                   () =>
                                   {
                                       if (!Singleton<UIController>.exists) return;
                                       Singleton<UIController>.instance.ResetToolButtonPosition();
                                   });

            // Finally try to fit everything to our parent
            this.FitWidth(parent, UIConstants.Padding);
#if DEBUG
            warningText.FitWidth(this, UIConstants.Padding);
#endif
            languageDropdown.FitWidth(this, UIConstants.Padding);
            buttonsGroupAsPanel.FitWidth(this, UIConstants.Padding);
        }
    }
}
