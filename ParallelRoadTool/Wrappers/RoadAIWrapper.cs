using System.Reflection;

namespace ParallelRoadTool.Wrappers
{
    public class RoadAIWrapper
    {
        #region Fields

        private readonly NetAI     m_ai;
        private readonly FieldInfo m_bridge;

        private readonly FieldInfo m_elevated;
        private readonly FieldInfo m_invisible;
        private readonly FieldInfo m_slope;
        private readonly FieldInfo m_tunnel;

        #endregion

        #region Properties

        public bool hasElevation => m_elevated != null && m_bridge != null && m_slope != null && m_tunnel != null;

        public NetInfo info
        {
            get => m_ai.m_info;
            set => m_ai.m_info = value;
        }

        public NetInfo elevated
        {
            get => hasElevation ? m_elevated.GetValue(m_ai) as NetInfo : null;
            set
            {
                if (!hasElevation) return;
                m_elevated.SetValue(m_ai, value);
            }
        }

        public NetInfo bridge
        {
            get => hasElevation ? m_bridge.GetValue(m_ai) as NetInfo : null;
            set
            {
                if (!hasElevation) return;
                m_bridge.SetValue(m_ai, value);
            }
        }

        public NetInfo slope
        {
            get => hasElevation ? m_slope.GetValue(m_ai) as NetInfo : null;
            set
            {
                if (!hasElevation) return;
                m_slope.SetValue(m_ai, value);
            }
        }

        public NetInfo tunnel
        {
            get => hasElevation ? m_tunnel.GetValue(m_ai) as NetInfo : null;
            set
            {
                if (!hasElevation) return;
                m_tunnel.SetValue(m_ai, value);
            }
        }

        #endregion

        public RoadAIWrapper(NetAI ai)
        {
            m_ai = ai;

            try
            {
                m_elevated  = m_ai.GetType().GetField("m_elevatedInfo");
                m_bridge    = m_ai.GetType().GetField("m_bridgeInfo");
                m_slope     = m_ai.GetType().GetField("m_slopeInfo");
                m_tunnel    = m_ai.GetType().GetField("m_tunnelInfo");
                m_invisible = m_ai.GetType().GetField("m_invisible");
            }
            catch
            {
                m_elevated  = null;
                m_bridge    = null;
                m_slope     = null;
                m_tunnel    = null;
                m_invisible = null;
            }
        }

        public bool IsInvisible()
        {
            if (m_invisible != null) return (bool)m_invisible.GetValue(m_ai);

            return false;
        }
    }
}
