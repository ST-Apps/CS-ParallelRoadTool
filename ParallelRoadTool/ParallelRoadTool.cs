using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.Extensions.LocaleModels;
using ParallelRoadTool.UI;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Mod's "launcher" class.
    ///     It also acts as a "controller" to connect the mod with its UI.
    /// </summary>
    public class ParallelRoadTool : MonoBehaviour
    {
        public const string SettingsFileName = "ParallelRoadTool";
        public static readonly string SaveFolder = Path.Combine(DataLocation.localApplicationData, "ParallelRoadToolExports");

        public static ParallelRoadTool Instance;

        public static readonly List<NetInfo> AvailableRoadTypes = new List<NetInfo>();
        public static string[] AvailableRoadNames;
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
        
        private void AddNetworkType(NetInfo net, int index = 0)
        {
            AvailableRoadTypes.Add(net);
            AvailableRoadNames[index] = net.GenerateBeautifiedNetName();
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

                var count = PrefabCollection<NetInfo>.PrefabCount();
                AvailableRoadTypes.Clear();
                AvailableRoadNames = new string[count+1];

                // Default item, creates a net with the same type as source
                AddNetworkType(null);
                var addedNetworksCount = 1;

                for (uint i = 0; i < count; i++)
                {
                    var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null) AddNetworkType(prefab, addedNetworksCount++);
                }

                DebugUtils.Log($"Loaded {AvailableRoadTypes.Count} networks.");

                for(var i=0; i < AvailableRoadTypes.Count; i++)
                {
                    DebugUtils.Log($"ROAD: {AvailableRoadTypes[i].GenerateBeautifiedNetName()} | NAME: {AvailableRoadNames[i]}");
                }

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
                if (UIView.HasModalInput() || UIView.HasInputFocus() || !IsToolActive) return;
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

        public bool Export(string filename)
        {
            string path = Path.Combine(SaveFolder, filename + ".xml");
            Directory.CreateDirectory(SaveFolder);

            List<PresetNetItem> PresetItems = SelectedRoadTypes.Select(NetTypeItem => new PresetNetItem {HorizontalOffset = NetTypeItem.HorizontalOffset, IsReversed = NetTypeItem.IsReversed, NetName = NetTypeItem.NetInfo.name, VerticalOffset = NetTypeItem.VerticalOffset}).ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));

            try
            {
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(path))
                {
                    xmlSerializer.Serialize(streamWriter, PresetItems);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't export networks");
                DebugUtils.LogException(e);

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Export failed", "The networks couldn't be exported to '" + path + "'\n\n" + e.Message, true);
                return false;
            }
            return true;
        }

        public void Import(string filename)
        {
            string path = Path.Combine(SaveFolder, filename + ".xml");
            var PresetItems = new List<PresetNetItem>();

            var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));

            try
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(path))
                {
                    PresetItems = (List<PresetNetItem>)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't import networks");
                DebugUtils.LogException(e);

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Import failed", "The networks couldn't be imported from '" + path + "'\n\n" + e.Message, true);
            }

            var netTypeItems = new List<NetTypeItem>();
            SelectedRoadTypes.Clear();
            foreach (PresetNetItem preset in PresetItems)
            {
                NetInfo netInfo;

                netInfo = PrefabCollection<NetInfo>.FindLoaded(preset.NetName);
                if (netInfo != null)
                {
                    DebugUtils.Log("Adding network:" + netInfo.name);
                    var n = new NetTypeItem(netInfo, preset.HorizontalOffset, preset.VerticalOffset, preset.IsReversed);
                    netTypeItems.Add(n);
                    SelectedRoadTypes.Add(n);
                }
                else
                {
                    //TODO action for missing networks needed here
                }
            }
            DebugUtils.Log("Network count: " + netTypeItems.Count);
            //_mainWindow._netList.List.Clear();
            _mainWindow._netList.List = netTypeItems;
            _mainWindow._netList.RenderList();
            _mainWindow._netList.Changed();
        }

        public void Delete(string filename)
        {
            try
            {
                string path = Path.Combine(SaveFolder, filename + ".xml");

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Couldn't delete file");
                DebugUtils.LogException(ex);

                return;
            }
        }
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

            LocalizationManager.LoadLocalization();                
        }

        public override void OnReleased()
        {
            LocalizationManager.UnloadLocalization();
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
