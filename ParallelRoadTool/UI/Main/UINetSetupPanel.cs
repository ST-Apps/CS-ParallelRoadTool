using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace ParallelRoadTool.UI.Main;

/// <summary>
///     This panel is meant as a complete way to setup a parallel/stacked network.
///     It allows to set the network type, its offsets (both horizontal and vertical) and its direction.
/// </summary>
internal class UINetSetupPanel : UIPanel
{
    #region Fields

    /// <summary>
    ///     True means that all the editing tools must be disabled.
    /// </summary>
    private bool _isReadOnly;

    #endregion

    #region Properties

    /// <summary>
    ///     Current index for this item in a list.
    ///     While it's an ugly solution, this is more reliable than using <see cref="Transform.GetSiblingIndex" /> which
    ///     sometimes returns the wrong index after removing a child.
    /// </summary>
    public int CurrentIndex { get; set; }

    /// <summary>
    ///     True means that all the editing tools must be disabled.
    /// </summary>
    public bool IsReadOnly
    {
        set
        {
            _isReadOnly              = value;
            _netInfoPanel.IsReadOnly = value;
            color                    = UIConstants.ReadOnlyColor;

            HideTools();
        }
    }

    #endregion

    #region Events

    /// <summary>
    ///     Event triggered when the delete button is pressed.
    /// </summary>
    public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked;

    /// <summary>
    ///     Event triggered when any of the properties are changed.
    ///     This applies to:
    ///     <list type="bullet">
    ///         <item>Selected network</item>
    ///         <item>Horizontal offset</item>
    ///         <item>Vertical offset</item>
    ///         <item>Network direction</item>
    ///     </list>
    /// </summary>
    public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged;

    public event ChildComponentEventHandler OnPopupOpened
    {
        add => _netInfoPanel.OnPopupOpened += value;
        remove => _netInfoPanel.OnPopupOpened -= value;
    }

    public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnPopupSelectionChanged;

    #endregion

    #region Callbacks

