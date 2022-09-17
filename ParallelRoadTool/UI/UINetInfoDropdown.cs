using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UINetInfoDropdown : UIPanel
    {
        private UINetInfoButton _netInfoButton;

        public event MouseEventHandler ToggleDropdownButtonEventClick
        {
            add => _netInfoButton.DropdownToggleButtonEventClick += value;
            remove => _netInfoButton.DropdownToggleButtonEventClick -= value;
        }

        public bool IsReadOnly { set => _netInfoButton.IsReadOnly = value; }

        public override void Awake()
        {
            base.Awake();

            clipChildren = true;
            size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);

            _netInfoButton = AddUIComponent<UINetInfoButton>();
            _netInfoButton.FitWidth(this, 0);
        }

        public void Render(NetInfoItem netInfo)
        {
            _netInfoButton.Render(netInfo);
        }

    }
}
