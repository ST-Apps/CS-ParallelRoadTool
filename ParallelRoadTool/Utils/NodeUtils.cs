namespace ParallelRoadTool.Utils;

internal static class NodeUtils
{
    /// <summary>
    ///     Retrieves the <see cref="NetNode" /> with the given id.
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public static ref NetNode FromId(ushort nodeId)
    {
        return ref NetManager.instance.m_nodes.m_buffer[nodeId];
    }
}
