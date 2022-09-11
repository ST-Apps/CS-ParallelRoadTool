using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Interfaces;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetInfoPanel : UIPanel, IUIListabeItem<ExtendedNetInfo>
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
            hoverCursor = new ColossalFramework.CursorInfo
            {
                m_texture = UIHelpers.Atlas["FindIt"].texture
            };

            _thumbnail = AddUIComponent<UISprite>();
            _thumbnail.size = new Vector2(48, 48);

            _label = AddUIComponent<UILabel>();
            _label.textScale = .8f;
            _label.verticalAlignment = UIVerticalAlignment.Middle;
            _label.minimumSize = new Vector2(264, 48);
            _label.autoSize = true;
            _label.wordWrap = true;
        }

        public void Render(ExtendedNetInfo netInfo)
        {
            Log.Info(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Received a new network ""{netInfo.Name}"".");
            Log._Debug(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Network's metadata: [ATLAS = ""{netInfo.Atlas?.name}"", THUMBNAIL = ""{netInfo.Thumbnail}""].");
            Log._Debug(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Network's ORIGINAL metadata: [ATLAS = ""{netInfo.NetInfo.m_Atlas?.name}"", THUMBNAIL = ""{netInfo.NetInfo.m_Thumbnail}""].");

            _thumbnail.atlas = netInfo.NetInfo.m_Atlas; // netInfo.Atlas;
            _thumbnail.spriteName = netInfo.NetInfo.m_Thumbnail; // netInfo.Thumbnail;
            _label.text = netInfo.BeautifiedName;

            Id = netInfo.Name;
        }

        internal void MakeReadOnly()
        {
            color = new Color(0, 0.710f, 1, 1);
            opacity = 0.5f;
        }
    }
}
