using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelRoadTool.UI.Interfaces
{
    internal class UIList<T> : List<T> where T : IUIListabeItem
    {
    }
}
