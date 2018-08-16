using System;
using System.IO;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using ColossalFramework.Globalization;

namespace ParallelRoadTool.UI
{
    public class UISaveWindow : UIPanel
    {
        public static readonly SavedInt saveWindowX = new SavedInt("saveWindowX", Configuration.SettingsFileName, -1000, true);
        public static readonly SavedInt saveWindowY = new SavedInt("saveWindowY", Configuration.SettingsFileName, -1000, true);

        public class UIFastList : UIFastList<string, UISaveLoadFileRow> { }

        private UIFastList _fastList;
        private UITextField _fileNameInput;
        private UIButton _saveButton;
        private UIDragHandle _dragHandle;
        private UIButton _closeButton;
        private UILabel _exportLabel;
        private UIPanel _savePanel;
        private UIComponent _modalEffect;

        public static UISaveWindow Instance;

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

            _closeButton.eventClicked += (c, p) =>
            {
                Close();
            };

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
            _fileNameInput.submitOnFocusLost = false;

            _fileNameInput.eventTextSubmitted += (c, p) =>
            {
                string filename = _fileNameInput.text.Trim();
                filename = String.Concat(filename.Split(Path.GetInvalidFileNameChars()));

                if (!filename.IsNullOrWhiteSpace())
                {
                    Export(filename);
                }

                _fileNameInput.Focus();
                _fileNameInput.SelectAll();
            };

            // Save
            _saveButton = UIUtil.CreateUiButton(_savePanel, string.Empty, string.Empty, new Vector2(100, 30), string.Empty, true);
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

            _saveButton.eventClicked += (c, p) =>
            {
                string filename = _fileNameInput.text.Trim();
                filename = String.Concat(filename.Split(Path.GetInvalidFileNameChars()));

                if (!filename.IsNullOrWhiteSpace())
                {
                    Export(filename);
                }

                _fileNameInput.Focus();
                _fileNameInput.SelectAll();
            };

            height = _fastList.relativePosition.y + _fastList.height + 8;
            _dragHandle.size = size;
            absolutePosition = new Vector3(saveWindowX.value, saveWindowY.value);
            MakePixelPerfect();

            RefreshFileList();

            _modalEffect = GetUIView().panelsLibraryModalEffect;
            if (_modalEffect != null && !_modalEffect.isVisible)
            {
                _modalEffect.Show(false);
                ValueAnimator.Animate("ModalEffect", delegate (float val)
                {
                    _modalEffect.opacity = val;
                }, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
            }

            BringToFront();
            _fileNameInput.Focus();
        }

        public static void Export(string filename)
        {
            string file = Path.Combine(Configuration.SaveFolder, filename + ".xml");

            if (File.Exists(file))
            {
                ConfirmPanel.ShowModal("Overwrite file", "The file '" + filename + "' already exists.\n Do you want to overwrite it?", (comp, ret) =>
                {
                    if (ret == 1)
                    {
                        //DebugUtils.Log("Deleting " + file);
                        File.Delete(file);
                        Singleton<ParallelRoadTool>.instance.Export(filename);
                        Instance.RefreshFileList();
                    }
                });
            }
            else
            {
                Singleton<ParallelRoadTool>.instance.Export(filename);
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

                UIComponent modalEffect = Instance.GetUIView().panelsLibraryModalEffect;
                if (modalEffect != null && modalEffect.isVisible)
                {
                    ValueAnimator.Animate("ModalEffect", delegate (float val)
                    {
                        modalEffect.opacity = val;
                    }, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate
                    {
                        modalEffect.Hide();
                    });
                }

                Instance.isVisible = false;
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
            Vector2 resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
                MakePixelPerfect();
            }

            absolutePosition = new Vector2(
                (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            saveWindowX.value = (int)absolutePosition.x;
            saveWindowY.value = (int)absolutePosition.y;

            base.OnPositionChanged();
        }

        public void RefreshFileList()
        {
            _fastList.rowsData.Clear();

            if (Directory.Exists(Configuration.SaveFolder))
            {
                string[] files = Directory.GetFiles(Configuration.SaveFolder, "*.xml");

                foreach (string file in files)
                {
                    if (Path.GetFileNameWithoutExtension(file) != Configuration.AutoSaveFileName) //exclude autosaved file from list) 
                        _fastList.rowsData.Add(Path.GetFileNameWithoutExtension(file));
                }

                _fastList.DisplayAt(0);
            }

            _fileNameInput.Focus();
            _fileNameInput.SelectAll();
        }
    }
}