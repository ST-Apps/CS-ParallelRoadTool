using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;

// ReSharper disable ClassNeverInstantiated.Global

namespace ParallelRoadTool.UI.Presets
{
    /// <summary>
    ///     Row for a <see cref="UIList" /> item containing a file name.
    /// </summary>
    internal sealed class UIFileListRow : UIListRow
    {
        #region Fields

        #region Components

        private UILabel _fileName;

        #endregion

        #endregion

        #region Unity

        #region Lifecycle

        /// <summary>
        ///     To display the row we simply render a <see cref="UILabel" /> containing our data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rowIndex"></param>
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
