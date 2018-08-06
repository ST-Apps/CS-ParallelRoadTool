using System;
using System.Globalization;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UINetTypeItem : UIPanel
    {
        #region Constants

        private const int TextFieldWidth = 65;
        private const int LabelWidth = 250;
        private const float ColumnPadding = 8f;
        private const int ReverseButtonWidth = 36;

        #endregion

        #region Properties

        #region Data

        // We need to know item's index for faster delete
        public int Index;
        public NetInfo NetInfo;
        public float HorizontalOffset;
        public float VerticalOffset;
        public bool IsReversed;
        public bool IsCurrentItem;

        private bool _populated;
        private bool _canFireChangedEvent;

        #endregion

        #region UI

        private UILabel _label;
        private UIDropDown _dropDown;
        private UICheckBox _reverseCheckbox;
        private UITextField _horizontalOffsetField;
        private UITextField _verticalOffsetField;
        private UIButton _deleteButton;
        private UIButton _addButton;

        #endregion

        #endregion

        #region Events/Callbacks

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnChanged;
        public event EventHandler OnAddClicked;
        public event PropertyChangedEventHandler<int> OnDeleteClicked;

        #endregion

        #region Unity

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}NetTypeItem";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            color = new Color32(255, 255, 255, 255);
            size = new Vector2(500 - 8 * 2 - 4 * 2, 40);

            var panel = AddUIComponent<UIPanel>();
            panel.size = new Vector2(LabelWidth, 40);
            panel.relativePosition = Vector2.zero;

            _dropDown = UIUtil.CreateDropDown(panel);
            _dropDown.width = LabelWidth;
            _dropDown.relativePosition = Vector2.zero;

            _reverseCheckbox = UIUtil.CreateCheckBox(this, "Reverse", Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ReverseToggleButton"), false);
            _reverseCheckbox.relativePosition = new Vector3(LabelWidth + ColumnPadding, 2);

            _horizontalOffsetField = UIUtil.CreateTextField(this);
            _horizontalOffsetField.relativePosition =
                new Vector3(LabelWidth + 2 * ColumnPadding + ReverseButtonWidth, 10);
            _horizontalOffsetField.width = TextFieldWidth;

            _verticalOffsetField = UIUtil.CreateTextField(this);
            _verticalOffsetField.relativePosition =
                new Vector3(LabelWidth + 3 * ColumnPadding + ReverseButtonWidth + TextFieldWidth, 10);
            _verticalOffsetField.width = TextFieldWidth;

            _label = AddUIComponent<UILabel>();
            _label.textScale = .8f;
            _label.text = "Select a network";
            _label.autoSize = false;
            _label.width = LabelWidth;
            _label.relativePosition = new Vector3(10, 12);
            _label.isVisible = false;

            _deleteButton = UIUtil.CreateUiButton(this, string.Empty, Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "RemoveNetworkButton"), new Vector2(36, 36), "Remove");
            _deleteButton.zOrder = 0;
            _deleteButton.textScale = 0.8f;
            _deleteButton.relativePosition =
                new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            _addButton = UIUtil.CreateUiButton(this, string.Empty, Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "AddNetworkButton"), new Vector2(36, 36), "Add");
            _addButton.zOrder = 1;
            _addButton.isVisible = false;
            _addButton.textScale = 0.8f;
            _addButton.relativePosition =
                new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            SubscribeToUiEvents();

            UpdateItem();
        }

        public override void OnDestroy()
        {
            UnsubscribeToUiEvents();

            Destroy(_label);
            Destroy(_dropDown);
            Destroy(_reverseCheckbox);
            Destroy(_horizontalOffsetField);
            Destroy(_verticalOffsetField);
            Destroy(_deleteButton);
            Destroy(_addButton);
            base.OnDestroy();
        }

        #endregion

        #region Control

        public void UpdateItem()
        {
            _canFireChangedEvent = false;
            if (!_populated) PopulateDropdown();

            if (!IsCurrentItem)
            {
                _horizontalOffsetField.text = HorizontalOffset.ToString(CultureInfo.InvariantCulture);
                _verticalOffsetField.text = VerticalOffset.ToString(CultureInfo.InvariantCulture);
                _reverseCheckbox.isChecked = IsReversed;
                _dropDown.selectedIndex = Singleton<ParallelRoadTool>.instance.AvailableRoadTypes
                    .FindIndex(ni => ni != null && ni.name == NetInfo.name);
            }
            else
            {
                _dropDown.selectedIndex = 0;
                _deleteButton.isVisible =
                    _horizontalOffsetField.isVisible =
                        _verticalOffsetField.isVisible =
                            _reverseCheckbox.isVisible =
                                _dropDown.isVisible = false;
                _label.isVisible = _addButton.isVisible = true;
                _label.text = Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "SameAsSelectedLabel");
            }

            _canFireChangedEvent = true;
        }

        #endregion        

        #region Handlers

        private void UnsubscribeToUiEvents()
        {
            if (!IsCurrentItem)
            {
                _dropDown.eventSelectedIndexChanged -= DropDown_eventSelectedIndexChanged;
                _reverseCheckbox.eventCheckChanged -= ReverseCheckboxOnEventCheckChanged;
                _horizontalOffsetField.eventTextSubmitted -= HorizontalOffsetField_eventTextSubmitted;
                _verticalOffsetField.eventTextSubmitted -= VerticalOffsetField_eventTextSubmitted;
                _deleteButton.eventClicked -= DeleteButton_eventClicked;
            }
            else
                _addButton.eventClicked -= AddButton_eventClicked;
        }

        private void SubscribeToUiEvents()
        {
            if (!IsCurrentItem)
            {
                _dropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;
                _reverseCheckbox.eventCheckChanged += ReverseCheckboxOnEventCheckChanged;
                _horizontalOffsetField.eventTextSubmitted += HorizontalOffsetField_eventTextSubmitted;
                _verticalOffsetField.eventTextSubmitted += VerticalOffsetField_eventTextSubmitted;
                _deleteButton.eventClicked += DeleteButton_eventClicked;
            }
            else
                _addButton.eventClicked += AddButton_eventClicked;
        }

        private void DropDown_eventSelectedIndexChanged(UIComponent component, int index)
        {
            DebugUtils.Log($"{nameof(DropDown_eventSelectedIndexChanged)}");
            FireChangedEvent();
        }

        private void HorizontalOffsetField_eventTextSubmitted(UIComponent component, string value)
        {
            DebugUtils.Log($"{nameof(HorizontalOffsetField_eventTextSubmitted)}");
            FireChangedEvent();
        }

        private void VerticalOffsetField_eventTextSubmitted(UIComponent component, string value)
        {
            DebugUtils.Log($"{nameof(VerticalOffsetField_eventTextSubmitted)}");
            FireChangedEvent();
        }

        private void ReverseCheckboxOnEventCheckChanged(UIComponent component, bool value)
        {
            DebugUtils.Log($"{nameof(ReverseCheckboxOnEventCheckChanged)}");
            FireChangedEvent();
        }

        private void AddButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            DebugUtils.Log($"{nameof(AddButton_eventClicked)}");
            OnAddClicked?.Invoke(this, null);
        }

        private void DeleteButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            OnDeleteClicked?.Invoke(this, Index);
        }

        private void FireChangedEvent()
        {
            if (!_canFireChangedEvent) return;
            if (!float.TryParse(_horizontalOffsetField.text, out HorizontalOffset)) return;
            if (!float.TryParse(_verticalOffsetField.text, out VerticalOffset)) return;
            IsReversed = _reverseCheckbox.isChecked;

            var eventArgs = new NetTypeItemEventArgs(Index, HorizontalOffset, VerticalOffset, _dropDown.selectedIndex, IsReversed);
            OnChanged?.Invoke(this, eventArgs);
        }

        #endregion        

        #region Utility

        private void PopulateDropdown()
        {
            _dropDown.items = IsCurrentItem
                    ? Singleton<ParallelRoadTool>.instance.AvailableRoadNames.Take(1).ToArray()
                    : Singleton<ParallelRoadTool>.instance.AvailableRoadNames;
            _dropDown.selectedIndex = 0;
            _populated = true;
        }

        #endregion        
    }
}