using System.Collections.Generic;
using ParallelRoadTool.Redirection;

namespace ParallelRoadTool.EventArgs
{
    public class NetworksConfigurationChangedEventArgs : System.EventArgs
    {
        public IEnumerable<Tuple<NetInfo, float>> NetworkConfigurations { get; set; }
    }
}