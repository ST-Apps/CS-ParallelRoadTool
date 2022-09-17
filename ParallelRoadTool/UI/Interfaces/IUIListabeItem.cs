namespace ParallelRoadTool.UI.Interfaces
{

    /// <summary>
    /// Generic UI item that can be added to a UI List.
    /// </summary>
    /// <typeparam name="TObject">Type of the data object to be rendered.</typeparam>
    internal interface IUIListabeItem
    {
        string Id { get; set; }
    }

    /// <summary>
    /// Generic UI item that can be added to a UI List.
    /// </summary>
    /// <typeparam name="TObject">Type of the data object to be rendered.</typeparam>
    internal interface IUIListabeItem<TObject> : IUIListabeItem
    {
        // TODO: add abstract class to extend UIComponent and handle click events for selection
        void Render(TObject netInfo);
    }
}
