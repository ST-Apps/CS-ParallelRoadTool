using System.Collections.Generic;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Presets
{
    public sealed class UISavePresetWindow : UIModalWindow
    {
        #region Callbacks

        private void FileListOnEventSelectionChanged(UIComponent component, object value)
        {
            // Set the clicked item as filename
            _fileNameInput.text = (string)value;
        }

        private void FileNameInput_eventTextChanged(UIComponent component, string value)
        {
            // If text is not empty we can make the save button interactive again, otherwise we disable it
            _saveButton.isEnabled = !string.IsNullOrEmpty(value);
        }

        #endregion

        #region Control

        #region Internals

        private void AttachToEvents()
        {
            _fileList.EventSelectionChanged += FileListOnEventSelectionChanged;
            _fileNameInput.eventTextChanged += FileNameInput_eventTextChanged;
        }

        private void DetachFromEvents()
        {
            _fileList.EventSelectionChanged -= FileListOnEventSelectionChanged;
        }

        #endregion

        #region Public API

        public void RefreshItems(IEnumerable<string> items)
        {
            var fileList = new FastList<object>();
            foreach (var item in items) fileList.Add(item);

            _fileList.Data = fileList;
        }

        #endregion

        #endregion

        #region Properties

        public override float PanelHeight => 256;

        protected override string PanelTitle => Translations.Translate("LABEL_SAVE_PRESET_WINDOW_TITLE");

        #endregion

        #region Unity

        #region Components

        private readonly UITextField _fileNameInput;
        private readonly UIButton _saveButton;
        private readonly UIList _fileList;

        #endregion

        #region Lifecycle

        public UISavePresetWindow() : base("Parallel")
        {
            var topPanel = Container.AddUIComponent<UIPanel>();
            topPanel.relativePosition = new Vector2(0, UIConstants.Padding);
            topPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            topPanel.autoLayout = true;
            topPanel.autoFitChildrenVertically = true;
            topPanel.FitWidth(Container, UIConstants.Padding);

            // Main/FileName
            _fileNameInput = UITextFields.AddBigTextField(topPanel, 0, 0);
            _fileNameInput.FitWidth(topPanel, 0);

            // Main/Save
            var saveButtonAtlas = UITextures.LoadSingleSpriteAtlas("Save");
            _saveButton = UIButtons.AddIconButton(topPanel, 0, 0, saveButtonAtlas["normal"].height, saveButtonAtlas);
            _saveButton.isEnabled = false; //Disabled at start
            _fileNameInput.height = saveButtonAtlas["normal"].height;
            _fileNameInput.width -= _saveButton.width - UIConstants.Padding;

            // Main/FileList
            _fileList = UIList.AddUIList<UIFileListRow>(Container, 0, 0,
                                                        Container.width - 2 * UIConstants.Padding,
                                                        Container.height - topPanel.height - 3 * UIConstants.Padding, UIConstants.TinySize);

            // Events
            AttachToEvents();
        }

        public override void OnDestroy()
        {
            DetachFromEvents();

            base.OnDestroy();
        }

        #endregion

        #endregion
    }
}
