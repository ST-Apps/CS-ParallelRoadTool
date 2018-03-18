using UnityEngine;

using ColossalFramework.Math;

using FineRoadAnarchy.Redirection;
using ColossalFramework;

namespace FineRoadAnarchy.Detours
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
