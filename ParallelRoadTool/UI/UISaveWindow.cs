using System.IO;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UISaveWindow : UIPanel
    {
        public static readonly SavedInt saveWindowX =
            new SavedInt("saveWindowX", Configuration.SettingsFileName, -1000, true);

        public static readonly SavedInt saveWindowY =
            new SavedInt("saveWindowY", Configuration.SettingsFileName, -1000, true);

        public static UISaveWindow Instance;

        private readonly bool _isSaving;
        private readonly object _saveLock = new object();
        private UIButton _closeButton;
        private UIDragHandle _dragHandle;
        private UILabel _exportLabel;

        private UIFastList _fastList;
        private UITextField _fileNameInput;
        private UIComponent _modalEffect;
        private UIButton _saveButton;
        private UIPanel _savePanel;

        private void UnsubscribeFromUiEvents()
        {
            _closeButton.eventClicked -= CloseButtonOnEventClicked;
        }

        private void SubscribeToUiEvents()
        {
            _closeButton.eventClicked += CloseButtonOnEventClicked;
            _fileNameInput.eventTextSubmitted += FileNameInputOnEventTextSubmitted;
            _saveButton.eventClicked += SaveButtonOnEventClicked;
        }

        private void SaveButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventparam)
        {
            SaveFile();
        }

        private void FileNameInputOnEventTextSubmitted(UIComponent component, string value)
        {
            // TODO: decommenting this will enable enter key for submit but files will be saved multiple times, showing multiple "overwrite" popups.
            //SaveFile();
        }

        private void CloseButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventparam)
        {
            Close();
        }

        private void SaveFile()
        {
            //lock (_saveLock)
            //{
            //    DebugUtils.Log($"IsSaving: {_isSaving}");
            //    if (_isSaving)
            //    {
            //        return;
            //    }

            //    _isSaving = true;
            //    DebugUtils.Log($"IsSaving set to true: {_isSaving}");
            //}
            var filename = _fileNameInput.text.Trim();
            filename = string.Concat(filename.Split(Path.GetInvalidFileNameChars()));

            if (!filename.IsNullOrWhiteSpace()) Export(filename);

            _fileNameInput.Focus();
            _fileNameInput.SelectAll();
            //_isSaving = false;
        }

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}SaveWindow";
            atlas = UIUtil.DefaultAtlas;
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(465, 180);
            canFocus = true;

            _dragHandle = AddUIComponent<UIDragHandle>();
            _dragHandle.target = parent;
            _dragHandle.relativePosition = Vector3.zero;

            _closeButton = AddUIComponent<UIButton>();
            _closeButton.size = new Vector2(30f, 30f);
            _closeButton.text = "X";
            _closeButton.textScale = 0.9f;
            _closeButton.textColor = new Color32(118, 123, 123, 255);
            _closeButton.focusedTextColor = new Color32(118, 123, 123, 255);
            _closeButton.hoveredTextColor = new Color32(140, 142, 142, 255);
            _closeButton.pressedTextColor = new Color32(99, 102, 102, 102);
            _closeButton.textPadding = new RectOffset(8, 8, 8, 8);
            _closeButton.canFocus = false;
            _closeButton.playAudioEvents = true;
            _closeButton.relativePosition = new Vector3(width - _closeButton.width, 0);

            _exportLabel = AddUIComponent<UILabel>();
            _exportLabel.textScale = 0.9f;
            _exportLabel.text = "Export";
            _exportLabel.relativePosition = new Vector2(8, 8);
            _exportLabel.SendToBack();

            // Save Panel
            _savePanel = AddUIComponent<UIPanel>();
            _savePanel.atlas = atlas;
            _savePanel.backgroundSprite = "GenericPanel";
            _savePanel.color = new Color32(206, 206, 206, 255);
            _savePanel.size = new Vector2(width - 16, 46);
            _savePanel.relativePosition = new Vector2(8, 28);

            // Input
            _fileNameInput = UIUtil.CreateTextField(_savePanel);
            _fileNameInput.padding.top = 7;
            _fileNameInput.horizontalAlignment = UIHorizontalAlignment.Left;
            _fileNameInput.relativePosition = new Vector3(8, 8);
            _fileNameInput.submitOnFocusLost = true;

            // Save
            _saveButton = UIUtil.CreateUiButton(_savePanel, string.Empty, string.Empty, new Vector2(100, 30),
                string.Empty, true);
            _saveButton.name = $"{Configuration.ResourcePrefix}SaveButton";
            _saveButton.text = Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ExportButton");
            _saveButton.tooltip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ExportButton");
            _saveButton.relativePosition = new Vector3(_savePanel.width - _saveButton.width - 8, 8);

            _fileNameInput.size = new Vector2(_saveButton.relativePosition.x - 16f, 30f);

            // FastList
            _fastList = AddUIComponent<UIFastList>();
            _fastList.backgroundSprite = "UnlockingPanel";
            _fastList.width = width - 16;
            _fastList.height = 46 * 5;
            _fastList.canSelect = true;
            _fastList.relativePosition = new Vector3(8, _savePanel.relativePosition.y + _savePanel.height + 8);
            _fastList.rowHeight = 46f;

            height = _fastList.relativePosition.y + _fastList.height + 8;
            _dragHandle.size = size;
            absolutePosition = new Vector3(saveWindowX.value, saveWindowY.value);
            MakePixelPerfect();

            RefreshFileList();

            _modalEffect = GetUIView().panelsLibraryModalEffect;
            if (_modalEffect != null && !_modalEffect.isVisible)
            {
                _modalEffect.Show(false);
                ValueAnimator.Animate("ModalEffect", delegate(float val) { _modalEffect.opacity = val; },
                    new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
            }

            SubscribeToUiEvents();

            BringToFront();
            _fileNameInput.Focus();
        }

        public static void Export(string filename)
        {
            var file = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");

            if (File.Exists(file))
            {
                ConfirmPanel.ShowModal(
                    Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "OverwriteButton"),
                    string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "OverwriteConfirmationMessage"),
                        filename),
                    (comp, ret) =>
                    {
                        if (ret != 1) return;
                        //DebugUtils.Log("Deleting " + file);
                        File.Delete(file);
                        PresetsUtils.Export(filename);
                        Instance.RefreshFileList();
                    });
            }
            else
            {
                PresetsUtils.Export(filename);
                Instance.RefreshFileList();
            }
        }

        public static void Open()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(UISaveWindow)) as UISaveWindow;
                UIView.PushModal(Instance);
            }
        }

        public static void Close()
        {
            if (Instance != null)
            {
                UIView.PopModal();

                var modalEffect = Instance.GetUIView().panelsLibraryModalEffect;
                if (modalEffect != null && modalEffect.isVisible)
                    ValueAnimator.Animate("ModalEffect", delegate(float val) { modalEffect.opacity = val; },
                        new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate { modalEffect.Hide(); });

                Instance.isVisible = false;
                Instance.UnsubscribeFromUiEvents();
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                Close();
            }

            base.OnKeyDown(p);
        }

        protected override void OnPositionChanged()
        {
            var resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
                MakePixelPerfect();
            }

            absolutePosition = new Vector2(
                (int) Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int) Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            saveWindowX.value = (int) absolutePosition.x;
            saveWindowY.value = (int) absolutePosition.y;

            base.OnPositionChanged();
        }

        public void RefreshFileList()
        {
            _fastList.rowsData.Clear();

            if (Directory.Exists(Configuration.AutoSaveFolderPath))
            {
                var files = Directory.GetFiles(Configuration.AutoSaveFolderPath, "*.xml")
                    .Where(f => Path.GetFileNameWithoutExtension(f) != Configuration.AutoSaveFileName)
                    .Select(Path.GetFileNameWithoutExtension);

                foreach (var file in files)
                        _fastList.rowsData.Add(file);

                _fastList.DisplayAt(0);
            }

            _fileNameInput.Focus();
            _fileNameInput.SelectAll();
        }

        public class UIFastList : UIFastList<string, UISaveLoadFileRow>
        {
        }
    }
}