    /// <summary>
    ///     Pressing the delete button triggers another event which sends <see cref="CurrentIndex" /> as a reference to the
    ///     panel that started it.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="eventParam"></param>
    private void DeleteButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        DeleteNetworkButtonEventClicked?.Invoke(this, CurrentIndex);
    }

    /// <summary>
    ///     If any of the properties for a <see cref="NetInfoItem" /> changes we trigger the event sending back all the up to
    ///     date properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <param name="value"></param>
    private void NetInfo_EventChanged<T>(UIComponent component, T value)
    {
        var netTypeArgs = new NetTypeItemEventArgs(CurrentIndex, float.Parse(_horizontalOffsetField.text), float.Parse(_verticalOffsetField.text),
                                                   _reverseCheckbox.isChecked);
        NetTypeEventChanged?.Invoke(null, netTypeArgs);
    }

    private void NetInfoPanelOnOnPopupSelectionChanged(UIComponent component, NetTypeItemEventArgs value)
    {
        value.ItemIndex = CurrentIndex;
        OnPopupSelectionChanged?.Invoke(component, value);
    }

    private void HorizontalOffsetFieldOnEventMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
    {
        UpdateOffsetOnScrollWheel(eventParam.wheelDelta);
    }

    private void VerticalOffsetFieldOnEventMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
    {
        UpdateOffsetOnScrollWheel(eventParam.wheelDelta, true);
    }

    #endregion

    #region Unity

    #region Components

    private UINetInfoButton _netInfoPanel;

    private UIPanel     _offsetsPanel;
    private UITextField _horizontalOffsetField;
    private UITextField _verticalOffsetField;

    private UIPanel    _buttonsPanel;
    private UICheckBox _reverseCheckbox;
    private UIButton   _deleteButton;

    #endregion

    #region Lifecycle

    public override void Awake()
    {
        base.Awake();

        // NetSetup
        name                      = $"{Constants.ResourcePrefix}NetSetup";
        backgroundSprite          = "GenericPanel";
        autoFitChildrenVertically = true;
        autoLayout                = true;
        autoLayoutDirection       = LayoutDirection.Horizontal;

        // NetSetup/NetInfo
        _netInfoPanel        = AddUIComponent<UINetInfoButton>();
        _netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

        // NetSetup/Offsets
        _offsetsPanel                             = AddUIComponent<UIPanel>();
        _offsetsPanel.name                        = $"{name}_Offsets";
        _offsetsPanel.autoLayout                  = true;
        _offsetsPanel.autoLayoutDirection         = LayoutDirection.Vertical;
        _offsetsPanel.anchor                      = UIAnchorStyle.CenterVertical;
        _offsetsPanel.autoFitChildrenHorizontally = true;
        _offsetsPanel.autoFitChildrenVertically   = true;

        // NetSetup/Offsets/Spacer
        _offsetsPanel.AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);

        // NetSetup/Offsets/Horizontal
        var horizontalOffsetPanel = _offsetsPanel.AddUIComponent<UIPanel>();
        horizontalOffsetPanel.autoFitChildrenHorizontally = true;
        horizontalOffsetPanel.autoFitChildrenVertically   = true;
        horizontalOffsetPanel.autoLayout                  = true;
        horizontalOffsetPanel.autoLayoutDirection         = LayoutDirection.Horizontal;

        // NetSetup/Offsets/Horizontal/Icon
        var horizontalOffsetIcon = horizontalOffsetPanel.AddUIComponent<UISprite>();
        horizontalOffsetIcon.size       = new Vector2(UIConstants.TinySize - 4, UIConstants.TinySize - 4);
        horizontalOffsetIcon.atlas      = UITextures.LoadSingleSpriteAtlas("HorizontalOffset");
        horizontalOffsetIcon.spriteName = "normal";

        // NetSetup/Offsets/Horizontal/Text
        _horizontalOffsetField = UITextFields.AddTextField(horizontalOffsetPanel, 0, 0, UIConstants.HugeSize, UIConstants.TinySize,
                                                           tooltip: Translations.Translate("TOOLTIP_HORIZONTAL_OFFSET_TEXT"));
        _horizontalOffsetField.numericalOnly = _horizontalOffsetField.allowFloats
                                                   = _horizontalOffsetField.allowNegative = _horizontalOffsetField.submitOnFocusLost = true;

        // NetSetup/Offsets/Vertical
        var verticalOffsetPanel = _offsetsPanel.AddUIComponent<UIPanel>();
        verticalOffsetPanel.autoFitChildrenHorizontally = true;
        verticalOffsetPanel.autoFitChildrenVertically   = true;
        verticalOffsetPanel.autoLayout                  = true;
        verticalOffsetPanel.autoLayoutDirection         = LayoutDirection.Horizontal;

        // NetSetup/Offsets/Vertical/Icon
        var verticalOffsetIcon = verticalOffsetPanel.AddUIComponent<UISprite>();
        verticalOffsetIcon.size       = new Vector2(UIConstants.TinySize - 4, UIConstants.TinySize - 4);
        verticalOffsetIcon.atlas      = UITextures.LoadSingleSpriteAtlas("VerticalOffset");
        verticalOffsetIcon.spriteName = "normal";

        // NetSetup/Offsets/Vertical/Text
        _verticalOffsetField = UITextFields.AddTextField(verticalOffsetPanel, 0, 0, UIConstants.HugeSize, UIConstants.TinySize,
                                                         tooltip: Translations.Translate("TOOLTIP_VERTICAL_OFFSET_TEXT"));
        _verticalOffsetField.maxLength = 3;
        _verticalOffsetField.numericalOnly = _verticalOffsetField.allowFloats
                                                 = _verticalOffsetField.allowNegative = _verticalOffsetField.submitOnFocusLost = true;

        // Manually align icons on offsets panel
        horizontalOffsetPanel.autoLayout      =  false;
        horizontalOffsetIcon.relativePosition += new Vector3(0, 2);
        verticalOffsetPanel.autoLayout        =  false;
        verticalOffsetIcon.relativePosition   += new Vector3(0, 2);

        // NetSetup/Buttons
        _buttonsPanel                             = AddUIComponent<UIPanel>();
        _buttonsPanel.name                        = $"{name}_Buttons";
        _buttonsPanel.autoLayout                  = true;
        _buttonsPanel.autoLayoutDirection         = LayoutDirection.Vertical;
        _buttonsPanel.anchor                      = UIAnchorStyle.CenterVertical;
        _buttonsPanel.autoFitChildrenHorizontally = true;
        _buttonsPanel.autoFitChildrenVertically   = true;

        // NetSetup/Buttons/Spacer
        _buttonsPanel.AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);

        // NetSetup/Buttons/Delete
        _deleteButton = UIButtons.AddIconButton(_buttonsPanel, 0, 0, UIConstants.TinySize, UITextures.LoadQuadSpriteAtlas("PRT-Remove"),
                                                Translations.Translate("TOOLTIP_REMOVE_NETWORK_BUTTON"));

        // NetSetup/Buttons/Reverse
        var reverseAtlas = UITextures.LoadSpriteAtlas("PRT-Reverse", new[] { "Reverse", "ReversePressed" });
        _reverseCheckbox = UICheckBoxes.AddIconToggle(_buttonsPanel, 0, 0, reverseAtlas.name, "ReversePressed", "Reverse",
                                                      backgroundSprite: "OptionBase",
                                                      tooltip: Translations.Translate("TOOLTIP_SNAPPING_TOGGLE_BUTTON"), height: UIConstants.TinySize,
                                                      width: UIConstants.TinySize);
    }

    public override void Start()
    {
        base.Start();

        AttachToEvents();

        // Make NetInfoPanel wide enough to fill the empty space
        _netInfoPanel.width = width - _offsetsPanel.width - _buttonsPanel.width;

        // Since our layout is now complete, we can disable autoLayout for all the panels to avoid wasting CPU cycle
        autoLayout               = false;
        _offsetsPanel.autoLayout = false;
        _buttonsPanel.autoLayout = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        DetachFromEvents();

        // Forcefully destroy all children
        Destroy(_netInfoPanel);
        Destroy(_offsetsPanel);
        Destroy(_horizontalOffsetField);
        Destroy(_verticalOffsetField);
        Destroy(_buttonsPanel);
        Destroy(_reverseCheckbox);
        Destroy(_deleteButton);
    }

    #endregion

    #endregion

    #region Control

    #region Internals

    private void AttachToEvents()
    {
        _deleteButton.eventClicked                += DeleteButton_eventClicked;
        _reverseCheckbox.eventCheckChanged        += NetInfo_EventChanged;
        _horizontalOffsetField.eventTextSubmitted += NetInfo_EventChanged;
        _horizontalOffsetField.eventMouseWheel    += HorizontalOffsetFieldOnEventMouseWheel;
        _verticalOffsetField.eventTextSubmitted   += NetInfo_EventChanged;
        _verticalOffsetField.eventMouseWheel      += VerticalOffsetFieldOnEventMouseWheel;
        _netInfoPanel.OnPopupSelectionChanged     += NetInfoPanelOnOnPopupSelectionChanged;
    }

    private void DetachFromEvents()
    {
        _deleteButton.eventClicked                -= DeleteButton_eventClicked;
        _reverseCheckbox.eventCheckChanged        -= NetInfo_EventChanged;
        _horizontalOffsetField.eventTextSubmitted -= NetInfo_EventChanged;
        _horizontalOffsetField.eventMouseWheel    -= HorizontalOffsetFieldOnEventMouseWheel;
        _verticalOffsetField.eventTextSubmitted   -= NetInfo_EventChanged;
        _verticalOffsetField.eventMouseWheel      -= VerticalOffsetFieldOnEventMouseWheel;
        _netInfoPanel.OnPopupSelectionChanged     -= NetInfoPanelOnOnPopupSelectionChanged;
    }

    /// <summary>
    ///     This will hide the main tools containers so that if <see cref="IsReadOnly" /> is true we don't allow any
    ///     customization.
    /// </summary>
    private void HideTools()
    {
        _offsetsPanel.isVisible = false;
        _buttonsPanel.isVisible = false;
    }

    /// <summary>
    ///     Changes either horizontal or vertical offset based on scroll wheel's delta.
    /// </summary>
    /// <param name="wheelDelta"></param>
    /// <param name="isVertical"></param>
    private void UpdateOffsetOnScrollWheel(float wheelDelta, bool isVertical = false)
    {
        var currentValue = float.Parse(isVertical ? _verticalOffsetField.text : _horizontalOffsetField.text);

        var multiplier = ModifiersManager.IsShiftPressed ? 10f : ModifiersManager.IsCtrlPressed ? 0.1f : 1f;
        currentValue += wheelDelta * multiplier;

        if (isVertical)
            _verticalOffsetField.text = $"{currentValue}";
        else
            _horizontalOffsetField.text = $"{currentValue}";

        NetInfo_EventChanged<string>(this, null);
    }

    #endregion

    #region Public API

    /// <summary>
    ///     To render a <see cref="UINetSetupPanel" /> we just set its parallel/stacked properties and then render the
    ///     <see cref="UINetInfoPanel" />.
    ///     If <see cref="IsReadOnly" /> is true we also set our color to <see cref="UIConstants.ReadOnlyColor" /> to match the
    ///     same default color for a new network.
    /// </summary>
    /// <param name="netInfo"></param>
    public void Render(NetInfoItem netInfo)
    {
        color                       = _isReadOnly ? UIConstants.ReadOnlyColor : netInfo.Color;
        _horizontalOffsetField.text = $"{netInfo.HorizontalOffset}";
        _verticalOffsetField.text   = $"{netInfo.VerticalOffset}";
        _reverseCheckbox.isChecked  = netInfo.IsReversed;

        _netInfoPanel.Render(netInfo);
    }

    #endregion

    #endregion
}
