using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.Extensions.LocaleModels;
using ParallelRoadTool.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Mod's "launcher" class.
    ///     It also acts as a "controller" to connect the mod with its UI.
    ///     TODO: Drag & drop is not as smooth as before.
    /// </summary>
    public class ParallelRoadTool : MonoBehaviour
    {
        public const string SettingsFileName = "ParallelRoadTool";

        public static ParallelRoadTool Instance;

        public static readonly List<NetInfo> AvailableRoadTypes = new List<NetInfo>();
        public static readonly List<NetTypeItem> SelectedRoadTypes = new List<NetTypeItem>();
        public static NetTool NetTool;
        public static bool IsInGameMode;

        private UIMainWindow _mainWindow;

        private bool _isToolActive;
        public bool IsToolActive
        {
            get => _isToolActive && NetTool != null && NetTool.enabled;

            private set
            {
                if (IsToolActive == value) return;
                if (value)
                {
                    DebugUtils.Log("Enabling parallel road support");
                    NetManagerDetour.Deploy();
                    NetToolDetour.Deploy();
                }
                else
                {
                    DebugUtils.Log("Disabling parallel road support");
                    NetManagerDetour.Revert();
                    NetToolDetour.Revert();
                }

                _isToolActive = value;
            }
        }

        public bool IsSnappingEnabled { get; set; }

        public bool IsLeftHandTraffic;        

        #region Utils

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            var index = 0;
            foreach (var item in SelectedRoadTypes)
            {
                if (isHorizontal)
                    item.HorizontalOffset += (1 + index) * step;
                else
                    item.VerticalOffset += (1 + index) * step;
                index++;
            }

            _mainWindow.RenderNetList();
        }     

        #endregion

        #region Handlers

        private void UnsubscribeToUIEvents()
        {
            _mainWindow.OnParallelToolToggled -= MainWindowOnOnParallelToolToggled;
            _mainWindow.OnNetworksListCountChanged -= MainWindowOnOnNetworksListCountChanged;
        }

        private void SubscribeToUIEvents()
        {
            _mainWindow.OnParallelToolToggled += MainWindowOnOnParallelToolToggled;
            _mainWindow.OnNetworksListCountChanged += MainWindowOnOnNetworksListCountChanged;
            _mainWindow.OnSnappingToggled += MainWindowOnOnSnappingToggled;
        }

        private void MainWindowOnOnSnappingToggled(UIComponent component, bool value)
        {
            IsSnappingEnabled = value;
        }

        private void MainWindowOnOnNetworksListCountChanged(object sender, System.EventArgs e)
        {
            NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
        }

        private void MainWindowOnOnParallelToolToggled(UIComponent component, bool value)
        {
            IsToolActive = value;

            if (value && ToolsModifierControl.advisorPanel)
                _mainWindow.ShowTutorial();
        }

        #endregion

        #region Unity

        public void Start()
        {
            // Find NetTool and deploy
            try
            {
                NetTool = FindObjectOfType<NetTool>();
                if (NetTool == null)
                {
                    DebugUtils.Log("Net Tool not found");
                    enabled = false;
                    return;
                }

                // Available networks loading
                DebugUtils.Log("Loading all available networks.");

                AvailableRoadTypes.Clear();

                var count = PrefabCollection<NetInfo>.PrefabCount();

                // Default item, creates a net with the same type as source
                AvailableRoadTypes.Add(null);

                for (uint i = 0; i < count; i++)
                {
                    var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null) AvailableRoadTypes.Add(prefab);
                }

                DebugUtils.Log($"Loaded {AvailableRoadTypes.Count} networks.");

                IsLeftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;

                DebugUtils.Log($"IsLeftHandTraffic = {IsLeftHandTraffic}");

                NetManagerDetour.Deploy();
                NetToolDetour.Deploy();

                // Main UI init
                var view = UIView.GetAView();
                _mainWindow = view.FindUIComponent<UIMainWindow>("PRT_MainWindow");
                if (_mainWindow != null)
                    Destroy(_mainWindow);

                DebugUtils.Log("Adding UI components");
                _mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

                SubscribeToUIEvents();

                DebugUtils.Log("Initialized");
            }
            catch (Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
            }
        }        

        public void OnDestroy()
        {
            try
            {
                DebugUtils.Log("Destroying ...");

                NetManagerDetour.Revert();

                UnsubscribeToUIEvents();
                AvailableRoadTypes.Clear();
                SelectedRoadTypes.Clear();
                IsToolActive = false;
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;
                _mainWindow.OnDestroy();
                _mainWindow = null;
            }
            catch {
                // HACK - [ISSUE 31]
            }
        }

        public void OnGUI()
        {
            try
            {
                if (UIView.HasModalInput() || UIView.HasInputFocus()) return;
                var e = Event.current;

                // Checking key presses
                if (OptionsKeymapping.toggleParallelRoads.IsPressed(e)) _mainWindow.ToggleToolCheckbox();

                if (OptionsKeymapping.decreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);

                if (OptionsKeymapping.increaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);

                if (OptionsKeymapping.decreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);

                if (OptionsKeymapping.increaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        #endregion
    }

    public class ParallelRoadToolLoader : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            // Re-instantiate mod if recompiled after level has been loaded. Useful for UI development, but breaks actual building!
            /*if (loading.loadingComplete)
            {
                ParallelRoadTool.Instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
            }*/

            // Set current game mode, we can't load some stuff if we're not in game (e.g. Map Editor)
            ParallelRoadTool.IsInGameMode = loading.currentMode == AppMode.Game;

            // Add post locale change event handlers
            LocaleManager.eventLocaleChanged += OnLocaleChanged;

            DebugUtils.Log("Added locale change event handlers.");

            // Reload the current locale once to effect changes
            LocaleManager.ForceReload();            
        }

        public override void OnReleased()
        {
            // Remove post locale change event handlers
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;

            DebugUtils.Log("Removed locale change event handlers.");

            // Reload the current locale once to effect changes
            LocaleManager.ForceReload();
        }

        private void OnLocaleChanged()
        {
            DebugUtils.Log("Locale changed callback started.");

            XmlSerializer serializer = new XmlSerializer(typeof(NameList));

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"ParallelRoadTool.Localization.{LocaleManager.cultureInfo.TwoLetterISOLanguageName}.xml";

            if (!assembly.GetManifestResourceNames().Contains(resourceName))
            {
                // Fallback to english
                resourceName = "ParallelRoadTool.Localization.en.xml";
            }

            DebugUtils.Log($"Trying to read {resourceName} localization file...");            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                using (XmlReader xmlStream = XmlReader.Create(reader))
                {
                    if (serializer.CanDeserialize(xmlStream))
                    {
                        NameList nameList = (NameList)serializer.Deserialize(xmlStream);
                        nameList.Apply();
                    }
                }
            }

            DebugUtils.Log($"Namelists {resourceName} applied.");

            DebugUtils.Log("Locale changed callback finished.");
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (ParallelRoadTool.Instance == null)
                ParallelRoadTool.Instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
            else
                ParallelRoadTool.Instance.Start();            
        }
    }
}