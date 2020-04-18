using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UISaveLoadFileRow : UIPanel, IUIFastListRow<string>
    {
        private UIPanel _background;
        private UIButton _deleteButton;
        private UILabel _fileNameLabel;
        private UIButton _saveLoadButton;

        public UIPanel Background
        {
            get
            {
                if (_background != null) return _background;

                _background = AddUIComponent<UIPanel>();
                _background.width = width;
                _background.height = 40;
                _background.relativePosition = Vector2.zero;
                _background.zOrder = 0;

                return _background;
            }
        }

        public void Display(string data, int i)
        {
            _fileNameLabel.text = data;

            if (i % 2 == 1)
            {
                Background.backgroundSprite = "UnlockingItemBackground";
                Background.color = new Color32(0, 0, 0, 128);
                Background.width = parent.width;
            }
            else
            {
                Background.backgroundSprite = null;
            }
        }

        public void Select(bool isRowOdd)
        {
        }

        public void Deselect(bool isRowOdd)
        {
        }

        private void UnsubscribeFromUiEvents()
        {
            _saveLoadButton.eventClicked -= SaveLoadButtonOnEventClicked;
            _deleteButton.eventClicked -= DeleteButtonOnEventClicked;
        }

        private void SubscribeToUiEvents()
        {
            _saveLoadButton.eventClicked += SaveLoadButtonOnEventClicked;
            _deleteButton.eventClicked += DeleteButtonOnEventClicked;
        }

        private void DeleteButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventparam)
        {
            ConfirmPanel.ShowModal(
                                   Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "DeleteButton"),
                                   string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DeleteConfirmationMessage"),
                                                 _fileNameLabel.text),
                                   (comp, ret) =>
                                   {
                                       if (ret != 1) return;

                                       PresetsUtils.Delete(_fileNameLabel.text);

                                       if (UISaveWindow.Instance != null)
                                           UISaveWindow.Instance.RefreshFileList();
                                       else
                                           UILoadWindow.Instance.RefreshFileList();
                                   });
        }

        private void SaveLoadButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventparam)
        {
            if (UISaveWindow.Instance != null)
            {
                UISaveWindow.Export(_fileNameLabel.text);
            }
            else
            {
                UILoadWindow.Close();
                PresetsUtils.Import(_fileNameLabel.text);
            }
        }

        public override void Awake()
        {
            height = 46;

            _fileNameLabel = AddUIComponent<UILabel>();
            _fileNameLabel.textScale = 0.9f;
            _fileNameLabel.autoSize = false;
            _fileNameLabel.height = 30;
            _fileNameLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _fileNameLabel.relativePosition = new Vector3(8, 8);

            _deleteButton = UIUtil.CreateUiButton(this, string.Empty, string.Empty, new Vector2(80, 30), "InfoIconGarbage", true);
            _deleteButton.name = $"{Configuration.ResourcePrefix}DeleteFileButton";
            _deleteButton.size = new Vector2(40f, 30f);
            _deleteButton.relativePosition = new Vector3(430 - _deleteButton.width - 8, 8);
            _deleteButton.tooltip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "DeleteButton");

            _saveLoadButton = UIUtil.CreateUiButton(this, string.Empty, string.Empty, new Vector2(80, 30), string.Empty, true);
            _saveLoadButton.name = $"{Configuration.ResourcePrefix}SaveLoadFileButton";
            _saveLoadButton.text = UISaveWindow.Instance != null
                                       ? Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "OverwriteButton")
                                       : Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ImportButton");
            _saveLoadButton.tooltip = UISaveWindow.Instance != null
                                          ? Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "OverwriteButton")
                                          : Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ImportButton");
            _saveLoadButton.size = new Vector2(80f, 30f);
            _saveLoadButton.relativePosition = new Vector3(_deleteButton.relativePosition.x - _saveLoadButton.width - 8, 8);

            SubscribeToUiEvents();

            _fileNameLabel.width = _saveLoadButton.relativePosition.x - 16f;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromUiEvents();
        }
    }
}
