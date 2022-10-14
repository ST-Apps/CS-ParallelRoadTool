using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;

namespace ParallelRoadTool.Extensions
{
    internal static class LineExtensions
    {

        public static Segment3 ToSegment3(this Line2 line)
        {
            return new Segment3(line.a, line.b);
        }

    }
}
