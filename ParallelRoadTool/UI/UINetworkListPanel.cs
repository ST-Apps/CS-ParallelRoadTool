using ColossalFramework.UI;
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
    internal class UINetworkListPanel : UIPanel
    {
        #region Constants

        // We only have padding for bottom side to separate multiple elements
        private readonly RectOffset LayoutPadding = new RectOffset(0, 0, 0, UIConstants.Padding / 2);

        #endregion

        #region Events

        public event Action<UINetSetupPanel> UINetSetupPanelClicked;

        #endregion

        #region Unity

        #region Components

        private readonly UIList<UINetSetupPanel> _netSetupPanels = new UIList<UINetSetupPanel>();

        #endregion

        public override void Awake()
        {
            // NetworkList
            name = $"{Configuration.ResourcePrefix}NetworkList";
            autoLayout = true;
            autoLayoutPadding = LayoutPadding;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Vertical;
        }

        #endregion

        #region Control

        /// <summary>
        /// 
        /// </summary>
        /// <param name="netInfo"></param>
        /// <param name="isCurrentNetwork">True if this is the info panel for the currently selected network in <see cref="NetTool"/>. Current network can't be customized.</param>
        internal void AddNetwork(ExtendedNetInfo netInfo)
        {
            // Basic setup
            var netSetupPanel = AddUIComponent<UINetSetupPanel>();
            netSetupPanel.FitWidth(this, 0);
            _netSetupPanels.Add(netSetupPanel);

            // Finally render the panel
            netSetupPanel.Render(netInfo);
        }

        #endregion
    }
}
