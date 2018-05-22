using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using ParallelRoadTool.Detours;
using ParallelRoadTool.EventArgs;
using ParallelRoadTool.Redirection;
using ParallelRoadTool.UI;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    /// TODO: this class is a mess because this won't be in the final release.
    /// </summary>
    public class OptionsPanel : ParallelRoadToolUIPanel
    {
        public UIButton m_addMoreNetworks;
        public List<UINetTypeOption> m_networks = new List<UINetTypeOption>();
        public UICheckBox m_parallel;

        public override void Start()
        {
            LoadResources();

            name = "PRT_OptionsPanel";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "GenericPanel";
            color = new Color32(206, 206, 206, 255);
            size = new Vector2(400, 52);
            relativePosition = new Vector2(8, 92);

            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            autoLayoutDirection = LayoutDirection.Vertical;

            m_parallel = CreateCheckBox(this, "Anarchy", "Toggle parallel road tool", false);
            m_addMoreNetworks = CreateButton(this, "Bending", "Add another parallel network", (c, p) =>
            {
                // TODO: added networks should also be removable or, as for now, once you add another network you can't removed it anymore
                // TODO: UI MUST be fixed before release, current one sucks.                
                CreateNetworksDropdown();
            });

            CreateNetworksDropdown();

            // UpdateOptions();

            autoLayout = true;
        }

        private void RaiseOnNetworksConfigurationChanged(object sender)
        {
            OnNetworksConfigurationChanged(sender, new NetworksConfigurationChangedEventArgs
            {                
                NetworkConfigurations = m_networks.Select(n => new Tuple<NetInfo, float>(n.SelectedNetInfo, n.Offset))
            });
        }

        private void RaiseOnParallelToolToggled(object sender)
        {
            OnParallelToolToggled(sender, new ParallelToolToggledEventArgs { IsEnabled = (sender as UICheckBox).isChecked });
        }

        private void CreateNetworksDropdown()
        {            
            var dropdown = AddUIComponent<UINetTypeOption>();
            dropdown.relativePosition = Vector2.zero;
            dropdown.enabled = false;
            dropdown.Populate();

            dropdown.OnSelectionChangedCallback = dropdown.OnHorizontalOffsetChangedCallback = dropdown.OnDeleteButtonCallback = () =>
            {
                DebugUtils.Log(
                    $"OptionsPanel.SelectionChangedCallback() - Selected {dropdown.SelectedNetInfo?.name} | index {m_networks.IndexOf(dropdown)}");

                RaiseOnNetworksConfigurationChanged(dropdown);
            };            

            m_networks.Add(dropdown);

            RaiseOnNetworksConfigurationChanged(dropdown);            
        }

        private UIButton CreateButton(UIComponent parent, string spriteName, string toolTip,
            MouseEventHandler clickAction)
        {
            var button = parent.AddUIComponent<UIButton>();
            button.name = "PRT_" + spriteName;
            button.atlas = TextureAtlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(186, -36);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            button.hoveredFgSprite = spriteName + "Hovered";
            button.pressedFgSprite = spriteName + "Pressed";
            button.disabledFgSprite = spriteName + "Disabled";

            button.eventClicked += clickAction;

            return button;
        }

        private UICheckBox CreateCheckBox(UIComponent parent, string spriteName, string toolTip, bool value)
        {
            var checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = new Vector2(36, 36);

            var button = checkBox.AddUIComponent<UIButton>();
            button.name = "PRT_" + spriteName;
            button.atlas = TextureAtlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(150, -36);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            button.hoveredFgSprite = spriteName + "Hovered";
            button.pressedFgSprite = spriteName + "Pressed";
            button.disabledFgSprite = spriteName + "Disabled";

            checkBox.isChecked = value;
            if (value)
            {
                button.normalBgSprite = "OptionBaseFocused";
                button.normalFgSprite = spriteName + "Focused";
            }

            checkBox.eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    button.normalFgSprite = spriteName + "Focused";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    button.normalFgSprite = spriteName;
                }

                RaiseOnParallelToolToggled(checkBox);

                // UpdateOptions();
            };

            return checkBox;
        }

        public void ToggleDropdowns(bool show)
        {
            foreach (var item in m_networks) item.enabled = show;
        }

    }
}