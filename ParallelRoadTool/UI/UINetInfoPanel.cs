using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetInfoPanel : UIPanel
    {
        private UISprite _thumbnail;
        private UILabel _label;

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

        public void Refresh(NetInfo netInfo)
        {
            Log.Info($"[{nameof(UINetInfoPanel)}.{nameof(Refresh)}] Received a new network {netInfo.name} [{_thumbnail == null}]");

            _thumbnail.atlas = netInfo.m_Atlas;
            _thumbnail.spriteName = netInfo.m_Thumbnail;
            _label.text = netInfo.GenerateBeautifiedNetName();
        }

    }
}
