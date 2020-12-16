using System.Collections.Generic;

public class IggPathFinder 
{
    /// <summary>
    /// 寻路列表[] :level
    /// </summary>
    private static PathNode[] StartSectorNodes;
    /// <summary>
    /// dest 目标列表[] :level
    /// </summary>
    private static PathNode[] DestSectorNodes;
    /// <summary>
    /// 道路返回结果，做了缓存多个小兵的路径就需要通过拷贝了。
    /// </summary>
    private static RoadResult m_RoadReslut = new RoadResult();


    private static Map g_map = null;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="map"></param>
    public static void Init(Map map)
    {
        g_map = map;
        StartSectorNodes = new PathNode[g_map.SectorLevelNum];
        DestSectorNodes = new PathNode[g_map.SectorLevelNum];
    }
    /// <summary>
    /// 清理数据
    /// </summary>
    public static void Clear()
    {
        StartSectorNodes = null;
        DestSectorNodes = null;
    }
    // find path on higher level between start and goal/destination
    /// <summary>
    /// 群体寻路
    /// </summary>
    /// <param name="startingPoints">寻路起点列表</param>
    /// <param name="destinationTile">目标tile</param>
    /// <returns></returns>
    public static RoadResult FindPaths(MapTile startTile, MapTile destinationTile)
    {
        RemovePreviousSearch();
        SetPathData(startTile, destinationTile);
        List<PathNode> listLine = FindPathRoad(StartSectorNodes, DestSectorNodes);
        m_RoadReslut.processRoad(listLine, destinationTile);
        return m_RoadReslut;
    }


    private static void SetPathData(MapTile startTile, MapTile destinationTile)
    {
        for (int level = 0; level < g_map.SectorLevelNum; level++)
        {
            PathNode s = g_map.GetWayPointPathNode(level, startTile);
            if (s == null)
            {
                MapSector startNodeSector = startTile.GetMapSector(level, g_map);
                PathNode startMultiSectorNode = startNodeSector.CreateWayPointInSector(startTile, g_map);
                startNodeSector.CalcWayPointJoinNode(startMultiSectorNode, g_map);
                StartSectorNodes[level] = startMultiSectorNode;
            }
            else
            {
                StartSectorNodes[level] = s;
            }

            PathNode e = g_map.GetWayPointPathNode(level, destinationTile);
            if (e == null)
            {
                MapSector destinationNodeSector = destinationTile.GetMapSector(level, g_map);
                PathNode destinationMultiSectorNode = destinationNodeSector.CreateWayPointInSector(destinationTile, g_map);
                destinationNodeSector.CalcWayPointJoinNode(destinationMultiSectorNode, g_map);
                DestSectorNodes[level] = destinationMultiSectorNode;
            }
            else
            {
                DestSectorNodes[level] = e;
            }
        }
    }
    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="start"></param>
    /// <param name="dest"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private static List<PathNode> FindPathRoad(PathNode[] start, PathNode[] dest)
    {
        List<PathNode> list = null;
        for (int lev = g_map.SectorLevelNum - 1; lev >= 0; lev--)
        {
            if (lev > 0 && start[lev].sector == dest[lev].sector)
                continue;
            list = AStar.SearchPathWayPointRoad(start[lev], dest[lev], null);
            if (list != null)
            {
                list = AStar.FindPathRoad(list, (ushort)lev);
                break;
            }
        }
        // 过滤掉一个扇区中三个点的情况，这种情况一般只出现在s，e，再+ 扇区的一个出入口情况。
        if (list != null)
        {
            for (int i = 0; i < list.Count - 3;)
            {
                if (list[i].sector == list[i + 1].sector && list[i + 1].sector == list[i + 2].sector)
                {
                    list.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }
        }
        return list;
    }
    /// <summary>
    /// 移除起始点way point 数据，担心一点该点为扇区的出入口产生的bug。
    /// </summary>
    private static void RemovePreviousSearch()
    {
        for (int lev = g_map.SectorLevelNum - 1; lev >= 0; lev--)
        {
            if (StartSectorNodes != null && StartSectorNodes.Length > lev)
            {
                if (StartSectorNodes[lev] != null && StartSectorNodes[lev].IsGate == false)
                {
                    g_map.RemoveWayPointNode(lev, StartSectorNodes[lev]);
                }
            }
            if (DestSectorNodes != null && DestSectorNodes.Length > lev)
            {
                if (DestSectorNodes[lev] != null  && DestSectorNodes[lev].IsGate == false)
                {
                    g_map.RemoveWayPointNode(lev, DestSectorNodes[lev]);
                }
            }
        }
    }
}

public class RoadResult
{
    public List<IntVector2> lRoad = new List<IntVector2>();
    public List<ushort> lSector = new List<ushort>();
    public MapTile dest;

    public void Clear()
    {
        lRoad.Clear();
        lSector.Clear();
        dest = null;
    }

    /// <summary>
    /// 道路处理
    /// </summary>
    /// <param name="lpn"></param>
    public void processRoad(List<PathNode> lpn, MapTile destinationTile)
    {
        Clear();
        dest = destinationTile;
        if (lpn == null)
            return;
        int count = lpn.Count;
        // 获取包含的扇区
        for (int i = 0; i < count; i++)
        {
            if (lSector.Contains(lpn[i].sector) == false)
            {
                lSector.Add(lpn[i].sector);
            }
        }
        /*if (CheckNeedOptimization() == true)
        {
            OptimizationRoad(lpn[0].tileConnection, destinationTile);
        }
        else*/
        {
            for (int i = 0; i < count;)
            {
                if (i < count - 1 && lpn[i].sector == lpn[i + 1].sector)
                {
                    lRoad.AddRange(lpn[i].GetRoad(lpn[i + 1]));
                    i += 2;
                }
                else
                {
                    lRoad.Add(lpn[i].tileConnection.Pos);
                    i++;
                }
            }
        }
    }
    /// <summary>
    /// 判断路径是否需要优化
    /// </summary>
    /// <returns></returns>
    private bool CheckNeedOptimization()
    {
        int max = 2;
        if (lSector.Count >= max * max)
            return false;
        IntVector2 min2 = IntVector2.zero;
        IntVector2 max2 = IntVector2.zero;

        for (int i = 0; i < lSector.Count; i++)
        {
            if (i == 0)
            {
                min2 = PathFind.instance.m_map.GetLowSectorPos(lSector[0]);
                max2 = min2;
            }
            else
            {
                IntVector2 v = PathFind.instance.m_map.GetLowSectorPos(lSector[i]);
                if (v.x < min2.x)
                {
                    min2.x = v.x;
                }
                if (v.y < min2.y)
                {
                    min2.y = v.y;
                }
                //
                if (v.x > max2.x)
                {
                    max2.x = v.x;
                }
                if (v.y > max2.y)
                {
                    max2.y = v.y;
                }
            }
        }
        if (max2.y - min2.y <= max && max2.x - min2.x <= max)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 优化线路，经过扇区在2*2范围内才进行优化。
    /// </summary>
    private void OptimizationRoad(MapTile start, MapTile end)
    {
        lRoad.Add(start.Pos);
        lRoad.AddRange(FlowField.GetRoadInSector(start, end, lSector));
        lRoad.Add(end.Pos);
    }
}

