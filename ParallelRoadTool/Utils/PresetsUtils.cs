using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Patches;
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
                Log.Info($"[{nameof(PresetsUtils)}.{nameof(Export)}] Failed exporting {filename} to {path}");
                Log.Exception(e);

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                      .SetMessage(
                                  Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ExportFailedTitle"),
                                  string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "ExportFailedMessage"), path),
                                  true);
                return false;
            }

            Log.Info($"[{nameof(PresetsUtils)}.{nameof(Export)}] Exported {filename} to {path}");
            return true;
        }

        public static void Import(string filename)
        {
            var path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            if (!File.Exists(path))
            {
                Log.Info($"[{nameof(PresetsUtils)}.{nameof(Import)}] No auto-save file found in path {path}");

                return;
            }

            Log.Info($"[{nameof(PresetsUtils)}.{nameof(Import)}] Loading auto-saved networks from file {path}");

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
                    if (netInfo == null)
                    {
                        Log.Info($"[{nameof(PresetsUtils)}.{nameof(Import)}] Couldn't import network with name {preset.NetName} from preset {filename}");

                        continue;
                    }

                    Log.Info($"[{nameof(PresetsUtils)}.{nameof(Import)}] Adding network {netInfo.name} from preset {filename}");

                    var item = new NetTypeItem(netInfo, preset.HorizontalOffset, preset.VerticalOffset,
                                               preset.IsReversed);
                    Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Add(item);
                    Singleton<ParallelRoadTool>.instance.AddItem(item);
                    NetManagerPatch.NetworksCount = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Count;
                }
            }
            catch (Exception e)
            {
                Log.Info($"[{nameof(PresetsUtils)}.{nameof(Import)}] Failed importing {filename} from {path}");
                Log.Exception(e);

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
                Log.Info($"[{nameof(PresetsUtils)}.{nameof(Delete)}] Failed deleting {filename} from {path}");
                Log.Exception(e);

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                      .SetMessage(
                                  Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DeleteFailedTitle"), 
                                  string.Format(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DeleteFailedMessage"), path),
                                  true);
            }
        }
    }
}
