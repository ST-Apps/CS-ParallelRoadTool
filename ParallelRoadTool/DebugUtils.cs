using System;
using ColossalFramework.Plugins;
using UnityEngine;

namespace ParallelRoadTool
{
    public class DebugUtils
    {
        public const string modPrefix = "[Parallel Road Tool " + ModInfo.Version + "] ";

        private static string m_lastLog;
        private static int m_duplicates;

        public static void Message(string message)
        {
            Log(message);
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, modPrefix + message);
        }

        public static void Warning(string message)
        {
            Debug.LogWarning(modPrefix + message);
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, modPrefix + message);
        }

        public static void Log(string message)
        {
#if DEBUG
            if (message == m_lastLog)
            {
                m_duplicates++;
            }
            else if (m_duplicates > 0)
            {
                Debug.Log(modPrefix + m_lastLog + "(x" + (m_duplicates + 1) + ")");
                Debug.Log(modPrefix + message);
                m_duplicates = 0;
            }
            else
            {
                Debug.Log(modPrefix + message);
            }

            m_lastLog = message;
#endif
        }

        public static void LogException(Exception e)
        {
            Log("Intercepted exception (not game breaking):");
            Debug.LogException(e);
        }
    }
}