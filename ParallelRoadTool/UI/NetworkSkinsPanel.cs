using System;
using System.IO;
using System.Linq;
using ColossalFramework.UI;
using ParallelRoadTool;
using ParallelRoadTool.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace NetworkSkins.UI
{
    public class UINetworkSkinsPanel : UIPanel
    {
        public const string Atlas = "NetworkSkinsSprites";
        private UITextureAtlas m_atlas;

        public const int PaddingTop = 9;
        public const int Padding = 7;
        public const int PagesPadding = 10;
        public const int TabHeight = 32;
        public const int PageHeight = 140;//300;
        public const int Width = 360;//310;

        private UIDragHandle _titleBar;
        private UITabstrip _tabstrip;

        // The panels containing the net options (e.g. dropdowns)
        private UIPanel[] _netTypePages;

        private NetInfo _selectedPrefab;
        private NetInfo[] _subPrefabs;

        public override void Awake()
        {
            base.Awake();

            Debug.Log("Begin UINetworkSkinsPanel.Awake");

            LoadResources();

            this.backgroundSprite = "MenuPanel2";
            this.width = Width + 2 * Padding;
            //this.padding = new RectOffset(PADDING, 0, PADDING, 0);

            _titleBar = this.AddUIComponent<UIDragHandle>();
            _titleBar.name = "TitlePanel";
            _titleBar.width = this.width;
            _titleBar.height = TabHeight + PaddingTop;
            _titleBar.target = this;
            _titleBar.relativePosition = new Vector3(0, 0);

            // display a drag cursor sprite in the top right corner of the panel
            var dragSprite = _titleBar.AddUIComponent<UISprite>();
            dragSprite.atlas = m_atlas;
            dragSprite.spriteName = "DragCursor";
            dragSprite.relativePosition = new Vector3(Width - 20, PaddingTop + 1);
            dragSprite.MakePixelPerfect();

            _tabstrip = _titleBar.AddUIComponent<UITabstrip>();
            _tabstrip.relativePosition = new Vector3(Padding, PaddingTop, 0);
            _tabstrip.width = Width;
            _tabstrip.height = TabHeight;
            _tabstrip.tabPages = this.AddUIComponent<UITabContainer>();
            _tabstrip.tabPages.width = this.width;
            _tabstrip.tabPages.height = PageHeight;
            _tabstrip.tabPages.relativePosition = new Vector3(0, _titleBar.height);
            _tabstrip.tabPages.padding = new RectOffset(PagesPadding, PagesPadding, PagesPadding, PagesPadding);
            _tabstrip.padding.right = 0;

            // Add 4 tabs and 4 pages
            var keyMappingTabstrip = GameObject.Find("EconomyTabstrip").GetComponent<UITabstrip>();
            var buttonTemplate = keyMappingTabstrip.GetComponentInChildren<UIButton>();

            _netTypePages = new UIPanel[1];

            for (var i = 0; i < 1; i++)
            {
                var tab = _tabstrip.AddTab("Networks", buttonTemplate, true);
                tab.textPadding.top = 8;
                tab.textPadding.bottom = 8;
                tab.textPadding.left = 10;
                tab.textPadding.right = 10;
                tab.autoSize = true;
                tab.textScale = .9f;
                tab.playAudioEvents = buttonTemplate.playAudioEvents;

                tab.pressedTextColor = new Color32(255, 255, 255, 255);
                tab.focusedTextColor = new Color32(255, 255, 255, 255);
                tab.focusedColor = new Color32(205, 205, 205, 255);
                tab.disabledTextColor = buttonTemplate.disabledTextColor;

                var page = _tabstrip.tabPages.components.Last() as UIPanel;
                page.autoLayoutDirection = LayoutDirection.Vertical;
                page.autoLayoutPadding = new RectOffset(0, 0, 0, Padding);
                page.autoLayout = true;
                page.isVisible = false;

                // TODO add scrolling + autofitting

                _netTypePages[i] = page;
            }

            this.FitChildren();

            // Add some example options
            GetPage(0).AddUIComponent<UINetTypeOption>();
            //GetPage(NetType.Ground).AddUIComponent<UILightDistanceOption>();
            //GetPage(NetType.Ground).AddUIComponent<UITreeOption>().LanePosition = LanePosition.Left;
            ////GetPage(NetType.Ground).AddUIComponent<UITreeDistanceOption>().LanePosition = LanePosition.Left;
            //GetPage(NetType.Ground).AddUIComponent<UITreeOption>().LanePosition = LanePosition.Middle;
            ////GetPage(NetType.Ground).AddUIComponent<UITreeDistanceOption>().LanePosition = LanePosition.Middle;
            //GetPage(NetType.Ground).AddUIComponent<UITreeOption>().LanePosition = LanePosition.Right;
            ////GetPage(NetType.Ground).AddUIComponent<UITreeDistanceOption>().LanePosition = LanePosition.Right;

            //GetPage(NetType.Elevated).AddUIComponent<UILightOption>();
            ////GetPage(NetType.Elevated).AddUIComponent<UILightDistanceOption>();
            //GetPage(NetType.Elevated).AddUIComponent<UIPillarOption>().PillarType = PillarType.BridgePillar;
            ////GetPage(NetType.ELEVATED).AddUIComponent<UIPillarOption>().PillarType = PillarType.MIDDLE_PILLAR;

            //GetPage(NetType.Bridge).AddUIComponent<UILightOption>();
            ////GetPage(NetType.Bridge).AddUIComponent<UILightDistanceOption>();
            //GetPage(NetType.Bridge).AddUIComponent<UIPillarOption>().PillarType = PillarType.BridgePillar;
            ////GetPage(NetType.BRIDGE).AddUIComponent<UIPillarOption>().PillarType = PillarType.MIDDLE_PILLAR;
            ////GetPage(NetType.BRIDGE).AddUIComponent<UIBridgeTypeOption>();

            _tabstrip.startSelectedIndex = (int)0;

            DebugUtils.Log("End UINetworkSkinsPanel.Awake");
        }

        public override void Start()
        {
            absolutePosition = new Vector3(Mathf.Floor((GetUIView().GetScreenResolution().x - width - 50)), Mathf.Floor((GetUIView().GetScreenResolution().y - height - 50)));
        }

        private UIPanel GetPage(int netType)
        {
            return _netTypePages[(int)netType];
        }

        /// <summary>
        /// Shows and populate the panel if NetTool is active, hide if not.
        /// </summary>
        public override void Update()
        {
            base.Update();

            DebugUtils.Log($"NetworkSkinsPanel.Update() - {ParallelRoadTool.ParallelRoadTool.instance.m_netTool}");

            if (ParallelRoadTool.ParallelRoadTool.instance.m_netTool.enabled)
            {
                var visibleTabCount = 0;

                var requiredHeight = 0;

                // Populate tabs and options
                var tabName = "Networks"; //NetUtil.NET_TYPE_NAMES[i];

                var visibleOptionCount = _netTypePages[0].components.OfType<UIOption>().Count();
                _netTypePages[0].components.ForEach(c => c.enabled = true);

                if (visibleOptionCount > 0)
                {
                    visibleTabCount++;
                    requiredHeight = Math.Max(requiredHeight, visibleOptionCount * (30 + Padding));
                    _tabstrip.ShowTab(tabName);
                }
                else
                {
                    _tabstrip.HideTab(tabName);
                }

                isVisible = visibleTabCount > 0;

                _tabstrip.tabPages.height = requiredHeight + 2 * PagesPadding;
                this.FitChildren();

                return;
            }

            if (isVisible)
            {
                isVisible = false;
                _selectedPrefab = null;
                _subPrefabs = null;
            }
        }

        private void LoadResources()
        {
            string[] spriteNames =
            {
                "Anarchy",
                "AnarchyDisabled",
                "AnarchyFocused",
                "AnarchyHovered",
                "AnarchyPressed",
                "Bending",
                "BendingDisabled",
                "BendingFocused",
                "BendingHovered",
                "BendingPressed"
            };

            m_atlas = ResourceLoader.CreateTextureAtlas("ParallelRoadTool", spriteNames, "ParallelRoadTool.Icons.");

            var defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures =
            {
                defaultAtlas["OptionBase"].texture,
                defaultAtlas["OptionBaseFocused"].texture,
                defaultAtlas["OptionBaseHovered"].texture,
                defaultAtlas["OptionBasePressed"].texture,
                defaultAtlas["OptionBaseDisabled"].texture
            };

            ResourceLoader.AddTexturesInAtlas(m_atlas, textures);
        }
    }
}
