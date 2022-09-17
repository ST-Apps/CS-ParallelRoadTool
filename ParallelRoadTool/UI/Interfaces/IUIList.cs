using System.Collections.Generic;

namespace ParallelRoadTool.UI.Interfaces
{
    internal class UIList<T> : List<T> where T : IUIListabeItem
    {
    }
}
