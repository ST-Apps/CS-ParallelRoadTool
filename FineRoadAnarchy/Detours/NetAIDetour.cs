
using ColossalFramework;
using ColossalFramework.Math;
using System;

using UnityEngine;

using FineRoadAnarchy.Redirection;

namespace FineRoadAnarchy.Detours
{
    [TargetType(typeof(NetAI))]
    public class NetAIDetour : NetAI
    {
        [RedirectMethod]
        public override bool BuildOnWater()
        {
            
                return true;
            
        }
        
    }
}
