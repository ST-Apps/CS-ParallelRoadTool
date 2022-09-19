using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Shared
{
    /// <summary>
    /// This is the main panel used to render a <see cref="NetInfoItem"/> as both its thumbnail and its name.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UINetInfoPanel : UIPanel
    {
        #region Unity

        #region Components

        protected readonly UISprite Thumbnail;
        protected readonly UILabel Label;

        #endregion

        #region Lifecycle

        public UINetInfoPanel()
        {
            // Main
            name = $"{Configuration.ResourcePrefix}NetInfo";
            size = UIConstants.NetInfoPanelLargeSize;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
            // We don't want padding on right side
            autoLayoutPadding.right = 0;

            // Main/Thumbnail
            Thumbnail = AddUIComponent<UISprite>();
            Thumbnail.size = UIConstants.ThumbnailLargeSize;

            // Main/Label
            Label = AddUIComponent<UILabel>();
            Label.textScale = .8f;
            Label.verticalAlignment = UIVerticalAlignment.Middle;
            Label.autoSize = false;
            Label.wordWrap = true;
            // Label should fill up the remaining space
            // x is 5 * padding because we have one at the beginning of the row, one between thumbnail and label, one at the end of the row, and we have also to consider that the entire panel is padded twice.
            Label.size = UIConstants.NetInfoPanelLargeSize - new Vector2(Thumbnail.width + 5 * UIConstants.Padding, 2 * UIConstants.Padding);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Forcefully destroy all children
            Destroy(Thumbnail);
            Destroy(Label);
        }

        #endregion

        #endregion

        #region Control

        #region Public API

        /// <summary>
        /// To render a <see cref="NetInfoItem"/> we just to set both atlas and spriteName for <see cref="Thumbnail"/>, as well as the provided network name.
        /// </summary>
        /// <param name="netInfo"></param>
        public void Render(NetInfoItem netInfo)
        {
            Log._Debug(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Received a new network ""{netInfo.Name}"".");

            Thumbnail.atlas = netInfo.Atlas;
            Thumbnail.spriteName = netInfo.Thumbnail;
            Label.text = netInfo.BeautifiedName;

            color = netInfo.Color;
        }

        #endregion



        #endregion
    }

    public class UINetInfoTinyPanel : UINetInfoPanel
    {
        public UINetInfoTinyPanel()
        {
            size = UIConstants.NetInfoPanelTinySize;

            Thumbnail.size = UIConstants.ThumbnailTinySize;
            Label.size = UIConstants.NetInfoPanelTinySize - new Vector2(Thumbnail.width + 2 * UIConstants.Padding, 2 * UIConstants.Padding);
        }
    }
}
