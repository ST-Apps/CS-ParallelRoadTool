using System;
using System.IO;
using System.Linq;
using ColossalFramework.UI;
using NetworkSkins.Meshes;
using NetworkSkins.Props;
using ParallelRoadTool;
using UnityEngine;

namespace NetworkSkins.UI
{
    public class UIParallelRoadToolPanel : UIPanel
    {
        public const string Atlas = "ParallelRoadToolSprites";
        private UITextureAtlas m_atlas;

        public const int PaddingTop = 9;
        public const int Padding = 7;
        public const int PagesPadding = 10;
        public const int TabHeight = 32;
        public const int PageHeight = 140;
        public const int Width = 360;
        
        private UIDragHandle _titleBar;

        //private INetToolWrapper _netToolWrapper;
        private NetInfo _selectedPrefab;
        private NetInfo[] _subPrefabs;

        public override void Awake()
        {
            base.Awake();

            Debug.Log("Begin UIParallelRoadToolPanel.Awake");

            LoadSprites();

            this.backgroundSprite = "MenuPanel2";
            this.width = Width + 2 * Padding;            

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
            
            this.AddUIComponent<UINetTypeOption>();
            this.AddUIComponent<UILightDistanceOption>();

            this.FitChildren();

            //_netToolWrapper = NetUtil.GenerateNetToolWrapper();
            //if (_netToolWrapper == null) throw new Exception("NetworkSkins Error: NetToolWrapper is null!");

            Debug.Log("End UIParallelRoadToolPanel.Awake");
        }

        public override void Start()
        {
            absolutePosition = new Vector3(Mathf.Floor((GetUIView().GetScreenResolution().x - width - 50)), Mathf.Floor((GetUIView().GetScreenResolution().y - height - 50)));
        }      

        /// <summary>
        /// Shows and populate the panel if NetTool is active, hide if not.
        /// </summary>
        public override void Update()
        {
            base.Update();

            /*
            // Fine Road Heights Net Tool support
            PrefabInfo newSelectedPrefab = _netToolWrapper.GetCurrentPrefab();

            if (newSelectedPrefab != null)
            {
                if (_selectedPrefab == newSelectedPrefab) return;
                _selectedPrefab = newSelectedPrefab;

                var newSubPrefabs = NetUtil.GetSubPrefabs(_selectedPrefab);

                if (_subPrefabs != null && _subPrefabs.SequenceEqual(newSubPrefabs)) return;
                _subPrefabs = newSubPrefabs;

                var visibleTabCount = 0;
                var firstVisibleIndex = -1;

                var requiredHeight = 0;

                // Populate tabs and options
                for (var i = 0; i < _subPrefabs.Length; i++)
                {
                    var tabName = NetUtil.NET_TYPE_NAMES[i];

                    if (_subPrefabs[i] != null)
                    {
                        if (firstVisibleIndex < 0) firstVisibleIndex = i;

                        var visibleOptionCount = 0;

                        foreach (var component in _netTypePages[i].components)
                        {
                            var option = component as UIOption;
                            if (option == null) continue;

                            // Pass the current prefab to the context-sensitive option
                            if (option.Populate(_subPrefabs[i])) visibleOptionCount++;
                        }

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
                    }
                    else
                    {
                        // Hide unrelevant tabs
                        _tabstrip.HideTab(tabName);
                    }
                }

                if (_subPrefabs[_tabstrip.selectedIndex] == null)
                {
                    if (_subPrefabs[(int)NetType.Ground] != null)
                    {
                        _tabstrip.selectedIndex = (int)NetType.Ground;
                    }
                    else if (firstVisibleIndex >= 0)
                    {
                        _tabstrip.selectedIndex = firstVisibleIndex;
                    }
                }
                else
                {
                    _tabstrip.selectedIndex = _tabstrip.selectedIndex;
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
            */
        }

        private void LoadSprites()
        {
            string[] spriteNames = new string[]
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
                "BendingPressed",
                "Snapping",
                "SnappingDisabled",
                "SnappingFocused",
                "SnappingHovered",
                "SnappingPressed",
                "Collision",
                "CollisionDisabled",
                "CollisionFocused",
                "CollisionHovered",
                "CollisionPressed",
                "Grid",
                "GridDisabled",
                "GridFocused",
                "GridHovered",
                "GridPressed"
            };

            m_atlas = ResourceLoader.CreateTextureAtlas("ParallelRoadTool", spriteNames, "ParallelRoadTool.Icons.");

            UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[]
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
