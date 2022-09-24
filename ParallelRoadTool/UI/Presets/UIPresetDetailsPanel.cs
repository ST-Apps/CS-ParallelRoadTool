using System.Collections.Generic;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;

namespace ParallelRoadTool.UI.Presets
{
    /// <summary>
    ///     This <see cref="UIPanel" /> shows a <see cref="UIList" /> with all the <see cref="NetInfoItem" /> contained in a
    ///     given preset file.
    /// </summary>
    internal class UIPresetDetailsPanel : UIPanel
    {
        #region Control

        #region Public API

        public void LoadPreset(IEnumerable<NetInfoItem> networks)
        {
            var items = new FastList<object>();
            foreach (var netItem in networks)
                items.Add(netItem);

            _netItemsList.Data = items;
        }

        #endregion

        #endregion

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
            _netItemsList = UIList.AddUIList<UINetItemMediumListRow>(this, 0, 0, width, height, UIConstants.MediumSize);

            // Force disable selection
            _netItemsList.EventSelectionChanged += (_, _) => { _netItemsList.SelectedIndex = -1; };
        }

        #endregion

        #endregion
    }
}
