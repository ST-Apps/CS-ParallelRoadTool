using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;
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

        #region Unity

        #region Components

        private readonly List<UINetSetupPanel> _netSetupPanels = new List<UINetSetupPanel>();

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
        internal void AddNetwork(NetInfo netInfo, bool isCurrentNetwork = false)
        {
            // Basic setup
            var netSetupPanel = AddUIComponent<UINetSetupPanel>();
            netSetupPanel.FitWidth(this, 0);
            netSetupPanel.Refresh(netInfo);
            
            // Set a color for this network
            // TODO: colors are used for overlay rendering too so move outside of this class and make it a parameter
            if (isCurrentNetwork)
            {
                netSetupPanel.color = new Color(0, 0.710f, 1, 1);
            } else
            {
                netSetupPanel.color = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);
            }

            _netSetupPanels.Add(netSetupPanel);
        }

        #endregion
    }
}
