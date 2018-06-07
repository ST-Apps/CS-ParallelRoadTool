using System;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIMainWindow : UIPanel
    {
        private static readonly SavedInt SavedWindowX =
            new SavedInt("windowX", ParallelRoadTool.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedWindowY =
            new SavedInt("windowY", ParallelRoadTool.SettingsFileName, -1000, true);

        private UIOptionsPanel _mainPanel;
        private UINetList _netList;
        private NetInfo _netToolSelection;
        private UICheckBox _toolToggleButton;
        private UICheckBox _snappingToggleButton;

        #region Events/Callbacks

        public event PropertyChangedEventHandler<bool> OnParallelToolToggled;
        public event EventHandler OnNetworksListCountChanged;
        public event PropertyChangedEventHandler<bool> OnSnappingToggled;

        private void UnsubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged -= ToolToggleButtonOnEventCheckChanged;
            _mainPanel.OnToolToggled -= ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged -= SnappingToggleButtonOnEventCheckChanged;
        }

        private void SubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged += ToolToggleButtonOnEventCheckChanged;
            _mainPanel.OnToolToggled += ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged += SnappingToggleButtonOnEventCheckChanged;
        }

        private void SnappingToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            DebugUtils.Log("Snapping toggle pressed.");
            OnSnappingToggled?.Invoke(component, value);
        }

        private void ToolToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            DebugUtils.Log("Tool toggle pressed.");
            OnParallelToolToggled?.Invoke(component, value);
        }

        private void NetListOnChangedCallback()
        {
            DebugUtils.Log($"_netList.OnChangedCallback (selected {ParallelRoadTool.SelectedRoadTypes.Count} nets)");
            OnNetworksListCountChanged?.Invoke(_netList, null);
        }

        #endregion

        #region Control

        public void RenderNetList()
        {
            _netList.RenderList();
        }

        public void ToggleToolCheckbox()
        {
            _toolToggleButton.isChecked = !_toolToggleButton.isChecked;
            OnParallelToolToggled?.Invoke(_toolToggleButton, _toolToggleButton.isChecked);
        }

        #endregion

        #region Unity

        public override void Start()
        {
            name = "PRT_MainWindow";
            isVisible = false;
            size = new Vector2(500, 240);
            autoFitChildrenVertically = true;
            absolutePosition = new Vector3(SavedWindowX.value, SavedWindowY.value);

            var bg = AddUIComponent<UIPanel>();
            bg.atlas = ResourceLoader.GetAtlas("Ingame");
            bg.backgroundSprite = "SubcategoriesPanel";
            bg.size = size;
            bg.padding = new RectOffset(8, 8, 8, 8);
            bg.autoLayoutPadding = new RectOffset(0, 0, 0, 4);
            bg.autoLayout = true;
            bg.autoLayoutDirection = LayoutDirection.Vertical;
            bg.autoFitChildrenVertically = true;

            var label = bg.AddUIComponent<UILabel>();
            label.name = "PRT_TitleLabel";
            label.textScale = 0.9f;
            label.text = "Parallel Road Tool";
            label.autoSize = false;
            label.width = 500;
            label.SendToBack();

            var dragHandle = label.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = label.size;

            _mainPanel = bg.AddUIComponent(typeof(UIOptionsPanel)) as UIOptionsPanel;
            _netList = bg.AddUIComponent(typeof(UINetList)) as UINetList;
            if (_netList != null)
            {
                _netList.List = ParallelRoadTool.SelectedRoadTypes;
                _netList.OnChangedCallback = NetListOnChangedCallback;
            }

            var space = bg.AddUIComponent<UIPanel>();
            space.size = new Vector2(1, 1);

            // Add options
            _snappingToggleButton = UIUtil.CreateCheckBox(_mainPanel, "Snapping", "Toggle snapping for all nodes", false);
            _snappingToggleButton.relativePosition = new Vector3(166, 38);
            _snappingToggleButton.BringToFront();

            // Add main tool button to road options panel
            if (_toolToggleButton != null) return;

            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            if (tsBar == null || !tsBar.gameObject.activeInHierarchy) return;
            var button = UIUtil.FindComponent<UICheckBox>("PRT_Parallel");
            if (button != null)
                Destroy(button);
            _toolToggleButton = UIUtil.CreateCheckBox(tsBar, "Parallel", "Parallel Road Tool", false);
            _toolToggleButton.relativePosition = new Vector3(424, -6);

            SubscribeToUIEvents();

            OnPositionChanged();
            DebugUtils.Log($"UIMainWindow created {size} | {position}");
        }

        public override void Update()
        {
            if (ParallelRoadTool.Instance != null)
                isVisible = ParallelRoadTool.Instance.IsToolActive;

            if (ParallelRoadTool.NetTool != null)
                _toolToggleButton.isVisible = ParallelRoadTool.NetTool.enabled;

            base.Update();
        }

        public override void OnDestroy()
        {
            UnsubscribeToUIEvents();
            base.OnDestroy();
        }

        protected override void OnPositionChanged()
        {
            var resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);

            absolutePosition = new Vector2(
                (int) Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int) Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            //DebugUtils.Log($"UIMainWindow OnPositionChanged | {resolution} | {absolutePosition}");

            SavedWindowX.value = (int) absolutePosition.x;
            SavedWindowY.value = (int) absolutePosition.y;
        }


        public void OnGUI()
        {
            var currentSelectedNetwork = ParallelRoadTool.NetTool.m_prefab;

            DebugUtils.Log($"Updating currentItem from {_netToolSelection?.name} to {currentSelectedNetwork?.name}");

            if (_netToolSelection == currentSelectedNetwork) return;
            _netToolSelection = currentSelectedNetwork;
            _netList.UpdateCurrentTool(currentSelectedNetwork);
        }

        #endregion
    }
}