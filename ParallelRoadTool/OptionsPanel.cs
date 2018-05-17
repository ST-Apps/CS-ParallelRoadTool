using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;
using NetworkSkins.Props;
using NetworkSkins.Meshes;
using System.Collections.Generic;

namespace ParallelRoadTool
{
    public class OptionsPanel: UIPanel
    {
        private UITextureAtlas m_atlas;

        public UICheckBox m_anarchy;

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

            m_anarchy = CreateCheckBox(this, "Anarchy", "Toggle road anarchy", false);            

            DebugUtils.Log("OptionsPanel.Start - START m_networks");

            m_networks = new List<UINetTypeOption>{
                AddUIComponent<UINetTypeOption>()
            };

            foreach (var network in m_networks)
            {                
                network.Populate();
            }

            DebugUtils.Log("OptionsPanel.Start - END m_networks");

            UpdateOptions();

            autoLayout = true;
        }

        private UICheckBox CreateCheckBox(UIComponent parent, string spriteName, string toolTip, bool value)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = new Vector2(36, 36);

            UIButton button = checkBox.AddUIComponent<UIButton>();
            button.name = "PRT_" + spriteName;
            button.atlas = m_atlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(100, -36);            

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
            ParallelRoadTool.anarchy = m_anarchy.isChecked;
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
				"BendingPressed",
				"Snapping",
				"SnappingDisabled",
				"SnappingFocused",
				"SnappingHovered",
				"SnappingPressed",
				"Collision",
				"CollisionDisabled",
				"CollisionFocused",
				"CollisionHovered",
				"CollisionPressed",
				"Grid",
				"GridDisabled",
				"GridFocused",
				"GridHovered",
				"GridPressed"
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
