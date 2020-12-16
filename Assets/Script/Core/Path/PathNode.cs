using System.Collections.Generic;
using System;

/// <summary>
/// 寻路节点
/// </summary>
public class PathNode : IHeapItem<PathNode>
{
    /// <summary>
    /// 所在的扇区索引
    /// </summary>
    public ushort sector = 0;
    /// <summary>
    /// 连接的Tile
    /// </summary>
    public MapTile tileConnection = null;
    /// <summary>
    /// 反推路径点需要，知道前一个路径点哪里来的。
    /// </summary>
    public PathNode parent = null;

    public int G = 0;
    public int H = 0;
    public int F
    {
        get { return G + H; }
    }

    private int heapIndex;
    /// <summary>
    /// 连接到相领扇区的gate，或连接到子地图的gate
    /// 起到连接桥梁作用。
    /// </summary>
    public PathNode LinkOtherSectorGate= null;
    /// <summary>
    /// 扇区内部各个出口的连接距离(寻路距离)，为构建扇区层面 way point使用。
    /// 当this PathNode作为way point时，拥有这个连接属性，
    /// 保存的是扇区内部连接cost(寻路距离)。
    /// 扇区内部那些点存在连接：
    /// 1.扇区内部的寻路起点，终点。 
    /// 2.扇区的出入口(相领扇区的出入口)。
    /// 3.做为子地图的出入口。
    /// 当LinkOtherSectorGate不为null时，也保存于他的连接距离。
    /// 他们之间互相连接，保存相互间的cost。
    /// </summary>
    public Dictionary<PathNode, LinkInfo> connections = new Dictionary<PathNode, LinkInfo>();
    /// <summary>
    /// 是否为gate，否则就只能是寻路起点/终点，寻路起点/终点可能为gate。
    /// </summary>
    public bool IsGate = true;



    public int HeapIndex
    {
        get{ return heapIndex;}
        set{ heapIndex = value;}
    }


    public int CompareTo(PathNode nodeToCompare)
    {
        int compare = F.CompareTo(nodeToCompare.F);
        if (compare == 0)
            compare = H.CompareTo(nodeToCompare.H);

        return -compare;
    }

    /// <summary>
    /// 建立2个way point的连接距离
    /// </summary>
    /// <param name="sectorNode"></param>
    /// <param name="sectorNode2"></param>
    /// <param name="distance"></param>
    public static void LinkSectorNode(PathNode sectorNode, PathNode sectorNode2, int distance, List<IntVector2> lRoad)
    {
        LinkInfo v ;
        LinkInfo rv;
        if (lRoad == null || lRoad.Count == 0)
        {
            v = new LinkInfo(distance);
            rv = new LinkInfo(distance);
        }
        else
        {
            v = new LinkInfo(distance, lRoad);
            List<IntVector2> RlRoad = new List<IntVector2>();
            RlRoad.AddRange(lRoad);
            RlRoad.Reverse();
            rv = new LinkInfo(distance, RlRoad);
        }
        if (sectorNode.connections.ContainsKey(sectorNode2))
            sectorNode.connections[sectorNode2] = v;
        else
            sectorNode.connections.Add(sectorNode2, v);


        if (sectorNode2.connections.ContainsKey(sectorNode))
            sectorNode2.connections[sectorNode] = rv;
        else
            sectorNode2.connections.Add(sectorNode, rv);
    }
    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public List<IntVector2> GetRoad(PathNode other)
    {
        List<IntVector2> l = new List<IntVector2>();
        if (other == null)
            return l;
        if (connections.ContainsKey(other) == true)
        {
            l.Add(this.tileConnection.Pos);
            l.AddRange(connections[other].ListCrossTile);
            l.Add(other.tileConnection.Pos);
        }
        return l;
    }


    public PathNodeWrite GetOutData()
    {
        PathNodeWrite wr = new PathNodeWrite();
        wr.pos = tileConnection.Pos;
        if (LinkOtherSectorGate != null)
        {
            wr.bridgegepos = LinkOtherSectorGate.tileConnection.Pos;
        }
        else
        {
            wr.bridgegepos = new IntVector2(-1, -1);
        }

        foreach (PathNode p in connections.Keys)
        {
            wr.link.Add(new LinkWrite(p.tileConnection.Pos, connections[p]));
        }
        return wr;
    }
}

/// <summary>
/// way point 链接信息
/// </summary>
[Serializable]
public class LinkInfo
{
    public LinkInfo() { }
    public LinkInfo(int distance)
    {
        this.LinkDistance = distance;
    }
    public LinkInfo(int distance, List<IntVector2> l)
    {
        this.LinkDistance = distance;
        if (l != null && l.Count > 0)
        {
            this.ListCrossTile.AddRange(l);
        }
    }
    /// <summary>
    /// 链接距离
    /// </summary>
    public int LinkDistance;
    /// <summary>
    /// 通过的Tile
    /// </summary>
    public List<IntVector2> ListCrossTile = new List<IntVector2>();
}
