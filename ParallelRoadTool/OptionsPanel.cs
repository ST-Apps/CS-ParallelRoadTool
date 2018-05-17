using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace ParallelRoadTool
{
    public class OptionsPanel: UIPanel
    {
        private UITextureAtlas m_atlas;

        public UICheckBox m_anarchy;
        public UICheckBox m_bending;
        public UICheckBox m_snapping;
        public UICheckBox m_collision;
        public UICheckBox m_grid;

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
            m_bending = CreateCheckBox(this, "Bending", "Toggle road bending", true);
            m_snapping = CreateCheckBox(this, "Snapping", "Toggle node snapping", true);
            m_collision = CreateCheckBox(this, "Collision", "Toggle road collision", ParallelRoadTool.collision);

            if((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                m_grid = CreateCheckBox(this, "Grid", "Toggle editor grid", true);
            }

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
            button.relativePosition = new Vector2(0, 0);

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
            ParallelRoadTool.bending = m_bending.isChecked;
            ParallelRoadTool.snapping = m_snapping.isChecked;
            ParallelRoadTool.collision = m_collision.isChecked;

            if(m_grid != null)
            {
                ParallelRoadTool.grid = m_grid.isChecked;
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
