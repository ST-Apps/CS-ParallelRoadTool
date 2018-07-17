using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ColossalFramework.Plugins;
using UnityEngine;

namespace ParallelRoadTool
{
    public class DebugUtils
    {
        public const string modPrefix = "[Parallel Road Tool " + ModInfo.Version + "] ";

        private static readonly string[] _allowedMethodsNames = new []{ "CreateSegment" };

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

        public static void Log(string message, [CallerMemberNameAttribute] string callerName = null)
        {
#if DEBUG            
            if (_allowedMethodsNames.Any() && !_allowedMethodsNames.Contains(callerName)) return;
            if (message == m_lastLog)
            {
                m_duplicates++;
            }
            else if (m_duplicates > 0)
            {
                Debug.Log(modPrefix + " [" + callerName + "] " + m_lastLog + "(x" + (m_duplicates + 1) + ")");
                Debug.Log(modPrefix + " [" + callerName + "] " + message);
                m_duplicates = 0;
            }
            else
            {
                Debug.Log(modPrefix + "[" + callerName + "] " + message);
            }

            m_lastLog = message;
#endif
        }

        public static void LogException(Exception e)
        {
            Log("Intercepted exception (not game breaking):");
            Debug.LogException(e);
        }

#if DEBUG
        private static string m_lastLog;
        private static int m_duplicates;
#endif
    }
}