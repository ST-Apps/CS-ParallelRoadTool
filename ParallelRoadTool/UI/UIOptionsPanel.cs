using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIOptionsPanel : UIPanel
    {
        public UICheckBox ToolToggleButton;

        public override void Start()
        {
            name = "PRT_OptionsPanel";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "GenericPanel";
            color = new Color32(206, 206, 206, 255);
            size = new Vector2(450 - 8 * 2, 36 + 2 * 8);

            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayout = true;
            autoSize = false;

            ToolToggleButton = UIUtil.CreateCheckBox(this, "Parallel", "Toggle parallel road tool", false);
            ToolToggleButton.eventCheckChanged += (component, value) =>
            {
                UpdateOptions();
            };
            //AddCheckBox("Anarchy", "Toggle parallel road tool", false);
            //AddCheckBox("Anarchy", "Toggle parallel road tool", false);

            DebugUtils.Log($"UIOptionsPanel created {size} | {position}");

            UpdateOptions();
        }

        public override void Update()
        {
            /*var sel = ParallelRoadTool.instance.NetTool.m_prefab;
            if (netToolSelection != sel)
            {
                netToolSelection = sel;
                m_nettoolSelection.text = netToolSelection.GenerateBeautifiedNetName();
                //m_nettoolSelection.
                //AddMoreNetworks.relativePosition = new Vector2(-20, m_nettoolSelection.width);
            }
            size = new Vector2(400 + 4 * 8, 80 + 40 * m_networks.Count);
            base.Update();
            */
        }

        private void UpdateOptions()
        {
            DebugUtils.Log("UIOptionsPanel.UpdateOptions()");

            ParallelRoadTool.IsParallelEnabled = ToolToggleButton.isChecked;

        }
    }
}