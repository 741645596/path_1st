using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public static Map g_map = null;
    public static Heap<PathNode> g_openSet = null;
    public static HashSet<PathNode> g_closedSet = null;
    public static void Init(Map map)
    {
        g_map = map;
        g_openSet = new Heap<PathNode>(6000);
        g_closedSet = new HashSet<PathNode>();
    }

    public static void Clear()
    {
        ClearCacheData();
        g_map = null;
        g_openSet = null;
        g_closedSet = null;
    }

    private static void ClearCacheData()
    {
        while (g_openSet.Count > 0)
        {
            g_openSet.RemoveFirst();
        }
        g_closedSet.Clear();
    }
    /// <summary>
    /// 获取2个节点的距离
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns></returns>
    private static int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        return Mathf.Abs(nodeB.tileConnection.Pos.x - nodeA.tileConnection.Pos.x) + Mathf.Abs(nodeB.tileConnection.Pos.y - nodeA.tileConnection.Pos.y);
    }
    /// <summary>
    /// 回溯路径
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private static List<PathNode> RetraceRoadPath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        // 获得路径
        PathNode currentNode = endNode;
        path.Insert(0, currentNode);
        while (currentNode != startNode)
        {// if sector  dont match, its a new one
            currentNode = currentNode.parent;
            path.Insert(0, currentNode);
        }
        return path;
    }
    /// <summary>
    /// 路径距离
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private static int RetraceRoadPathDistance(PathNode startNode, PathNode endNode)
    {
        int dis = 0;
        PathNode currentNode = endNode;
        while (currentNode != startNode)
        {
            if (currentNode.connections.ContainsKey(currentNode.parent))
                dis += currentNode.connections[currentNode.parent].LinkDistance;
            else
                dis += 1;
            currentNode = currentNode.parent;
        }
        return dis;
    }


    /// <summary>
    /// 在限制范围内的way point 上进行A* 寻路
    /// </summary>
    /// <param name="start">寻路起点</param>
    /// <param name="destination">寻路终点</param>
    /// <param name="validSectors">指定寻路way point 的范围指定的扇区列表中</param>
    /// <param name="returnPath">是否返回寻路路径</param>
    /// <returns></returns>
    public static int SearchPathWayPointRoadDistance(PathNode start, PathNode dest, List<ushort> validSectors)
    {
        ClearCacheData();
        if (validSectors == null || validSectors.Count == 0)
            return -1;
        g_openSet.Add(start);
        while (g_openSet.Count > 0)
        {
            PathNode currentNode = g_openSet.RemoveFirst();
            g_closedSet.Add(currentNode);

            if (currentNode == dest)
                return RetraceRoadPathDistance(start, dest);

            foreach (PathNode neighbour in currentNode.connections.Keys)
            {
                if (g_closedSet.Contains(neighbour) || validSectors.Contains(neighbour.sector) == false)
                    continue;

                int newMovementCostToNeighbour = currentNode.G + currentNode.connections[neighbour].LinkDistance;
                if (newMovementCostToNeighbour < neighbour.G || !g_openSet.Contains(neighbour))
                {
                    neighbour.G = newMovementCostToNeighbour;
                    neighbour.H = GetDistance(neighbour, dest);
                    neighbour.parent = currentNode;

                    if (!g_openSet.Contains(neighbour))
                        g_openSet.Add(neighbour);
                    else
                        g_openSet.UpdateItem(neighbour);
                }
            }
        }
        return -1;
    }
    /// <summary>
    /// 在限制范围内的way point 上进行A* 寻路
    /// </summary>
    /// <param name="start">寻路起点</param>
    /// <param name="destination">寻路终点</param>
    /// <param name="validSectors">指定寻路way point 的范围指定的扇区列表中</param>
    /// <param name="returnPath">是否返回寻路路径</param>
    /// <returns></returns>
    public static List<PathNode> SearchPathWayPointRoad(PathNode start, PathNode dest, List<ushort> validSectors)
    {
        ClearCacheData();
        g_openSet.Add(start);
        while (g_openSet.Count > 0)
        {
            PathNode currentNode = g_openSet.RemoveFirst();
            g_closedSet.Add(currentNode);

            if (currentNode == dest)
                return RetraceRoadPath(start, dest);

            foreach (PathNode neighbour in currentNode.connections.Keys)
            {
                if (g_closedSet.Contains(neighbour))
                    continue;
                if (validSectors != null && validSectors.Contains(neighbour.sector) == false)
                    continue;

                int newMovementCostToNeighbour = currentNode.G + currentNode.connections[neighbour].LinkDistance;
                if (newMovementCostToNeighbour < neighbour.G || !g_openSet.Contains(neighbour))
                {
                    neighbour.G = newMovementCostToNeighbour;
                    neighbour.H = GetDistance(neighbour, dest);
                    neighbour.parent = currentNode;

                    if (!g_openSet.Contains(neighbour))
                        g_openSet.Add(neighbour);
                    else
                        g_openSet.UpdateItem(neighbour);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 在扇区内寻路
    /// </summary>
    /// <param name="s"></param>
    /// <param name="e"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private static List<PathNode> FindPathRoadInSector(PathNode s, PathNode e, ushort level)
    {
        if (s == null || e == null || s.sector != e.sector || level<=0 )
            return null;
        // 低一级的。
        PathNode ls = g_map.GetWayPointPathNode(level -1, s.tileConnection);
        PathNode le = g_map.GetWayPointPathNode(level -1, e.tileConnection);

        if (ls == null || le == null)
            return null;
        List<ushort> validSectors = g_map.GetChildSectorindex(level, s.sector);

        List <PathNode> lp = SearchPathWayPointRoad(ls, le, validSectors);
        return FindPathRoad(lp, (ushort)(level -1));
    }

    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="list"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static List<PathNode> FindPathRoad(List<PathNode> list, ushort level)
    {
        if (level > 0)
        {
            List<PathNode> listp = new List<PathNode>();
            int count = list.Count;
            for (int i = 0; i < count; )
            {
                if (i + 1 < count  && list[i].sector == list[i + 1].sector)
                {
                    List<PathNode> ll = FindPathRoadInSector(list[i], list[i + 1], level);
                    if (ll != null && ll.Count > 0)
                    {
                        listp.AddRange(ll);
                    }
                    i += 2;
                }
                else
                {
                    PathNode ls = g_map.GetWayPointPathNode(level - 1, list[i].tileConnection);
                    if (ls != null )
                    {
                        listp.Add(ls);
                    }
                    i++;
                }
            }
            return listp;
        }
        else
        {
            return list;
        }
    }
}
