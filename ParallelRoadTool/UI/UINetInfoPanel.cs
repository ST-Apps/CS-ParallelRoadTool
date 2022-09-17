using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    /// <summary>
    /// This is the main panel used to render a <see cref="NetInfoItem"/> as both its thumbnail and its name.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UINetInfoPanel : UIPanel
    {
        #region Unity

        #region Components

        private UISprite _thumbnail;
        private UILabel _label;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
            base.Awake();

            // Main
            name = $"{Configuration.ResourcePrefix}NetInfo";
            size = UIConstants.NetInfoPanelSize;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);

            // Main/Thumbnail
            _thumbnail = AddUIComponent<UISprite>();
            _thumbnail.size = UIConstants.ThumbnailSize;

            // Main/Label
            _label = AddUIComponent<UILabel>();
            _label.textScale = .8f;
            _label.verticalAlignment = UIVerticalAlignment.Middle;
            _label.minimumSize = new Vector2(UIConstants.NetInfoPanelWidth - UIConstants.NetInfoPanelHeight, UIConstants.LargeSize);
            _label.autoSize = true;
            _label.wordWrap = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Forcefully destroy all children
            Destroy(_thumbnail);
            Destroy(_label);
        }

        #endregion

        #endregion

        #region Control

        /// <summary>
        /// To render a <see cref="NetInfoItem"/> we just to set both atlas and spriteName for <see cref="_thumbnail"/>, as well as the provided network name.
        /// </summary>
        /// <param name="netInfo"></param>
        public void Render(NetInfoItem netInfo)
        {
            Log._Debug(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Received a new network ""{netInfo.Name}"".");

            _thumbnail.atlas = netInfo.Atlas;
            _thumbnail.spriteName = netInfo.Thumbnail;
            _label.text = netInfo.BeautifiedName;

            color = netInfo.Color;
        }

        #endregion
    }
}
