using System.IO;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Utils;

namespace ParallelRoadTool.UI
{
    public class UILoadWindow : UIPanel
    {
        private static readonly SavedInt loadWindowX = new SavedInt("loadWindowX", Configuration.SettingsFileName, -1000, true);
        private static readonly SavedInt loadWindowY = new SavedInt("loadWindowY", Configuration.SettingsFileName, -1000, true);

        public class UIFastList : UIFastList<string, UISaveLoadFileRow> { }

        private UIFastList _fastList;
        private UIButton _closeButton;
        private UIDragHandle _dragHandle;
        private UILabel _importLabel;
        private UIComponent _modalEffect;

        public static UILoadWindow Instance;

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}LoadWindow";
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

            _importLabel = AddUIComponent<UILabel>();
            _importLabel.textScale = 0.9f;
            _importLabel.text = "Import";
            _importLabel.relativePosition = new Vector2(8, 8);
            _importLabel.SendToBack();

            // FastList
            _fastList = AddUIComponent<UIFastList>();
            _fastList.backgroundSprite = "UnlockingPanel";
            _fastList.width = width - 16;
            _fastList.height = 46 * 5;
            _fastList.canSelect = true;
            _fastList.relativePosition = new Vector3(8, 28);

            _fastList.rowHeight = 46f;

            height = _fastList.relativePosition.y + _fastList.height + 8;
            _dragHandle.size = size;
            absolutePosition = new Vector3(loadWindowX.value, loadWindowY.value);
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
            Focus();
        }

        public static void Open()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(UILoadWindow)) as UILoadWindow;
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
                    modalEffect.Hide();

                    /*ValueAnimator.Animate("ModalEffect", delegate (float val)
                    {
                        modalEffect.opacity = val;
                    }, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate
                    {
                        modalEffect.Hide();
                    });*/
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

            loadWindowX.value = (int)absolutePosition.x;
            loadWindowY.value = (int)absolutePosition.y;

            base.OnPositionChanged();
        }

        public void RefreshFileList()
        {
            _fastList.rowsData.Clear();

            if (Directory.Exists(Configuration.AutoSaveFolderPath))
            {
                string[] files = Directory.GetFiles(Configuration.AutoSaveFolderPath, "*.xml");

                foreach (string file in files)
                {
                    if (Path.GetFileNameWithoutExtension(file) != Configuration.AutoSaveFileName) //exclude autosaved file from list) 
                        _fastList.rowsData.Add(Path.GetFileNameWithoutExtension(file));
                }

                _fastList.DisplayAt(0);
            }

            Focus();
        }
    }
}