using System.Reflection;

namespace FineRoadTool
{
    public class RoadAIWrapper
    {
        private NetAI m_ai;

        private FieldInfo m_elevated;
        private FieldInfo m_bridge;
        private FieldInfo m_slope;
        private FieldInfo m_tunnel;
        private FieldInfo m_invisible;

        public RoadAIWrapper(NetAI ai)
        {
            m_ai = ai;

            try
            {
                m_elevated = m_ai.GetType().GetField("m_elevatedInfo");
                m_bridge = m_ai.GetType().GetField("m_bridgeInfo");
                m_slope = m_ai.GetType().GetField("m_slopeInfo");
                m_tunnel = m_ai.GetType().GetField("m_tunnelInfo");
                m_invisible = m_ai.GetType().GetField("m_invisible");
            }
            catch
            {
                m_elevated = null;
                m_bridge = null;
                m_slope = null;
                m_tunnel = null;
                m_invisible = null;
            }
        }

        public bool hasElevation
        {
            get { return m_elevated != null && m_bridge != null && m_slope != null && m_tunnel != null; }
        }

        public NetInfo info
        {
            get { return m_ai.m_info; }
            set { m_ai.m_info = value; }
        }

        public NetInfo elevated
        {
            get { return hasElevation ? m_elevated.GetValue(m_ai) as NetInfo : null; }
            set
            {
                if (!hasElevation) return;
                m_elevated.SetValue(m_ai, value);
            }
        }

        public NetInfo bridge
        {
            get { return hasElevation ? m_bridge.GetValue(m_ai) as NetInfo : null; }
            set
            {
                if (!hasElevation) return;
                m_bridge.SetValue(m_ai, value);
            }
        }

        public NetInfo slope
        {
            get { return hasElevation ? m_slope.GetValue(m_ai) as NetInfo : null; }
            set
            {
                if (!hasElevation) return;
                m_slope.SetValue(m_ai, value);
            }
        }

        public NetInfo tunnel
        {
            get { return hasElevation ? m_tunnel.GetValue(m_ai) as NetInfo : null; }
            set
            {
                if (!hasElevation) return;
                m_tunnel.SetValue(m_ai, value);
            }
        }

        public bool IsInvisible()
        {
            if (m_invisible != null) return (bool)m_invisible.GetValue(m_ai);

            return false;
        }
    }
}