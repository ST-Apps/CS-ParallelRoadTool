using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.Wrappers
{
    public class NetToolWrapper
    {

        private readonly NetTool m_netTool;

        private readonly FieldInfo _controlPointCount;
        private readonly FieldInfo _controlPoints;

        public NetToolWrapper(NetTool netTool)
        {
            m_netTool = netTool;

            try
            {
                _controlPointCount = m_netTool.GetType().GetField("m_controlPointCount", BindingFlags.Instance | BindingFlags.NonPublic);
                _controlPoints = m_netTool.GetType().GetField("m_controlPoints", BindingFlags.Instance | BindingFlags.NonPublic);

                DebugUtils.Log($"Bound to {_controlPointCount}, {_controlPoints}");
            }
            catch (Exception e)
            {
                DebugUtils.LogException(e);

                _controlPointCount = null;
                _controlPoints = null;
            }
        }

        public int m_controlPointCount
        {
            get => (int) _controlPointCount.GetValue(m_netTool);
            set => _controlPointCount.SetValue(m_netTool, value);
        }

        public NetTool.ControlPoint[] m_controlPoints
        {
            get => (NetTool.ControlPoint[])_controlPoints.GetValue(m_netTool);
            set => _controlPoints.SetValue(m_netTool, value);
        }
    }
}
