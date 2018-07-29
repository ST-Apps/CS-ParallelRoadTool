using System;
using System.IO;

using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace ParallelRoadTool.UI
{
    public class UISaveWindow : UIPanel
    {
        public static readonly SavedInt saveWindowX = new SavedInt("saveWindowX", ParallelRoadTool.SettingsFileName, -1000, true);
        public static readonly SavedInt saveWindowY = new SavedInt("saveWindowY", ParallelRoadTool.SettingsFileName, -1000, true);

        public class UIFastList : UIFastList<string, UISaveLoadFileRow> { }
        public UIFastList fastList;

        public UITextField fileNameInput;

        public UIButton saveButton;
        public UIPanel savePanel;

        public UIButton close;

        public static UISaveWindow instance;

        public override void Start()
        {
            name = "PRT_SaveWindow";
            atlas = UIUtils.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(465, 180);
            canFocus = true;

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.target = parent;
            dragHandle.relativePosition = Vector3.zero;

            close = AddUIComponent<UIButton>();
            close.size = new Vector2(30f, 30f);
            close.text = "X";
            close.textScale = 0.9f;
            close.textColor = new Color32(118, 123, 123, 255);
            close.focusedTextColor = new Color32(118, 123, 123, 255);
            close.hoveredTextColor = new Color32(140, 142, 142, 255);
            close.pressedTextColor = new Color32(99, 102, 102, 102);
            close.textPadding = new RectOffset(8, 8, 8, 8);
            close.canFocus = false;
            close.playAudioEvents = true;
            close.relativePosition = new Vector3(width - close.width, 0);

            close.eventClicked += (c, p) =>
            {
                Close();
            };

            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Export";
            label.relativePosition = new Vector2(8, 8);
            label.SendToBack();

            // Save Panel
            UIPanel savePanel = AddUIComponent<UIPanel>();
            savePanel.atlas = atlas;
            savePanel.backgroundSprite = "GenericPanel";
            savePanel.color = new Color32(206, 206, 206, 255);
            savePanel.size = new Vector2(width - 16, 46);
            savePanel.relativePosition = new Vector2(8, 28);

            // Input
            fileNameInput = UIUtils.CreateTextField(savePanel);
            fileNameInput.padding.top = 7;
            fileNameInput.horizontalAlignment = UIHorizontalAlignment.Left;
            fileNameInput.relativePosition = new Vector3(8, 8);
            fileNameInput.submitOnFocusLost = false;

            fileNameInput.eventTextSubmitted += (c, p) =>
            {
                string filename = fileNameInput.text.Trim();
                filename = String.Concat(filename.Split(Path.GetInvalidFileNameChars()));

                if (!filename.IsNullOrWhiteSpace())
                {
                    Export(filename);
                }

                fileNameInput.Focus();
                fileNameInput.SelectAll();
            };

            // Save
            saveButton = UIUtils.CreateButton(savePanel);
            saveButton.name = "PRT_SaveButton";
            saveButton.text = "Export";
            saveButton.size = new Vector2(100f, 30f);
            saveButton.relativePosition = new Vector3(savePanel.width - saveButton.width - 8, 8);

            fileNameInput.size = new Vector2(saveButton.relativePosition.x - 16f, 30f);

            // FastList
            fastList = AddUIComponent<UIFastList>();
            fastList.backgroundSprite = "UnlockingPanel";
            fastList.width = width - 16;
            fastList.height = 46 * 5;
            fastList.canSelect = true;
            fastList.relativePosition = new Vector3(8, savePanel.relativePosition.y + savePanel.height + 8);

            fastList.rowHeight = 46f;

            saveButton.eventClicked += (c, p) =>
            {
                string filename = fileNameInput.text.Trim();
                filename = String.Concat(filename.Split(Path.GetInvalidFileNameChars()));

                if (!filename.IsNullOrWhiteSpace())
                {
                    Export(filename);
                }

                fileNameInput.Focus();
                fileNameInput.SelectAll();
            };

            height = fastList.relativePosition.y + fastList.height + 8;
            dragHandle.size = size;
            absolutePosition = new Vector3(saveWindowX.value, saveWindowY.value);
            MakePixelPerfect();

            RefreshFileList();

            UIComponent modalEffect = GetUIView().panelsLibraryModalEffect;
            if (modalEffect != null && !modalEffect.isVisible)
            {
                modalEffect.Show(false);
                ValueAnimator.Animate("ModalEffect", delegate (float val)
                {
                    modalEffect.opacity = val;
                }, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
            }

            BringToFront();
            fileNameInput.Focus();
        }

        public static void Export(string filename)
        {
            string file = Path.Combine(ParallelRoadTool.SaveFolder, filename + ".xml");

            if (File.Exists(file))
            {
                ConfirmPanel.ShowModal("Overwrite file", "The file '" + filename + "' already exists.\n Do you want to overwrite it?", (comp, ret) =>
                {
                    if (ret == 1)
                    {
                        DebugUtils.Log("Deleting " + file);
                        File.Delete(file);
                        ParallelRoadTool.Instance.Export(filename);
                        instance.RefreshFileList();
                    }
                });
            }
            else
            {
                ParallelRoadTool.Instance.Export(filename);
                instance.RefreshFileList();
            }
        }

        public static void Open()
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UISaveWindow)) as UISaveWindow;
                UIView.PushModal(instance);
            }
        }

        public static void Close()
        {
            if (instance != null)
            {
                UIView.PopModal();

                UIComponent modalEffect = instance.GetUIView().panelsLibraryModalEffect;
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

                instance.isVisible = false;
                Destroy(instance.gameObject);
                instance = null;
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
            fastList.rowsData.Clear();

            if (Directory.Exists(ParallelRoadTool.SaveFolder))
            {
                string[] files = Directory.GetFiles(ParallelRoadTool.SaveFolder, "*.xml");

                foreach (string file in files)
                {
                    fastList.rowsData.Add(Path.GetFileNameWithoutExtension(file));
                }

                fastList.DisplayAt(0);
            }

            fileNameInput.Focus();
            fileNameInput.SelectAll();
        }
    }
}