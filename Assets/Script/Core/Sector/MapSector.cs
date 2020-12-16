using System.Collections.Generic;
using System;

/// <summary>
/// 扇区结构
/// </summary>
[Serializable]
public class MapSector
{
    /// <summary>
    /// 所在扇区的level
    /// </summary>
    public ushort level = 0;
    /// <summary>
    /// 扇区索引
    /// </summary>
    public ushort ID = 0;
    /// <summary>
    /// 扇区坐标
    /// </summary>
    public IntVector2 Pos = new IntVector2(-1, -1);
    /// <summary>
    /// 扇区所在的Rect，为tile的坐标。
    /// </summary>
    public ushort top = 0;
    public ushort bottom = 0;
    public ushort left = 0;
    public ushort right = 0;
    /// <summary>
    /// 真实扇区的宽高，主要是在area边缘的扇区会比标准的小
    /// </summary>
    public ushort tilesInWidth = 0;
    public ushort tilesInHeight = 0;
    /// <summary>
    /// 扇区4条边的通道出口，每条边上可能又多个出口。具体依赖door的算法。
    /// gate 做为扇区级别的寻路寻路way point。
    /// </summary>
    private Dictionary<Side, List<PathNode>> sectorGateOnEdge = new Dictionary<Side, List<PathNode>>();

    private int numPathNode = 0;
    /// <summary>
    /// 初始化工作
    /// </summary>
    public void Init()
    {
        sectorGateOnEdge.Add(Side.Left, new List<PathNode>());
        sectorGateOnEdge.Add(Side.Right, new List<PathNode>());
        sectorGateOnEdge.Add(Side.Top, new List<PathNode>());
        sectorGateOnEdge.Add(Side.Down, new List<PathNode>());
        numPathNode = 0;
    }
    /// <summary>
    /// 获取邻居扇区
    /// </summary>
    /// <param name="neighour"></param>
    /// <returns></returns>
    public MapSector Findneighbour(IntVector2 neighour, Map map)
    {
        if (map != null)
        {
            return map.Findneighbour(this, neighour);
        }
        return null;
    }

    /// <summary>
    /// 获取邻居扇区
    /// </summary>
    /// <param name="neighour"></param>
    /// <returns></returns>
    public MapSector Findneighbour(Side neighour, Map map)
    {
        if (map != null)
        {
            return map.Findneighbour(this, neighour);
        }
        return null;
    }
    /// <summary>
    /// 获取父扇区
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public MapSector GetParentMapSector(Map map)
    {
        if (map == null)
            return null;
        return map.GetParentMapSector(this);
    }

    /// <summary>
    /// 获取扇区的gate数量
    /// </summary>
    /// <returns></returns>
    public int GetAllGateNum()
    {
        int maxgates = sectorGateOnEdge[Side.Top].Count
            + sectorGateOnEdge[Side.Down].Count
            + sectorGateOnEdge[Side.Left].Count
            + sectorGateOnEdge[Side.Right].Count;
        return maxgates;
    }

    /// <summary>
    /// 获取边上的所有出口
    /// </summary>
    /// <returns></returns>
    public List<PathNode> GetPathNodeInEdge()
    {
        List<PathNode> listAllGate = new List<PathNode>();
        foreach (List<PathNode> list in sectorGateOnEdge.Values)
            listAllGate.AddRange(list);
        return listAllGate;
    }
    /// <summary>
    /// 获取指定边上的所有出口。
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public List<PathNode> GetPathNodeInEdge(Side edge)
    {
        return sectorGateOnEdge[edge];
    }
    /// <summary>
    /// 扇区的出入口创建为一个way point 点。或称为一个gate
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public PathNode CreateWayPointInSector(MapTile tile, Side side, Map map)
    {
        PathNode sectorNode = new PathNode();
        sectorNode.tileConnection = tile;
        tile.hasPathNodeConnection = true;
        sectorNode.sector = this.ID;
        sectorNode.IsGate = true;
        sectorGateOnEdge[side].Add(sectorNode);
        if (map != null)
        {
            map.AddWayPointNode(this.level, sectorNode);
            AddPathNodeNum();
        }
        return sectorNode;
    }

