using System.Collections.Generic;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Presets
{
    internal class UIPresetDetailsPanel : UIPanel
    {
        #region Control

        #region Public API

        public void LoadPreset(IEnumerable<XMLNetItem> networks)
        {
            var items = new FastList<object>();
            foreach (var xmlNetItem in networks)
                items.Add(new NetInfoItem(Singleton<ParallelRoadToolManager>.instance.FromName(xmlNetItem.Name), xmlNetItem.HorizontalOffset,
                                          xmlNetItem.VerticalOffset, xmlNetItem.IsReversed));

            _netItemsList.Data = items;
        }

        #endregion

        #endregion

        private class UIPresetListRow : UIListRow
        {
            public override void Display(object data, int rowIndex)
            {
                var netInfoRow = AddUIComponent<UINetInfoTinyPanel>();
                netInfoRow.Render((NetInfoItem)data);
                netInfoRow.relativePosition = Vector2.zero;
            }
        }

        #region Unity

        #region Components

        private UIList _netItemsList;

        #endregion

        #region Lifecycle

        public UIPresetDetailsPanel()
        {
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = UIHelpers.RectOffsetFromPadding(0);
            autoLayout = true;
        }

        public override void Start()
        {
            base.Start();

            // We need to create this here because this panel's size is set by its container and is not know during ctor
            _netItemsList = UIList.AddUIList<UIPresetListRow>(this, 0, 0,
                                                              width,
                                                              height, UIConstants.NetInfoPanelTinyHeight);
            _netItemsList.isEnabled = false;
        }

        #endregion

        #endregion
    }
}
