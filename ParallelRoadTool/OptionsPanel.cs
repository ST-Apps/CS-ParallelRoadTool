using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;
using NetworkSkins.Meshes;
using System.Collections.Generic;
using System.Linq;

namespace ParallelRoadTool
{
    public class OptionsPanel: UIPanel
    {
        private UITextureAtlas m_atlas;

        public UICheckBox m_parallel;

        public UIButton m_addMoreNetworks;

        public List<UINetTypeOption> m_networks;

        public override void Start()
        {
            LoadResources();

            name = "PRT_OptionsPanel";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "GenericPanel";
            color = new Color32(206, 206, 206, 255);
            size = new Vector2(300, 52);
            relativePosition = new Vector2(8, 92);

            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            autoLayoutDirection = LayoutDirection.Horizontal;

            m_parallel = CreateCheckBox(this, "Anarchy", "Toggle parallel road tool", false);
            /*m_addMoreNetworks = CreateButton(this, "Bending", "Add another parallel network", (c, p) => {
                // TODO: support for multiple parallel networks
                var network = CreateNetworksDropdown();
                m_networks.Add(network);
                network.Populate();
            });*/

            m_networks = new List<UINetTypeOption>{
                CreateNetworksDropdown()
            };      

            UpdateOptions();

            autoLayout = true;
        }

        private UINetTypeOption CreateNetworksDropdown()
        {
            var dropdown = AddUIComponent<UINetTypeOption>();
            dropdown.Populate();

            dropdown.SelectionChangedCallback = () =>
            {
                DebugUtils.Log($"OptionsPanel.SelectionChangedCallback() - Selected {dropdown.selectedNetInfo.name}");
                UpdateOptions();
            };            

            return dropdown;
        }

        private UIButton CreateButton(UIComponent parent, string spriteName, string toolTip, MouseEventHandler clickAction)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = "PRT_" + spriteName;
            button.atlas = m_atlas;
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
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = new Vector2(36, 36);

            UIButton button = checkBox.AddUIComponent<UIButton>();
            button.name = "PRT_" + spriteName;
            button.atlas = m_atlas;
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

                UpdateOptions();
            };

            return checkBox;
        }

        private void UpdateOptions()
        {
            DebugUtils.Log("OptionsPanel.UpdateOptions()");

            ParallelRoadTool.IsParallelEnabled = m_parallel.isChecked;
            ParallelRoadTool.SelectedRoadTypes = m_networks.Select(n => n.selectedNetInfo).ToList();

            foreach (var item in m_networks)
            {
                item.enabled = m_parallel.isChecked;
            }
        }

        private void LoadResources()
        {
            string[] spriteNames = new string[]
			{
				"Anarchy",
				"AnarchyDisabled",
				"AnarchyFocused",
				"AnarchyHovered",
				"AnarchyPressed",
				"Bending",
				"BendingDisabled",
				"BendingFocused",
				"BendingHovered",
				"BendingPressed"
			};

            m_atlas = ResourceLoader.CreateTextureAtlas("ParallelRoadTool", spriteNames, "ParallelRoadTool.Icons.");

            UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[]
            {
                defaultAtlas["OptionBase"].texture,
                defaultAtlas["OptionBaseFocused"].texture,
                defaultAtlas["OptionBaseHovered"].texture,
                defaultAtlas["OptionBasePressed"].texture,
                defaultAtlas["OptionBaseDisabled"].texture
            };

            ResourceLoader.AddTexturesInAtlas(m_atlas, textures);
        }
    }
}
