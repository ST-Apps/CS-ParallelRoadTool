using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;

namespace ParallelRoadTool.UI
{
    public class UIOptionsPanel : UIPanel
    {
        public UIOptionsPanel()
        {
            var helper = new UIHelper(this);
            
            // TODO: complete!
            var languageDropDown = UIDropDowns.AddPlainDropDown(this, 0, 0, Translations.Translate("CHOOSE_LANGUAGE"), Translations.LanguageList,
                                                                Translations.Index);
            languageDropDown.eventSelectedIndexChanged += (_, index) =>
                                                          {
                                                              Translations.Index = index;
                                                              OptionsPanelManager<UIOptionsPanel>.LocaleChanged();
                                                          };

            var group = helper.AddGroup(Mod.Instance.Name);

            // Main/Keybindings

            // Main/Spacer
            group.AddSpace(UIConstants.Padding);

            // Main/Buttons
            // TODO: localization
            group.AddButton("RESET 1", () =>
                                       {
                                           if (!Singleton<UIController>.exists) return;
                                           Singleton<UIController>.instance.ResetToolWindowPosition();
                                       });
            group.AddButton("Reset 2",
                            () =>
                            {
                                if (!Singleton<UIController>.exists) return;
                                Singleton<UIController>.instance.ResetToolButtonPosition();
                            });
        }

        public void UIEOptionsPanel()
        {
            //// Main
            //var panel = AddUIComponent<UIPanel>();
            //panel.autoLayoutDirection = LayoutDirection.Vertical;

            //var helper = new UIHelper(panel);

            //var languageGroup = helper.AddGroup(Translations.Translate("CHOOSE_LANGUAGE"));

            //// Language selection.
            //UIDropDown languageDropDown = languageGroup.AddDropdown(" ", Translations.LanguageList, Translations.Index, (index) =>
            //                                                        {
            //                                                            Translations.Index = index;
            //                                                            OptionsPanelManager<UIOptionsPanel>.LocaleChanged();
            //                                                        }) as UIDropDown;
            //languageDropDown.width += 200f;

            //// Remove language dropdown label.
            //UIComponent languageParent = languageDropDown.parent;
            //if (languageParent.Find("Label") is UILabel label)
            //{
            //    languageParent.height -= label.height;
            //    label.height = 0;
            //    label.Hide();
            //}

            //// Main
            //name = $"{Configuration.ResourcePrefix}OptionsPanel";
            ////atlas = UIUtil.DefaultAtlas;
            ////backgroundSprite = "GenericPanel";
            ////color = new Color32(206, 206, 206, 255);
            ////size = new Vector2(500 - 8 * 2, 36 + 2 * 8);

            //padding = new RectOffset(8, 8, 8, 8);
            //autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            //autoLayoutDirection = LayoutDirection.Horizontal;
            //autoLayoutStart = LayoutStart.TopRight;
            //autoLayout = true;
            //autoSize = true;

            //// Helper needed to fill the options groups
            ////var rootContainer = AddUIComponent<UIPanel>();
            ////rootContainer.atlas = UIUtil.DefaultAtlas;
            ////rootContainer.backgroundSprite = "GenericPanel";
            ////rootContainer.color = new Color32(206, 206, 206, 255);
            ////rootContainer.size = new Vector2(500 - 8 * 2, 36 + 2 * 8);

            ////rootContainer.padding = new RectOffset(8, 8, 8, 8);
            ////rootContainer.autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            ////rootContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            ////rootContainer.autoLayoutStart = LayoutStart.TopRight;
            ////rootContainer.autoLayout = true;
            ////rootContainer.autoSize = false;

            //var helper = new UIHelper(this);
            //var group = helper.AddGroup(Mod.Instance.Name);

            //// Main/Keybindings

            //// Main/Spacer
            //group.AddSpace(UIConstants.Padding);

            //// Main/Buttons
            //// TODO: localization
            //group.AddButton("RESET 1", () =>
            //                           {
            //                               if (!Singleton<UIController>.exists) return;
            //                               Singleton<UIController>.instance.ResetToolWindowPosition();
            //                           });
            //group.AddButton("Reset 2",
            //                () =>
            //                {
            //                    if (!Singleton<UIController>.exists) return;
            //                    Singleton<UIController>.instance.ResetToolButtonPosition();
            //                });

            //Log._Debug($"[UIOptionsPanel.Start] Panel created with size {size} and position {position}");
        }
    }
}
