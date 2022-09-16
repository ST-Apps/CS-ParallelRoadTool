using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Interfaces;
using ParallelRoadTool.UI.Utils;
using System.Reflection.Emit;
using UnityEngine;

namespace ParallelRoadTool.UI
{

    public class UINetInfoItemDropdown : UIPanel
    {

        private bool _isReadOnly;

        private UINetInfoPanel _netInfoPanel;
        private UIButton button;

        public bool IsReadOnly { set { _isReadOnly = value; button.isEnabled = false; } }

        public override void Awake()
        {
            size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);

            _netInfoPanel = AddUIComponent<UINetInfoPanel>();
            _netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

            button = AddUIComponent<UIButton>();
            button.size = _netInfoPanel.size;
            button.relativePosition = Vector3.zero;
            button.enabled = true;
            button.hoveredBgSprite = "ButtonWhite";
            button.opacity = 0.25f;

            button.FitWidth(_netInfoPanel, UIConstants.Padding);
            _netInfoPanel.relativePosition = Vector3.zero;
        }

        public void Render(NetInfoItem netInfo)
        {
            _netInfoPanel.Render(netInfo);
        }
    }

    public class UINetInfoPanel : UIPanel, IUIListabeItem<NetInfoItem>
    {
        private UISprite _thumbnail;
        private UILabel _label;

        public string Id { get; set; }

        public override void Awake()
        {
            name = $"{Configuration.ResourcePrefix}NetInfo";
            size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(UIConstants.Padding, UIConstants.Padding, UIConstants.Padding, UIConstants.Padding);

            _thumbnail = AddUIComponent<UISprite>();
            _thumbnail.size = new Vector2(48, 48);

            _label = AddUIComponent<UILabel>();
            _label.textScale = .8f;
            _label.verticalAlignment = UIVerticalAlignment.Middle;
            _label.minimumSize = new Vector2(264, 48);
            _label.autoSize = true;
            _label.wordWrap = true;
        }

        public void Render(NetInfoItem netInfo)
        {
            Log.Info(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Received a new network ""{netInfo.Name}"".");

            _thumbnail.atlas = netInfo.Atlas;
            _thumbnail.spriteName = netInfo.Thumbnail;
            _label.text = netInfo.BeautifiedName;

            Id = netInfo.Name;
        }
    }
}
