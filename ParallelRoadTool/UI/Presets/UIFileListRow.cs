using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;

namespace ParallelRoadTool.UI.Presets
{
    internal sealed class UIFileListRow : UIListRow
    {
        #region Components

        private UILabel _fileName;

        #endregion

        #region Unity

        #region Lifecycle

        public override void Display(object data, int rowIndex)
        {
            if (_fileName == null)
            {
                // Init our row
                width = parent.width;
                height = RowHeight;

                // Set the label
                _fileName = AddLabel(UIConstants.Padding, width - 2 * UIConstants.Padding, wordWrap: true);
            }

            _fileName.text = (string)data;
            Deselect(rowIndex);
        }

        #endregion

        #endregion
    }
}
