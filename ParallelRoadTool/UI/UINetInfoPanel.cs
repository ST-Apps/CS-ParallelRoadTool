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

        private const int Padding = 12;
        private UISprite _thumbnail;
        private UILabel _label;

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}NetTypeItem";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            color = new Color32(255, 255, 255, 255);
            size = new Vector2(400, 48 + Padding + Padding);
            // padding = new RectOffset(Padding, Padding, Padding, Padding);
            absolutePosition = new Vector2(100, 100);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = new RectOffset(Padding, Padding, Padding, Padding);

            _thumbnail = AddUIComponent<UISprite>();
            _thumbnail.size = new Vector2(48, 48);
            // _thumbnail.relativePosition = new Vector2(Padding, Padding);

            _label = AddUIComponent<UILabel>();
            _label.textScale = .8f;
            _label.verticalAlignment = UIVerticalAlignment.Middle;
            _label.minimumSize = new Vector2(264, 48);
            _label.autoSize = true;
            //_label.size = new Vector2(size.x - _thumbnail.size.x - Padding - Padding, size.y - Padding - Padding);
            _label.wordWrap = true;
            // _label.relativePosition = new Vector3(_thumbnail.relativePosition.x + _thumbnail.size.x + Padding, 0);
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
