using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Detours;
using ParallelRoadTool.Models;

namespace ParallelRoadTool.Utils
{
    /// <summary>
    ///     Utility class used to allow presets saving, loading and deleting.
    /// </summary>
    public class PresetsUtils
    {
        public static bool Export(string filename)
        {
            var path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            try
            {
                Directory.CreateDirectory(Configuration.AutoSaveFolderPath);
                var presetItems = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes
                    .Select(i => new PresetNetItem
                    {
                        HorizontalOffset = i.HorizontalOffset,
                        IsReversed = i.IsReversed,
                        NetName = i.NetInfo.name,
                        VerticalOffset = i.VerticalOffset
                    })
                    .ToList();
                var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));
                using (var streamWriter = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(streamWriter, presetItems);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log($"Failed exporting {filename} to {path}");
                DebugUtils.LogException(e);
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                    .SetMessage(
                        Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ExportFailedTitle"),
                        string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ExportFailedMessage"), path),
                        true);
                return false;
            }

            return true;
        }

        public static void Import(string filename)
        {
            var path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            try
            {
                List<PresetNetItem> presetItems;
                var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));
                using (var streamReader = new StreamReader(path))
                {
                    presetItems = (List<PresetNetItem>) xmlSerializer.Deserialize(streamReader);
                }

                Singleton<ParallelRoadTool>.instance.ClearItems();
                Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Clear();
                foreach (var preset in presetItems)
                {
                    var netInfo = PrefabCollection<NetInfo>.FindLoaded(preset.NetName);
                    if (netInfo == null) continue;

                    DebugUtils.Log($"Adding network {netInfo.name} from preset {filename}");
                    var item = new NetTypeItem(netInfo, preset.HorizontalOffset, preset.VerticalOffset,
                        preset.IsReversed);
                    Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Add(item);
                    Singleton<ParallelRoadTool>.instance.AddItem(item);
                    NetManagerDetour.NetworksCount = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Count;
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log($"Failed importing {filename} from {path}");
                DebugUtils.LogException(e);
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                    .SetMessage(
                        Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ImportFailedTitle"),
                        string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ImportFailedMessage"), path),
                        true);
            }
        }

        public static void Delete(string filename)
        {
            var path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                DebugUtils.Log($"Failed deleting {filename} from {path}");
                DebugUtils.LogException(e);
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                    .SetMessage(
                        Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DeleteFailedTitle"),
                        string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DeleteFailedMessage"), path),
                        true);
            }
        }
    }
}