    /// <summary>
    /// 扇区内创建一个way point，该点为寻路起点，或终点
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public PathNode CreateWayPointInSector(MapTile tile, Map map)
    {
        PathNode sectorNode = new PathNode();
        sectorNode.tileConnection = tile;
        tile.hasPathNodeConnection = true;
        sectorNode.sector = this.ID;
        sectorNode.IsGate = false;
        if (map != null)
        {
            map.AddWayPointNode(this.level, sectorNode);
            AddPathNodeNum();
        }
        return sectorNode;
    }


    /// <summary>
    /// 移除扇区某条边的所有gate(出入口)
    /// </summary>
    /// <param name="sector">扇区</param>
    /// <param name="edgeIndex">某条边</param>
    public void RemoveAllWapPointOnSectorEdge(Side edge, Map map)
    {
        // remove the connections from other sectorNodes to the sectorNodes we will remove now
        foreach (PathNode sectorNode in sectorGateOnEdge[edge])
        {
            foreach (PathNode nodeConnected in sectorNode.connections.Keys)
                nodeConnected.connections.Remove(sectorNode);
        }

        // remove 
        foreach (PathNode sectorNode in sectorGateOnEdge[edge])
        {
            map.RemoveWayPointNode(this.level, sectorNode);
        }

        // remove entire edge
        this.sectorGateOnEdge[edge].Clear();
    }


    /// <summary>
    /// 移除扇区扇区内部的way point cost，不包含扇区间的桥梁
    /// </summary>
    public void RemoveAllWapPointCostInSector()
    {
        List<PathNode> listPath = GetPathNodeInEdge();
        foreach (PathNode node in listPath)
        {
            node.connections.Clear();

            if (node.LinkOtherSectorGate != null)
                node.connections.Add(node.LinkOtherSectorGate, new LinkInfo(1));
        }
    }


    /// <summary>
    /// 构建扇区内所有way point点的cost。
    /// </summary>
    /// <param name="map"></param>
    public void BuildSectorWayPointCost(Map map)
    {
        List<PathNode> l = GetPathNodeInEdge();
        if (l == null || l.Count == 0)
            return;
        foreach (PathNode node in l)
        {
            CreateOneWayPointCost(node, map, l.Count -1);
        }
    }

    /// <summary>
    /// 构建扇区的边及way point及cost
    /// </summary>
    /// <param name="map"></param>
    public void BuildSectorEdgeWaypoint2Cost(Map map)
    {
        // bot
        BuildOneEdgeGate(Side.Down, map);
        //right
        BuildOneEdgeGate(Side.Right, map);
        // 扇区内way point cost
        BuildSectorWayPointCost(map);
    }
    /// <summary>
    /// 构建扇区的边及way point及cost,由数据直接生成
    /// </summary>
    /// <param name="map"></param>
    public void BuildSectorEdgeWaypoint2Cost(Map map,MapSectorWrite ms)
    {
        foreach (PathNodeWrite pnw in ms.down)
        {
            PathNode node = this.CreateWayPointInSector(map.GetMapTileSafe(pnw.pos), Side.Down, map);
            PathNode neighbourNode = Findneighbour(Side.Down,map).CreateWayPointInSector(map.GetMapTileSafe(pnw.pos.Down), Side.Top, map);
            node.LinkOtherSectorGate = neighbourNode;
            neighbourNode.LinkOtherSectorGate = node;
            PathNode.LinkSectorNode(node, neighbourNode, 1, null);
        }

        foreach (PathNodeWrite pnw in ms.right)
        {
            PathNode node = this.CreateWayPointInSector(map.GetMapTileSafe(pnw.pos), Side.Right, map);
            PathNode neighbourNode = Findneighbour(Side.Right, map).CreateWayPointInSector(map.GetMapTileSafe(pnw.pos.Right), Side.Left, map);
            node.LinkOtherSectorGate = neighbourNode;
            neighbourNode.LinkOtherSectorGate = node;
            PathNode.LinkSectorNode(node, neighbourNode, 1, null);
        }

        List<PathNode> All = GetPathNodeInEdge();
        if (All == null || All.Count == 0)
            return;
        foreach (PathNode nodeA in All)
        {
            foreach (PathNode nodeB in All)
            {
                if (nodeA != nodeB)
                {
                    LinkInfo v = ms.GetDistance(nodeA.tileConnection.Pos, nodeB.tileConnection.Pos);
                    PathNode.LinkSectorNode(nodeA, nodeB, v.LinkDistance, v.ListCrossTile);
                }
            }
        }
    }
    /// <summary>
    /// 创建way point cost
    /// </summary>
    protected virtual void CreateOneWayPointCost(PathNode start, Map map, int maxRoad)
    {
    }

