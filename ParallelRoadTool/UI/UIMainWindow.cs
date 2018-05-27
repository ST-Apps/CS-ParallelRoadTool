using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIMainWindow : UIPanel
    {
        private static readonly SavedInt savedWindowX =
            new SavedInt("windowX", ParallelRoadTool.SettingsFileName, -1000, true);

        private static readonly SavedInt savedWindowY =
            new SavedInt("windowY", ParallelRoadTool.SettingsFileName, -1000, true);

        private UIOptionsPanel _mainPanel;
        private UINetList _netList;
        private UICheckBox _toolToggleButton;

        public override void Start()
        {
            name = "PRT_MainWindow";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            isVisible = false;
            size = new Vector2(450, 280);
            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 0, 0, 4);

            var label = AddUIComponent<UILabel>();
            label.name = "PRT_TitleLabel";
            label.textScale = 0.9f;
            label.text = "Parallel Road Tool";
            label.autoSize = false;
            label.width = 450;
            label.SendToBack();

            var dragHandle = label.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = label.size;
 
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;

            absolutePosition = new Vector3(savedWindowX.value, savedWindowY.value);

            _mainPanel = AddUIComponent(typeof(UIOptionsPanel)) as UIOptionsPanel;
            _netList = AddUIComponent(typeof(UINetList)) as UINetList;
            if (_netList != null)
            {
                _netList.List = ParallelRoadTool.SelectedRoadTypes;
                _netList.OnChangedCallback = () =>
                {
                    DebugUtils.Log($"_netList.OnChangedCallback (selected {ParallelRoadTool.SelectedRoadTypes.Count} nets)");
                    // TODO: callback to ParallelRoadTool -> NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
                };
            }

            var space = AddUIComponent<UIPanel>();
            space.size = new Vector2(1, 1);

            // Add main tool button to road options panel
            if (_toolToggleButton != null) return;

            var roadsOptionPanel = UIUtil.FindComponent<UIComponent>("RoadsOptionPanel", null, UIUtil.FindOptions.NameContains);
            if (roadsOptionPanel == null || !roadsOptionPanel.gameObject.activeInHierarchy) return;
            var button = UIUtil.FindComponent<UICheckBox>("PRT_Parallel");
            if (button != null)
                Destroy(button);
            _toolToggleButton = UIUtil.CreateCheckBox(roadsOptionPanel, "Parallel", "Parallel Road Tool", false);
            _toolToggleButton.relativePosition = new Vector3(166, 38);

            DebugUtils.Log($"Assigning event to _toolToggleButton checkbox.");

            _toolToggleButton.eventCheckChanged += (component, value) =>
            {
                DebugUtils.Log("tool toggle changed");
                // TODO: callback to -> ParallelRoadTool.IsParallelEnabled = _toolToggleButton.isChecked;
            };

            OnPositionChanged();
            DebugUtils.Log($"UIMainWindow created {size} | {position}");
        }

        public override void Update()
        {
            if (ParallelRoadTool.Instance != null)
                isVisible = ParallelRoadTool.Instance.IsToolActive();
       
            base.Update();
        }
 
        protected override void OnPositionChanged()
        {            
            var resolution = GetUIView().GetScreenResolution();            

            if (absolutePosition.x == -1000)
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);

            absolutePosition = new Vector2(
                (int) Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int) Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            DebugUtils.Log($"UIMainWindow OnPositionChanged | {resolution} | {absolutePosition}");

            savedWindowX.value = (int) absolutePosition.x;
            savedWindowY.value = (int) absolutePosition.y;
        }
 
    }
}