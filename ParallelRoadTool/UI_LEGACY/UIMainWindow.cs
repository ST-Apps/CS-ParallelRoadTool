using System;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI_LEGACY
{
    public class UIMainWindow : UIPanel
    {
        #region Settings

        private static readonly SavedInt SavedWindowX =
            new SavedInt("windowX", Configuration.SettingsFileName, 0, true);

        private static readonly SavedInt SavedWindowY =
            new SavedInt("windowY", Configuration.SettingsFileName, 0, true);

        private static readonly SavedInt SavedToggleX =
            new SavedInt("toggleX", Configuration.SettingsFileName, 0, true);

        private static readonly SavedInt SavedToggleY =
            new SavedInt("toggleY", Configuration.SettingsFileName, 0, true);

        #endregion

        #region Properties

        #region Data

        // We use this to prevent clicks while user is dragging the button
        private bool _isDragging;

        /// <summary>
        ///     Index of the NetTypeItem that we're currently filtering.
        /// </summary>
        private int _filteredItemIndex = -1;

        /// <summary>
        ///     Current tool that is being used.
        /// </summary>
        private ToolBase _currentTool;

        #endregion

        #region UI

        private UIRightDragHandle _buttonDragHandle;
        private UIOptionsPanel _mainPanel;
        private UINetList _netList;
        private UICheckBox _toolToggleButton;

        private UICheckBox _snappingToggleButton;

        private UITextField _dropdownFilterField;
        private UIButton _loadPresetsButton;
        private UIButton _savePresetsButton;
        private UIButton _closeButton;

        #endregion

        #endregion

        #region Events/Callbacks

        public event PropertyChangedEventHandler<bool> OnParallelToolToggled;
        public event PropertyChangedEventHandler<bool> OnSnappingToggled;
        public event PropertyChangedEventHandler<float> OnHorizontalOffsetKeypress;
        public event PropertyChangedEventHandler<float> OnVerticalOffsetKeypress;

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnItemChanged;
        public event EventHandler OnNetworkItemAdded;
        public event PropertyChangedEventHandler<int> OnNetworkItemDeleted;

        public event PropertyChangedEventHandler<ToolBase> OnToolChanged;

        private void UnsubscribeFromUIEvents()
        {
            _toolToggleButton.eventCheckChanged -= ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged -= SnappingToggleButtonOnEventCheckChanged;

            _buttonDragHandle.eventDragStart -= ButtonDragHandleOnEventDragStart;
            _buttonDragHandle.eventDragEnd -= ButtonDragHandleOnEventDragEnd;
            _netList.OnItemChanged -= NetListOnOnItemChanged;
            _netList.OnItemAdded -= NetListOnOnItemAdded;
            _netList.OnItemDeleted -= NetListOnOnItemDeleted;
            _dropdownFilterField.eventTextChanged -= DropdownFilterFieldOnEventTextChanged;

            _loadPresetsButton.eventClicked -= LoadPresetsButtonOnClicked;
            _savePresetsButton.eventClicked -= SavePresetsButtonOnClicked;

            _closeButton.eventClicked -= CloseButtonOneventClicked;
        }

        private void SubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged += ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged += SnappingToggleButtonOnEventCheckChanged;

            _buttonDragHandle.eventDragStart += ButtonDragHandleOnEventDragStart;
            _buttonDragHandle.eventDragEnd += ButtonDragHandleOnEventDragEnd;
            _netList.OnItemChanged += NetListOnOnItemChanged;
            _netList.OnItemAdded += NetListOnOnItemAdded;
            _netList.OnItemDeleted += NetListOnOnItemDeleted;
            _netList.OnSearchModeToggled += NetListOnOnSearchModeToggled;
            _dropdownFilterField.eventTextChanged += DropdownFilterFieldOnEventTextChanged;

            _loadPresetsButton.eventClicked += LoadPresetsButtonOnClicked;
            _savePresetsButton.eventClicked += SavePresetsButtonOnClicked;

            _closeButton.eventClicked += CloseButtonOneventClicked;
        }

        private void CloseButtonOneventClicked(UIComponent component, UIMouseEventParameter eventparam)
        {
            ToggleToolCheckbox(true);
        }

        private void DropdownFilterFieldOnEventLostFocus(UIComponent component, UIFocusEventParameter eventparam)
        {
            _netList.DisableSearchMode(_filteredItemIndex);
        }

        private void DropdownFilterFieldOnEventTextChanged(UIComponent component, string value)
        {
            _netList.FilterItemDropdown(_filteredItemIndex, value);
        }

        private void NetListOnOnSearchModeToggled(UIComponent component, int value)
        {
            ToggleDropdownFiltering(value);
        }

        private void NetListOnOnItemAdded(object sender, EventArgs eventArgs)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(NetListOnOnItemAdded)}] Event triggered with eventArgs: {eventArgs}");

            OnNetworkItemAdded?.Invoke(this, null);
        }

        private void NetListOnOnItemChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(NetListOnOnItemChanged)}] Event triggered with value: {value}");

            OnItemChanged?.Invoke(this, value);
        }

        private void NetListOnOnItemDeleted(UIComponent component, int index)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(NetListOnOnItemDeleted)}] Event triggered with index: {index}");

            OnNetworkItemDeleted?.Invoke(this, index);
        }

        private void ButtonDragHandleOnEventDragStart(UIComponent component, UIDragEventParameter eventparam)
        {
            _isDragging = true;
        }

        private void ButtonDragHandleOnEventDragEnd(UIComponent component, UIDragEventParameter eventparam)
        {
            _isDragging = false;

            // Also save position
            SavedToggleX.value = (int) _toolToggleButton.absolutePosition.x;
            SavedToggleY.value = (int) _toolToggleButton.absolutePosition.y;
        }

        private void SnappingToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(SnappingToggleButtonOnEventCheckChanged)}] Event triggered with value: {value}");

            OnSnappingToggled?.Invoke(component, value);
        }

        private void ToolToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            // Prevent click during dragging
            if (_isDragging) return;

            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(ToolToggleButtonOnEventCheckChanged)}] Event triggered with value: {value}");

            OnParallelToolToggled?.Invoke(component, value);
        }

        private void LoadPresetsButtonOnClicked(UIComponent component, UIMouseEventParameter parameter)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(LoadPresetsButtonOnClicked)}] Event triggered with parameter: {parameter}");

            UILoadWindow.Open();
        }

        private void SavePresetsButtonOnClicked(UIComponent component, UIMouseEventParameter parameter)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(SavePresetsButtonOnClicked)}] Event triggered with parameter: {parameter}");

            UISaveWindow.Open();
        }

        private void ToolChanged(UIComponent component, ToolBase tool)
        {
            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(ToolChanged)}] Changed tool to {tool.GetType().Name}");

            OnToolChanged?.Invoke(null, tool);
        }

        #endregion

        #region Control

        public void ToggleToolButton(bool value)
        {
            _toolToggleButton.isVisible = value;
        }

        public void AddItem(NetTypeItem item)
        {
            _netList.AddItem(item);
        }

        public void UpdateItem(NetTypeItem item, int index)
        {
            _netList.UpdateItem(item, index);
        }

        public void DeleteItem(int index)
        {
            _netList.DeleteItem(index);
        }

        public void ToggleDropdownFiltering(int index)
        {
            Log.Info($"[{nameof(UIMainWindow)}.{nameof(ToggleDropdownFiltering)}] Toggling search mode for item with index {index} and current filtered item {_filteredItemIndex}");

            if (index == _filteredItemIndex)
            {
                // We need to disable filtering
                _filteredItemIndex = -1;
                _dropdownFilterField.isVisible = false;
            }
            else
            {
                if (_filteredItemIndex != -1)
                {
                    // We had filtering enabled for another item, so we need to disable it
                    _netList.DisableSearchMode(_filteredItemIndex);
                    _dropdownFilterField.text = string.Empty;
                }

                _filteredItemIndex = index;
                _dropdownFilterField.isVisible = true;
                _dropdownFilterField.Focus();
            }
        }

        public void ClearItems()
        {
            _netList.ClearItems();
        }

        public void UpdateDropdowns()
        {
            _netList.UpdateDropdowns();
        }

        public void ResetToolToggleButtonPosition()
        {
            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);

            _toolToggleButton.absolutePosition = new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1, toolModeBar.absolutePosition.y);
        }

        #endregion

        #region Utility

        private void ToggleToolCheckbox(bool forceClose = false)
        {
            if (forceClose)
            {
                _toolToggleButton.isChecked = false;
                OnParallelToolToggled?.Invoke(_toolToggleButton, _toolToggleButton.isChecked);
            }
            else
            {
                _toolToggleButton.isChecked = !_toolToggleButton.isChecked;
                OnParallelToolToggled?.Invoke(_toolToggleButton, _toolToggleButton.isChecked);
            }
        }

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            if (isHorizontal)
                OnHorizontalOffsetKeypress?.Invoke(this, step);
            else
                OnVerticalOffsetKeypress?.Invoke(this, step);
        }


        private void CheckToolStatus()
        {
            if (_currentTool == ToolsModifierControl.toolController.CurrentTool)
                return;

            _currentTool = ToolsModifierControl.toolController.CurrentTool;
            ToolChanged(null, _currentTool);
        }

        #endregion

        #region Unity

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}MainWindow";
            isVisible = false;
            size = new Vector2(500, 240);
            autoFitChildrenVertically = true;
            absolutePosition = new Vector3(SavedWindowX.value, SavedWindowY.value);

            var bg = AddUIComponent<UIPanel>();
            bg.atlas = UIUtil.DefaultAtlas;
            bg.backgroundSprite = "SubcategoriesPanel";
            bg.size = size;
            bg.padding = new RectOffset(8, 8, 8, 8);
            bg.autoLayoutPadding = new RectOffset(0, 0, 0, 4);
            bg.autoLayout = true;
            bg.autoLayoutDirection = LayoutDirection.Vertical;
            bg.autoFitChildrenVertically = true;

            var topPanel = bg.AddUIComponent<UIPanel>();
            topPanel.size = new Vector2(500, 28);
            topPanel.padding = new RectOffset(8, 8, 8, 8);

            var label = topPanel.AddUIComponent<UILabel>();
            label.name = $"{Configuration.ResourcePrefix}TitleLabel";
            label.text = ModInfo.ModName;
            label.relativePosition = Vector2.zero;
            label.SendToBack();

            _closeButton = topPanel.AddUIComponent<UIButton>();
            _closeButton.text = "";
            _closeButton.normalBgSprite = "buttonclose";
            _closeButton.hoveredBgSprite = "buttonclosehover";
            _closeButton.pressedBgSprite = "buttonclosepressed";
            _closeButton.size = new Vector2(32, 32);
            _closeButton.relativePosition = new Vector3(width - 44, -8);

            var dragHandle = topPanel.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = topPanel.size - new Vector2(60, 0);

            _mainPanel = bg.AddUIComponent(typeof(UIOptionsPanel)) as UIOptionsPanel;
            _netList = bg.AddUIComponent(typeof(UINetList)) as UINetList;

            var space = bg.AddUIComponent<UIPanel>();
            space.size = new Vector2(1, 1);

            // Add filter box
            _dropdownFilterField = UIUtil.CreateTextField(this);
            _dropdownFilterField.size = new Vector2(size.x - 160, 32);
            _dropdownFilterField.relativePosition = new Vector2(16, 38);
            _dropdownFilterField.isVisible = false;

            // Add options
            _snappingToggleButton = UIUtil.CreateCheckBox(_mainPanel, "Snapping",
                                                          Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SnappingToggleButton"), false);
            _snappingToggleButton.relativePosition = new Vector3(166, 38);
            _snappingToggleButton.BringToFront();

            _savePresetsButton = UIUtil.CreateUiButton(_mainPanel, string.Empty, Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SaveButton"),
                                                       new Vector2(36, 36), "Save");
            _savePresetsButton.relativePosition = new Vector3(166, 38);
            _savePresetsButton.BringToFront();
            _loadPresetsButton = UIUtil.CreateUiButton(_mainPanel, string.Empty, Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "LoadButton"),
                                                       new Vector2(36, 36), "Load");
            _loadPresetsButton.relativePosition = new Vector3(166, 38);
            _loadPresetsButton.BringToFront();

            // Add main tool button to road options panel
            if (_toolToggleButton == null)
            {
                DestroyImmediate(_toolToggleButton);
                _toolToggleButton = null;
            }

            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            if (tsBar == null || !tsBar.gameObject.activeInHierarchy) return;

            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);
            if (toolModeBar == null) return;

            var button = UIUtil.FindComponent<UICheckBox>($"{Configuration.ResourcePrefix}Parallel");
            if (button != null) DestroyImmediate(button);

            _toolToggleButton = UIUtil.CreateCheckBox(tsBar, "Parallel",
                                                      Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ToolToggleButton"), false);
            if (SavedToggleX.value != -1000 && SavedToggleY.value != -1000)
                _toolToggleButton.absolutePosition = new Vector3(SavedToggleX.value, SavedToggleY.value);
            else
                _toolToggleButton.absolutePosition =
                    new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1,
                                toolModeBar.absolutePosition.y);

            // HACK - [ISSUE-26] Tool's main button must be draggable to prevent overlapping other mods buttons.
            _buttonDragHandle = _toolToggleButton.AddUIComponent<UIRightDragHandle>();
            _buttonDragHandle.size = _toolToggleButton.size;
            _buttonDragHandle.relativePosition = Vector3.zero;
            _buttonDragHandle.target = _toolToggleButton;

            SubscribeToUIEvents();

            OnPositionChanged();

            Log.Info($"[{nameof(UIMainWindow)}.{nameof(Start)}] UIMainWindow created with size {size} and position {position}");
        }

        public override void OnDestroy()
        {
            try
            {
                UnsubscribeFromUIEvents();

                Destroy(_buttonDragHandle);
                Destroy(_mainPanel);
                Destroy(_netList);
                Destroy(_toolToggleButton);
                Destroy(_snappingToggleButton);

                Destroy(_loadPresetsButton);
                Destroy(_savePresetsButton);
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

            if (absolutePosition.x == -1000) absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);

            absolutePosition = new Vector2(
                                           (int) Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                                           (int) Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            // HACK - [ISSUE-9] Setting window's position seems not enough, we also need to set position for the first children of the window.
            var firstChildren = m_ChildComponents.FirstOrDefault();
            if (firstChildren != null) firstChildren.absolutePosition = absolutePosition;

            SavedWindowX.value = (int) absolutePosition.x;
            SavedWindowY.value = (int) absolutePosition.y;
        }

        public void OnGUI()
        {
            if (UIView.HasModalInput()
                || UIView.HasInputFocus()
                || !Singleton<ParallelRoadTool>.exists
                || !(ToolsModifierControl.toolController.CurrentTool is NetTool))
                return;

            var e = Event.current;

            if (e.isMouse)
            {
                // HACK - [ISSUE-84] Report if we're currently having a long mouse press
                Singleton<ParallelRoadTool>.instance.IsMouseLongPress = e.type switch
                {
                    EventType.MouseDown => true,
                    EventType.MouseUp   => false,
                    _                   => Singleton<ParallelRoadTool>.instance.IsMouseLongPress
                };

                Log._Debug($"[{nameof(UIMainWindow)}.{nameof(OnGUI)}] Settings {nameof(Singleton<ParallelRoadTool>.instance.IsMouseLongPress)} to {Singleton<ParallelRoadTool>.instance.IsMouseLongPress}");
            }

            // Checking key presses
            if (OptionsKeymapping.ToggleParallelRoads.IsPressed(e)) ToggleToolCheckbox();

            if (OptionsKeymapping.DecreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);

            if (OptionsKeymapping.IncreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);

            if (OptionsKeymapping.DecreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);

            if (OptionsKeymapping.IncreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);
        }

        public override void Update()
        {
            CheckToolStatus();
        }

        #endregion
    }
}