    public void CalcWayPointJoinNode(PathNode start, Map map)
    {
         CreateOneWayPointCost(start, map, numPathNode);
    }
    /// <summary>
    /// 构建扇区某条边的gate
    /// </summary>
    /// <param name="map"></param>
    public virtual void BuildOneEdgeGate(Side edge, Map map)
    {

    }
    /// <summary>
    /// 构建指定的几条边
    /// </summary>
    /// <param name="listEdge"></param>
    /// <param name="map"></param>
    public void BuildEdgeGate(List<Side> listEdge, Map map)
    {
        if (map == null || listEdge == null || listEdge.Count == 0)
            return;
        foreach (Side edge in listEdge)
        {
            BuildOneEdgeGate(Side.Top, map);
        }
    }

    /// <summary>
    /// 扇区内添加一个寻路节点
    /// </summary>
    public void AddPathNodeNum()
    {
        if (numPathNode < 0)
            numPathNode = 0; 
        numPathNode++;
    }
    /// <summary>
    /// 扇区内移除一个寻路节点
    /// </summary>
    public void RemovePathNodeNum()
    {
        numPathNode--;
        if (numPathNode < 0)
            numPathNode = 0;
    }
    /// <summary>
    /// 构建扇区所有边
    /// </summary>
    /// <param name="map"></param>
    public void BuildEdgeGate(Map map)
    {
        BuildOneEdgeGate(Side.Top, map);
        BuildOneEdgeGate(Side.Down, map);
        BuildOneEdgeGate(Side.Left, map);
        BuildOneEdgeGate(Side.Right, map);
    }

    /// <summary>
    /// 重构cost
    /// </summary>
    /// <param name="map"></param>
    public void AdjustedSectorWayPointCost(Map map)
    {
        RemoveAllWapPointCostInSector();
        BuildSectorWayPointCost(map);
    }

    /// <summary>
    /// 导出预处理数据
    /// </summary>
    /// <returns></returns>
    public MapSectorWrite GetOutData()
    {
        MapSectorWrite mw = new MapSectorWrite();
        // left
        if (sectorGateOnEdge.ContainsKey(Side.Left) == true)
        {
            List<PathNode> l = sectorGateOnEdge[Side.Left];
            foreach (PathNode n in l)
            {
                mw.Left.Add(n.GetOutData());
            }
        }
        // top
        if (sectorGateOnEdge.ContainsKey(Side.Top) == true)
        {
            List<PathNode> l = sectorGateOnEdge[Side.Top];
            foreach (PathNode n in l)
            {
                mw.top.Add(n.GetOutData());
            }
        }
        // right
        if (sectorGateOnEdge.ContainsKey(Side.Right) == true)
        {
            List<PathNode> l = sectorGateOnEdge[Side.Right];
            foreach (PathNode n in l)
            {
                mw.right.Add(n.GetOutData());
            }
        }
        // down
        if (sectorGateOnEdge.ContainsKey(Side.Down) == true)
        {
            List<PathNode> l = sectorGateOnEdge[Side.Down];
            foreach (PathNode n in l)
            {
                mw.down.Add(n.GetOutData());
            }
        }


        return mw;
    }
}
