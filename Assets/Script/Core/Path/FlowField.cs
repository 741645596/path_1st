using System.Collections.Generic;

/// <summary>
/// 流畅寻路算法。
/// </summary>
public class FlowField
{
    private static List<MapTile> openSet = new List<MapTile>();
    private static List<MapTile> closedSet = new List<MapTile>();
    private static Map g_map = null;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="map"></param>
    public static void Init(Map map)
    {
        g_map = map;
    }
    /// <summary>
    /// 使用流程寻路算法计算start点到扇区的各个gate，并保存的way point的cost中。
    /// </summary>
    /// <param name="start">sector中的起点</param>
    /// <param name="sector">指定的扇区</param>
    /// <param name="map">指定地图</param>
    public static void CalcSectorWayPointCost(PathNode start)
    {
        openSet.Clear();
        closedSet.Clear();

        openSet.Add(start.tileConnection);
        start.tileConnection.integrationValue = 0;

        while (openSet.Count > 0)
        {
            MapTile currentNode = openSet[0];
            MapTileSearchResult  result = TileHelp.GetAllNeighboursInSectorFlowFieldSearch(currentNode);
            for (int i = 0; i < result.Count; i++)
            {
                MapTile neighbour = result.Get(i);
                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);

                    // if true, there is a higher node here
                    if (neighbour.hasPathNodeConnection)
                    {
                        PathNode neighbourSectorNode = g_map.GetWayPointPathNode(0, neighbour);
                        if (neighbourSectorNode != null )
                        {
                            List<IntVector2> l = GetRoadList(start.tileConnection, neighbour);
                            PathNode.LinkSectorNode(start, neighbourSectorNode, neighbour.integrationValue / 10, l);
                        }
                    }
                }
            }
            closedSet.Add(currentNode);
            openSet.Remove(currentNode);
        }
        // reset
        for (int i = 0; i < openSet.Count; i++)
        {
            openSet[i].integrationValue = TileHelp.tileResetIntegrationValue;
        }
        for (int i = 0; i < closedSet.Count; i++)
        {
            closedSet[i].integrationValue = TileHelp.tileResetIntegrationValue;
        }
    }
    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="end"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    private static List<IntVector2> GetRoadList(MapTile start, MapTile end)
    {
        List<IntVector2> l = new List<IntVector2>();
        MapTile t = TileHelp.GetLowestIntergrationCostTile(end);
        while (t != null && t != start)
        {
            l.Add(t.Pos);
            t = TileHelp.GetLowestIntergrationCostTile(t);
        }
        l.Reverse();
        return l;
    }


    /// <summary>
    /// 在一个扇区内进行流程寻路算法
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="map"></param>
    /// <param name="lSector">指定扇区内</param>
    /// <returns></returns>
    public static List<IntVector2> GetRoadInSector(MapTile start, MapTile end, List<ushort> lSector)
    {
        bool ret = false;
        openSet.Clear();
        closedSet.Clear();

        openSet.Add(start);
        start.integrationValue = 0;

        while (openSet.Count > 0 && ret == false)
        {
            MapTile currentNode = openSet[0];
            MapTileSearchResult result = TileHelp.GetNeighboursExpansionSearch(currentNode, lSector);
            for (int i = 0; i < result.Count; i++)
            {
                MapTile neighbour = result.Get(i);
                if (!openSet.Contains(neighbour))
                {
                    openSet.Add(neighbour);
                    // if true, there is a higher node here
                    if (neighbour == end)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            closedSet.Add(currentNode);
            openSet.Remove(currentNode);
        }
        // 得到路径了，回溯路径
        List<IntVector2>  l = GetRoadList(start, end);
        // reset
        for (int i = 0; i < openSet.Count; i++)
        {
            openSet[i].integrationValue = TileHelp.tileResetIntegrationValue;
        }
        for (int i = 0; i < closedSet.Count; i++)
        {
            closedSet[i].integrationValue = TileHelp.tileResetIntegrationValue;
        }
        return l;
    }
}
