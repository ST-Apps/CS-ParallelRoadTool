using UnityEngine;

using ColossalFramework.Math;

using ParallelRoadTool.Redirection;
using ColossalFramework;

namespace ParallelRoadTool.Detours
{
    [TargetType(typeof(NetAI))]
    public unsafe class NetAIDetour : NetAI
    {
        [RedirectMethod]
        public override bool BuildOnWater()
        {
            return true;
        }
    }
}
