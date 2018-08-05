using System;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIMainWindow : UIPanel
    {
        private static readonly SavedInt SavedWindowX =
            new SavedInt("windowX", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedWindowY =
            new SavedInt("windowY", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedToggleX =
            new SavedInt("toggleX", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedToggleY =
            new SavedInt("toggleY", Configuration.SettingsFileName, -1000, true);

        private UIOptionsPanel _mainPanel;
        private UINetList _netList;
        private NetInfo _netToolSelection;
        private UIRightDragHandle _buttonDragHandle;

        private UICheckBox _toolToggleButton;
        private UICheckBox _snappingToggleButton;
        private UICheckBox _tutorialToggleButton;

        private UISprite _tutorialIcon
        {
            get
            {
                return ToolsModifierControl.advisorPanel?.Find<UISprite>("Icon");
            }
        }
        private UISprite _tutorialImage
        {
            get
            {
                return ToolsModifierControl.advisorPanel?.Find<UISprite>("Sprite");
            }
        }

        // We use this to prevent clicks while user is dragging the button
        private bool _isDragging;

        // We use this to prevent handling events while the advisor is being updated
        private bool _isUpdatingTutorialAdvisor;

        #region Events/Callbacks

        public event PropertyChangedEventHandler<bool> OnParallelToolToggled;        
        public event PropertyChangedEventHandler<bool> OnSnappingToggled;
        public event PropertyChangedEventHandler<float> OnHorizontalOffsetKeypress;
        public event PropertyChangedEventHandler<float> OnVerticalOffsetKeypress;
        public event EventHandler OnNetworksListCountChanged;

        private void UnsubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged -= ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged -= SnappingToggleButtonOnEventCheckChanged;
            _tutorialToggleButton.eventCheckChanged -= _tutorialToggleButton_eventCheckChanged;
            _buttonDragHandle.eventDragStart -= ButtonDragHandleOnEventDragStart;
            _buttonDragHandle.eventDragEnd -= ButtonDragHandleOnEventDragEnd;
        }

        private void SubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged += ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged += SnappingToggleButtonOnEventCheckChanged;
            _tutorialToggleButton.eventCheckChanged += _tutorialToggleButton_eventCheckChanged;
            _buttonDragHandle.eventDragStart += ButtonDragHandleOnEventDragStart;
            _buttonDragHandle.eventDragEnd += ButtonDragHandleOnEventDragEnd;
        }

        private void _tutorialToggleButton_eventCheckChanged(UIComponent component, bool value)
        {
            if (_isUpdatingTutorialAdvisor) return;
            DebugUtils.Log($"_tutorialToggleButton_eventCheckChanged: {value}");
            if (value)
                ToolsModifierControl.advisorPanel.Show("ParallelRoadTool", "Parallel", "Tutorial", 0.0f);
            else
                ToolsModifierControl.advisorPanel.Hide();
        }

        private void ButtonDragHandleOnEventDragEnd(UIComponent component, UIDragEventParameter eventparam)
        {
            _isDragging = false;

            // Also save position
            SavedToggleX.value = (int)_toolToggleButton.absolutePosition.x;
            SavedToggleY.value = (int)_toolToggleButton.absolutePosition.y;
        }

        private void ButtonDragHandleOnEventDragStart(UIComponent component, UIDragEventParameter eventparam)
        {
            _isDragging = true;
        }

        private void SnappingToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            DebugUtils.Log("Snapping toggle pressed.");
            OnSnappingToggled?.Invoke(component, value);
        }

        private void ToolToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            // Prevent click during dragging
            if (_isDragging) return;

            DebugUtils.Log("Tool toggle pressed.");
            OnParallelToolToggled?.Invoke(component, value);
        }

        private void NetListOnChangedCallback()
        {
            DebugUtils.Log($"_netList.OnChangedCallback (selected {Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Count} nets)");
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

        public void ShowTutorial()
        {
            _tutorialToggleButton_eventCheckChanged(null, true);
        }

        #endregion

        #region Utility

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            if (isHorizontal)
                OnHorizontalOffsetKeypress?.Invoke(this, step);
            else
                OnVerticalOffsetKeypress?.Invoke(this, step);

            RenderNetList();
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
                _netList.List = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes;
                _netList.OnChangedCallback = NetListOnChangedCallback;
            }

            var space = bg.AddUIComponent<UIPanel>();
            space.size = new Vector2(1, 1);

            // Add options
            _snappingToggleButton = UIUtil.CreateCheckBox(_mainPanel, "Snapping", Locale.Get("PRT_TOOLTIPS", "SnappingToggleButton"), false);
            _snappingToggleButton.relativePosition = new Vector3(166, 38);
            _snappingToggleButton.BringToFront();

            _tutorialToggleButton = UIUtil.CreateCheckBox(_mainPanel, "ToolbarIconHelp", Locale.Get("PRT_TOOLTIPS", "TutorialToggleButton"), false, true);
            _tutorialToggleButton.relativePosition = new Vector3(166, 38);
            _tutorialToggleButton.BringToFront();
            _tutorialToggleButton.isVisible = ParallelRoadTool.IsInGameMode;

            // Add main tool button to road options panel
            if (_toolToggleButton != null) return;

            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            if (tsBar == null || !tsBar.gameObject.activeInHierarchy) return;

            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);
            if (toolModeBar == null) return;

            var button = UIUtil.FindComponent<UICheckBox>("PRT_Parallel");
            if (button != null)
                Destroy(button);

            _toolToggleButton = UIUtil.CreateCheckBox(tsBar, "Parallel", Locale.Get("PRT_TOOLTIPS", "ToolToggleButton"), false);
            if (SavedToggleX.value != -1000 && SavedToggleY.value != -1000)
            {
                _toolToggleButton.absolutePosition = new Vector3(SavedToggleX.value, SavedToggleY.value);
            }
            else
            {
                _toolToggleButton.absolutePosition = new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1, toolModeBar.absolutePosition.y);
            }

            // HACK - [ISSUE-26] Tool's main button must be draggable to prevent overlapping other mods buttons.
            _buttonDragHandle = _toolToggleButton.AddUIComponent<UIRightDragHandle>();
            _buttonDragHandle.size = _toolToggleButton.size;
            _buttonDragHandle.relativePosition = Vector3.zero;
            _buttonDragHandle.target = _toolToggleButton;

            SubscribeToUIEvents();

            OnPositionChanged();
            DebugUtils.Log($"UIMainWindow created {size} | {position}");
        }

        public override void Update()
        {
            if (Singleton<ParallelRoadTool>.exists)
                isVisible = Singleton<ParallelRoadTool>.instance.IsToolActive;

            if (ToolsModifierControl.GetTool<NetTool>() != null)
                _toolToggleButton.isVisible = ToolsModifierControl.GetTool<NetTool>().enabled;

            if (!Singleton<ParallelRoadTool>.instance.IsToolActive) return;

            // HACK - Adding textures to default atlas fails and TutorialAdvisor only uses default atlas, so we need to update the selected atlas based on the tutorial we're showing.
            if (_tutorialIcon == null) return;
            _isUpdatingTutorialAdvisor = true;
            DebugUtils.Log($"SpriteName: {_tutorialIcon.spriteName} | CustomAtlasName: {_tutorialImage.atlas.name} | IsChecked: { _tutorialToggleButton.isChecked}");
            if (_tutorialIcon.spriteName == "Parallel")
            {
                if (_tutorialImage.atlas.name != UIUtil.TextureAtlas.name)
                {                   
                    _tutorialIcon.atlas = _tutorialImage.atlas = UIUtil.TextureAtlas;
                }
            }
            else
            {
                if (_tutorialImage.atlas.name != UIUtil.AdvisorAtlas.name)
                {
                    _tutorialIcon.atlas = UIUtil.DefaultAtlas;
                    _tutorialImage.atlas = UIUtil.AdvisorAtlas;
                }
            }             
            _tutorialToggleButton.isChecked = _tutorialIcon.spriteName == "Parallel" 
                && _tutorialImage.atlas.name == UIUtil.TextureAtlas.name
                && ToolsModifierControl.advisorPanel.isVisible 
                && ToolsModifierControl.advisorPanel.isOpen;            
            _isUpdatingTutorialAdvisor = false;

            base.Update();
        }

        public override void OnDestroy()
        {
            try
            {
                UnsubscribeToUIEvents();
                base.OnDestroy();
            }
            catch
            {
                // HACK - [ISSUE-31]
            }
        }

        protected override void OnPositionChanged()
        {
            var resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);

            absolutePosition = new Vector2(
                (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            //DebugUtils.Log($"UIMainWindow OnPositionChanged | {resolution} | {absolutePosition}");

            // HACK - [ISSUE-9] Setting window's position seems not enough, we also need to set position for the first children of the window.
            var firstChildren = m_ChildComponents.FirstOrDefault();
            if (firstChildren != null)
            {
                firstChildren.absolutePosition = absolutePosition;
            }

            SavedWindowX.value = (int)absolutePosition.x;
            SavedWindowY.value = (int)absolutePosition.y;
        }

        public void OnGUI()
        {
            if (UIView.HasModalInput() || UIView.HasInputFocus()) return;
            
            var e = Event.current;

            // Checking key presses
            if (OptionsKeymapping.toggleParallelRoads.IsPressed(e)) ToggleToolCheckbox();
            if (OptionsKeymapping.decreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);
            if (OptionsKeymapping.increaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);
            if (OptionsKeymapping.decreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);
            if (OptionsKeymapping.increaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);

            if (!Singleton<ParallelRoadTool>.instance.IsToolActive) return;

            var currentSelectedNetwork = ToolsModifierControl.GetTool<NetTool>().m_prefab;
            if (_netToolSelection == currentSelectedNetwork) return;
            DebugUtils.Log($"Updating currentItem from {_netToolSelection?.name} to {currentSelectedNetwork?.name}");
            _netToolSelection = currentSelectedNetwork;
            _netList.UpdateCurrentTool(currentSelectedNetwork);
        }

        #endregion
    }
}