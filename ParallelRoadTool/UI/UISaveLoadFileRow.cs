using UnityEngine;
using ColossalFramework.UI;

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

            deleteButton = UIUtils.CreateButton(this);
            deleteButton.name = "ParallelRoadTool_DeleteFileButton";
            deleteButton.text = "X";
            deleteButton.size = new Vector2(40f, 30f);
            deleteButton.relativePosition = new Vector3(430 - deleteButton.width - 8, 8);
            deleteButton.tooltip = "Delete saved networks";

            saveLoadButton = UIUtils.CreateButton(this);
            saveLoadButton.name = "ParallelRoadTool_SaveLoadFileButton";
            saveLoadButton.text = UISaveWindow.instance != null ? "Export" : "Import";
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
                    ParallelRoadTool.Instance.Import(fileNameLabel.text);
                }
            };

            deleteButton.eventClicked += (c, p) =>
            {
                ConfirmPanel.ShowModal("Delete file", "Do you want to delete the file '" + fileNameLabel.text + "' permanently?", (comp, ret) =>
                {
                    if (ret == 1)
                    {
                        ParallelRoadTool.Instance.Delete(fileNameLabel.text);

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