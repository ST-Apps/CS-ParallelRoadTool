using AlgernonCommons.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Shared
{
    /// <summary>
    ///     Row component for a <see cref="UIList" /> displaying <see cref="NetInfoItem" /> wrapped in a
    ///     <see cref="UINetInfoTinyPanel" />.
    /// </summary>
    internal class UINetItemListRow : UIListRow
    {
        #region Fields

        #region Unity

        #region Components

        private UINetInfoTinyPanel _netInfoRow;

        #endregion

        #endregion

        #endregion

        #region Control

        #region Public API

        /// <summary>
        ///     Generates and displays contents for the provided data object.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rowIndex"></param>
        public override void Display(object data, int rowIndex)
        {
            if (_netInfoRow == null)
            {
                // Init our row
                width         = parent.width;
                height        = RowHeight;
                isInteractive = false;

                // Set the item
                _netInfoRow                  = AddUIComponent<UINetInfoTinyPanel>();
                _netInfoRow.relativePosition = Vector2.zero;
                _netInfoRow.isInteractive    = false;
            }

            // Set the current item
            _netInfoRow.NetInfoItem = data as NetInfoItem;

            // Deselect to reset its style in case it was selected before
            Deselect(rowIndex);
        }

        /// <summary>
        ///     Sets the row display to the selected state (highlighted).
        /// </summary>
        public override void Select()
        {
            BackgroundOpacity = 1f;
        }

        /// <summary>
        ///     Sets the row display to the deselected state.
        /// </summary>
        /// <param name="rowIndex">Row index number (for background banding).</param>
        public override void Deselect(int rowIndex)
        {
            BackgroundSpriteName = "GenericPanel";
            BackgroundColor      = _netInfoRow.color;
            BackgroundOpacity    = 0.25f;
        }

        #endregion

        #endregion
    }

    /// <summary>
    ///     Medium-sized height <see cref="UINetItemListRow" />.
    /// </summary>
    internal class UINetItemMediumListRow : UINetItemListRow
    {
        #region Properties

        public override float RowHeight => UIConstants.MediumSize;

        #endregion
    }

    /// <summary>
    ///     Large-sized height <see cref="UINetItemListRow" />.
    /// </summary>
    internal class UINetItemLargeListRow : UINetItemListRow
    {
        #region Properties

        public override float RowHeight => UIConstants.LargeSize;

        #endregion
    }
}
