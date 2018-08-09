using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using ParallelRoadTool.Utils;
using ColossalFramework.Globalization;

namespace ParallelRoadTool.UI
{
    public class UISaveLoadFileRow : UIPanel, IUIFastListRow<string>
    {
        public UILabel fileNameLabel;

        public UIButton saveLoadButton;
        public UIButton deleteButton;

        private UIPanel m_background;

        public UIPanel background
        {
            get
            {
                if (m_background == null)
                {
                    m_background = AddUIComponent<UIPanel>();
                    m_background.width = width;
                    m_background.height = 40;
                    m_background.relativePosition = Vector2.zero;

                    m_background.zOrder = 0;
                }

                return m_background;
            }
        }

        public override void Awake()
        {
            height = 46;

            fileNameLabel = AddUIComponent<UILabel>();
            fileNameLabel.textScale = 0.9f;
            fileNameLabel.autoSize = false;
            fileNameLabel.height = 30;
            fileNameLabel.verticalAlignment = UIVerticalAlignment.Middle;
            fileNameLabel.relativePosition = new Vector3(8, 8);

            deleteButton = UIUtil.CreateUiButton(this, "", "", new Vector2(80, 30), "");
            deleteButton.name = $"{Configuration.ResourcePrefix}DeleteFileButton";
            deleteButton.text = "X";
            deleteButton.size = new Vector2(40f, 30f);
            deleteButton.relativePosition = new Vector3(430 - deleteButton.width - 8, 8);
            deleteButton.tooltip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "DeleteButton");

            saveLoadButton = UIUtil.CreateUiButton(this, "", "", new Vector2(80, 30), "");
            saveLoadButton.name = $"{Configuration.ResourcePrefix}SaveLoadFileButton";
            saveLoadButton.text = UISaveWindow.instance != null ? Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ExportButton") : Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ImportButton");
            saveLoadButton.tooltip = UISaveWindow.instance != null ? Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ExportButton") : Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ImportButton");
            saveLoadButton.size = new Vector2(80f, 30f);
            saveLoadButton.relativePosition = new Vector3(deleteButton.relativePosition.x - saveLoadButton.width - 8, 8);

            saveLoadButton.eventClicked += (c, p) =>
            {
                if (UISaveWindow.instance != null)
                {
                    UISaveWindow.Export(fileNameLabel.text);
                }
                else
                {
                    UILoadWindow.Close();
                    Singleton<ParallelRoadTool>.instance.Import(fileNameLabel.text);
                    
                }
            };

            deleteButton.eventClicked += (c, p) =>
            {
                ConfirmPanel.ShowModal("Delete file", "Do you want to delete the file '" + fileNameLabel.text + "' permanently?", (comp, ret) =>
                {
                    if (ret == 1)
                    {
                        Singleton<ParallelRoadTool>.instance.Delete(fileNameLabel.text);

                        if (UISaveWindow.instance != null)
                        {
                            UISaveWindow.instance.RefreshFileList();
                        }
                        else
                        {
                            UILoadWindow.instance.RefreshFileList();
                        }
                    }
                });
            };

            fileNameLabel.width = saveLoadButton.relativePosition.x - 16f;
        }

        public void Display(string data, int i)
        {
            fileNameLabel.text = data;

            if (i % 2 == 1)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
                background.width = parent.width;
            }
            else
            {
                background.backgroundSprite = null;
            }
        }
        public void Select(bool isRowOdd)
        {

        }
        public void Deselect(bool isRowOdd)
        {

        }
    }
}