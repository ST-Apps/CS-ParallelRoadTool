using AlgernonCommons.Translation;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Shared;

namespace ParallelRoadTool.UI.Presets
{
    public class UISavePresetWindow : UIModalWindow
    {
        #region Unity

        #region Lifecycle

        public UISavePresetWindow() : base("Parallel")
        {
            Container.AddUIComponent<UILabel>().text = "TESTTESTTEST";
        }

        #endregion

        #endregion

        #region Properties

        protected override string PanelTitle => Translations.Translate("LABEL_SAVE_PRESET_WINDOW_TITLE");

        #endregion
    }
}
