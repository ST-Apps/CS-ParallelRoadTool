using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using System;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIMainWindow : UIPanel
    {
        #region Settings

        private static readonly SavedInt SavedWindowX =
            new SavedInt("windowX", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedWindowY =
            new SavedInt("windowY", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedToggleX =
            new SavedInt("toggleX", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedToggleY =
            new SavedInt("toggleY", Configuration.SettingsFileName, -1000, true);

        #endregion

        #region Properties

        #region Data

        // We use this to prevent clicks while user is dragging the button
        private bool _isDragging;

        // We use this to prevent handling events while the advisor is being updated
        private readonly bool _isUpdatingTutorialAdvisor;

        #endregion

        #region UI

        private UIRightDragHandle _buttonDragHandle;
        private UIOptionsPanel _mainPanel;
        private UINetList _netList;
        private UICheckBox _toolToggleButton;
        private UICheckBox _snappingToggleButton;
        // private UICheckBox _tutorialToggleButton;
        private UIButton _loadPresetsButton;
        private UIButton _savePresetsButton;

        private UISprite _tutorialIcon => ToolsModifierControl.advisorPanel?.Find<UISprite>("Icon");
        private UISprite _tutorialImage => ToolsModifierControl.advisorPanel?.Find<UISprite>("Sprite");

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

        private void UnsubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged -= ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged -= SnappingToggleButtonOnEventCheckChanged;
            //_tutorialToggleButton.eventCheckChanged -= _tutorialToggleButton_eventCheckChanged;
            _buttonDragHandle.eventDragStart -= ButtonDragHandleOnEventDragStart;
            _buttonDragHandle.eventDragEnd -= ButtonDragHandleOnEventDragEnd;
            _netList.OnItemChanged -= NetListOnOnItemChanged;
            _netList.OnItemAdded -= NetListOnOnItemAdded;
            _netList.OnItemDeleted -= NetListOnOnItemDeleted;
            _loadPresetsButton.eventClicked -= LoadPresetsButtonOnClicked;
            _savePresetsButton.eventClicked -= SavePresetsButtonOnClicked;
        }

        private void SubscribeToUIEvents()
        {
            _toolToggleButton.eventCheckChanged += ToolToggleButtonOnEventCheckChanged;
            _snappingToggleButton.eventCheckChanged += SnappingToggleButtonOnEventCheckChanged;
            //_tutorialToggleButton.eventCheckChanged += _tutorialToggleButton_eventCheckChanged;
            _buttonDragHandle.eventDragStart += ButtonDragHandleOnEventDragStart;
            _buttonDragHandle.eventDragEnd += ButtonDragHandleOnEventDragEnd;
            _netList.OnItemChanged += NetListOnOnItemChanged;
            _netList.OnItemAdded += NetListOnOnItemAdded;
            _netList.OnItemDeleted += NetListOnOnItemDeleted;
            _loadPresetsButton.eventClicked += LoadPresetsButtonOnClicked;
            _savePresetsButton.eventClicked += SavePresetsButtonOnClicked;
        }

        private void NetListOnOnItemAdded(object sender, EventArgs eventArgs)
        {
            DebugUtils.Log($"{nameof(NetListOnOnItemAdded)}");
            OnNetworkItemAdded?.Invoke(this, null);
        }

        private void NetListOnOnItemChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            OnItemChanged?.Invoke(this, value);
        }

        private void NetListOnOnItemDeleted(UIComponent component, int index)
        {
            OnNetworkItemDeleted?.Invoke(this, index);
        }

        private void TutorialButtonToggleOnEventChanged(UIComponent component, bool value)
        {
            if (_isUpdatingTutorialAdvisor)
            {
                return;
            }

            DebugUtils.Log($"_tutorialToggleButton_eventCheckChanged: {value}");
            if (value)
            {
                ToolsModifierControl.advisorPanel.Show("ParallelRoadTool", "Parallel", "Tutorial", 0.0f);
            }
            else
            {
                ToolsModifierControl.advisorPanel.Hide();
            }
        }

        private void ButtonDragHandleOnEventDragStart(UIComponent component, UIDragEventParameter eventparam)
        {
            _isDragging = true;
        }

        private void ButtonDragHandleOnEventDragEnd(UIComponent component, UIDragEventParameter eventparam)
        {
            _isDragging = false;

            // Also save position
            SavedToggleX.value = (int)_toolToggleButton.absolutePosition.x;
            SavedToggleY.value = (int)_toolToggleButton.absolutePosition.y;
        }

        private void SnappingToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            DebugUtils.Log("Snapping toggle pressed.");
            OnSnappingToggled?.Invoke(component, value);
        }

        private void ToolToggleButtonOnEventCheckChanged(UIComponent component, bool value)
        {
            // Prevent click during dragging
            if (_isDragging)
            {
                return;
            }

            DebugUtils.Log("Tool toggle pressed.");
            OnParallelToolToggled?.Invoke(component, value);
        }

        private void LoadPresetsButtonOnClicked(UIComponent component, UIMouseEventParameter parameter)
        {
            UILoadWindow.Open();
        }

        private void SavePresetsButtonOnClicked(UIComponent component, UIMouseEventParameter parameter)
        {
            UISaveWindow.Open();
        }

        #endregion

        #region Control

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

        public void ShowTutorial()
        {
            TutorialButtonToggleOnEventChanged(null, true);
        }

        #endregion

        #region Utility

        private void ToggleToolCheckbox()
        {
            _toolToggleButton.isChecked = !_toolToggleButton.isChecked;
            OnParallelToolToggled?.Invoke(_toolToggleButton, _toolToggleButton.isChecked);
        }

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            if (isHorizontal)
            {
                OnHorizontalOffsetKeypress?.Invoke(this, step);
            }
            else
            {
                OnVerticalOffsetKeypress?.Invoke(this, step);
            }
        }

        public void ClearItems()
        {
            _netList.ClearItems();
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

            var label = bg.AddUIComponent<UILabel>();
            label.name = $"{Configuration.ResourcePrefix}TitleLabel";
            label.textScale = 0.9f;
            label.text = ModInfo.ModName;
            label.autoSize = false;
            label.width = 500;
            label.SendToBack();

            var dragHandle = label.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = label.size;

            _mainPanel = bg.AddUIComponent(typeof(UIOptionsPanel)) as UIOptionsPanel;
            _netList = bg.AddUIComponent(typeof(UINetList)) as UINetList;

            var space = bg.AddUIComponent<UIPanel>();
            space.size = new Vector2(1, 1);

            // Add options
            _snappingToggleButton = UIUtil.CreateCheckBox(_mainPanel, "Snapping",
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SnappingToggleButton"), false);
            _snappingToggleButton.relativePosition = new Vector3(166, 38);
            _snappingToggleButton.BringToFront();

            //_tutorialToggleButton = UIUtil.CreateCheckBox(_mainPanel, "ToolbarIconHelp",
            //    Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "TutorialToggleButton"), false, true);
            //_tutorialToggleButton.relativePosition = new Vector3(166, 38);
            //_tutorialToggleButton.BringToFront();
            //_tutorialToggleButton.isVisible = ParallelRoadTool.IsInGameMode;

            _loadPresetsButton = UIUtil.CreateUiButton(_mainPanel, string.Empty, Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "LoadButton"), new Vector2(36, 36), "Load");
            _loadPresetsButton.relativePosition = new Vector3(166, 38);
            _loadPresetsButton.BringToFront();
            _savePresetsButton = UIUtil.CreateUiButton(_mainPanel, string.Empty, Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SaveButton"), new Vector2(36, 36), "Save");
            _savePresetsButton.relativePosition = new Vector3(166, 38);
            _savePresetsButton.BringToFront();

            //TODO: Needs button state based on networks count 

            // Add main tool button to road options panel
            if (_toolToggleButton != null)
            {
                return;
            }

            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            if (tsBar == null || !tsBar.gameObject.activeInHierarchy)
            {
                return;
            }

            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);
            if (toolModeBar == null)
            {
                return;
            }

            var button = UIUtil.FindComponent<UICheckBox>($"{Configuration.ResourcePrefix}Parallel");
            if (button != null)
            {
                Destroy(button);
            }

            _toolToggleButton = UIUtil.CreateCheckBox(tsBar, "Parallel",
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ToolToggleButton"), false);
            if (SavedToggleX.value != -1000 && SavedToggleY.value != -1000)
            {
                _toolToggleButton.absolutePosition = new Vector3(SavedToggleX.value, SavedToggleY.value);
            }
            else
            {
                _toolToggleButton.absolutePosition =
                    new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1,
                        toolModeBar.absolutePosition.y);
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
            isVisible = Singleton<ParallelRoadTool>.exists && Singleton<ParallelRoadTool>.instance.IsToolActive;
            _toolToggleButton.isVisible = ToolsModifierControl.GetTool<NetTool>() != null && ToolsModifierControl.GetTool<NetTool>().enabled;

            // TODO: let's see if disabling tutorial helps with performances as I'm getting mixed reports and can't reproduce the issue
            if (!isVisible)
            {
                return;
            }

            // HACK - Adding textures to default atlas fails and TutorialAdvisor only uses default atlas, so we need to update the selected atlas based on the tutorial we're showing.
            //if (_tutorialIcon == null || !ToolsModifierControl.advisorPanel.isVisible || !ToolsModifierControl.advisorPanel.isOpen) return;
            //_isUpdatingTutorialAdvisor = true;
            //if (_tutorialIcon.spriteName == "Parallel")
            //{
            //    if (_tutorialImage.atlas.name != UIUtil.TextureAtlas.name)
            //        _tutorialIcon.atlas = _tutorialImage.atlas = UIUtil.TextureAtlas;
            //}
            //else
            //{
            //    if (_tutorialImage.atlas.name != UIUtil.AdvisorAtlas.name)
            //    {
            //        _tutorialIcon.atlas = UIUtil.DefaultAtlas;
            //        _tutorialImage.atlas = UIUtil.AdvisorAtlas;
            //    }
            //}

            //_tutorialToggleButton.isChecked = _tutorialIcon.spriteName == "Parallel"
            //                                  && _tutorialImage.atlas.name == UIUtil.TextureAtlas.name
            //                                  && ToolsModifierControl.advisorPanel.isVisible
            //                                  && ToolsModifierControl.advisorPanel.isOpen;
            //_isUpdatingTutorialAdvisor = false;

            base.Update();
        }

        public override void OnDestroy()
        {
            try
            {
                UnsubscribeToUIEvents();

                Destroy(_buttonDragHandle);
                Destroy(_mainPanel);
                Destroy(_netList);
                Destroy(_toolToggleButton);
                Destroy(_snappingToggleButton);
                //Destroy(_tutorialToggleButton);
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

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
            }

            absolutePosition = new Vector2(
                (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

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
            if (UIView.HasModalInput()
                || UIView.HasInputFocus()
                || !Singleton<ParallelRoadTool>.exists
                || !Singleton<ParallelRoadTool>.instance.IsToolActive)
            {
                return;
            }

            var e = Event.current;
            // Checking key presses
            if (OptionsKeymapping.toggleParallelRoads.IsPressed(e))
            {
                ToggleToolCheckbox();
            }

            if (OptionsKeymapping.decreaseHorizontalOffset.IsPressed(e))
            {
                AdjustNetOffset(-1f);
            }

            if (OptionsKeymapping.increaseHorizontalOffset.IsPressed(e))
            {
                AdjustNetOffset(1f);
            }

            if (OptionsKeymapping.decreaseVerticalOffset.IsPressed(e))
            {
                AdjustNetOffset(-1f, false);
            }

            if (OptionsKeymapping.increaseVerticalOffset.IsPressed(e))
            {
                AdjustNetOffset(1f, false);
            }
        }

        #endregion
    }
}