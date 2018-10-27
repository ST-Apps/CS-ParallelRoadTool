using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Detours;
using ParallelRoadTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ParallelRoadTool.Utils
{
    public class PresetsUtils
    {
        public static bool Export(string filename)
        {
            string path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            Directory.CreateDirectory(Configuration.AutoSaveFolderPath);
            List<PresetNetItem> PresetItems = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes
                .Select(NetTypeItem => new PresetNetItem { HorizontalOffset = NetTypeItem.HorizontalOffset, IsReversed = NetTypeItem.IsReversed, NetName = NetTypeItem.NetInfo.name, VerticalOffset = NetTypeItem.VerticalOffset })
                .ToList();
            var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(path))
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

        public static void Import(string filename)
        {
            string path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            var PresetItems = new List<PresetNetItem>();
            var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));
            try
            {
                using (StreamReader streamReader = new StreamReader(path))
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
            Singleton<ParallelRoadTool>.instance.ClearItems();
            Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Clear();
            foreach (PresetNetItem preset in PresetItems)
            {
                NetInfo netInfo;
                netInfo = PrefabCollection<NetInfo>.FindLoaded(preset.NetName);
                if (netInfo != null)
                {
                    DebugUtils.Log("Adding network:" + netInfo.name);
                    var item = new NetTypeItem(netInfo, preset.HorizontalOffset, preset.VerticalOffset, preset.IsReversed);
                    Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Add(item);
                    Singleton<ParallelRoadTool>.instance.AddItem(item);
                    NetManagerDetour.NetworksCount = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Count;
                }
                else
                {
                    //TODO: action for missing networks needed here 
                }
            }
        }

        public static void Delete(string filename)
        {
            string path = Path.Combine(Configuration.AutoSaveFolderPath, filename + ".xml");
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't delete file");
                DebugUtils.LogException(e);
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Delete failed", "Couldn't delete file '" + path + "'\n\n" + e.Message, true);
            }
        }
    }
}
