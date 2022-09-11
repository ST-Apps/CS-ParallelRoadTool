using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelRoadTool.Models
{
    public class CurrentToolChangedEventArgs : EventArgs
    {
        public ToolBase Tool { get; private set; }

        public CurrentToolChangedEventArgs(ToolBase tool)
        {
            Tool = tool;
        }
    }
}
