using System.Collections.Generic;

/// <summary>
/// 地图修改管理，主要用于设置动态挡格。
/// </summary>
public class MapChangeManger 
{
    public static Map g_map = null;
    public static void Init(Map map)
    {
        g_map = map;
    }
    private static  List<MapTile> g_tilesBlockedAdjusted = new List<MapTile>();
    /// <summary>
    /// 所有被影响到的low扇区列表
    /// </summary>
    private static List<LowSector> g_sectorChanges = new List<LowSector>();
    /// <summary>
    /// 影响到的高级扇区
    /// </summary>
    private static List<MapSector> g_sectorHighChanges = new List<MapSector>();
    /// <summary>
    /// 影响到low扇区边缘列表
    /// </summary>
    private static Dictionary<LowSector, List<Side>> g_sectorEdgeChangesLowLevel = new Dictionary<LowSector, List<Side>>();
    /// <summary>
    /// 影响到的高级扇区边缘的列表
    /// </summary>
    private static List<NeighbourSector> g_sectorEdgeChangesHighLevel = new List<NeighbourSector>();

    /// <summary>
    /// 设置动态挡格
    /// </summary>
    /// <param name="tile"></param>
    public static void BlockTile(MapTile tile)
    {
        if (tile != null && !tile.blocked)
            SetBlock(tile);
    }

    /// <summary>
    /// 设置动态挡格
    /// </summary>
    /// <param name="listT"></param>
    public static void BlockTile(List<MapTile> listT)
    {
        foreach (MapTile t in listT)
        {
            if (t != null)
            {
                BlockTile(t);
            }
        }
    }

    /// <summary>
    /// 移除动态挡格
    /// </summary>
    /// <param name="listT"></param>
    public static void UnBlockTile(List<MapTile> listT)
    {
        foreach (MapTile t in listT)
        {
            if (t != null)
            {
                UnBlockTile(t);
            }
        }
    }

    /// <summary>
    /// 移除动态挡格
    /// </summary>
    /// <param name="tile"></param>
    public static void UnBlockTile(MapTile tile)
    {
        if (tile != null && tile.blocked)
            RemoveBlock(tile);
    }


    private static void SetBlock(MapTile tile)
    {
        tile.blocked = true;

        if (g_tilesBlockedAdjusted.Contains(tile))
            g_tilesBlockedAdjusted.Remove(tile);
        else
            g_tilesBlockedAdjusted.Add(tile);
    }


    private static void RemoveBlock(MapTile tile)
    {
        tile.blocked = false;

        if (g_tilesBlockedAdjusted.Contains(tile))
            g_tilesBlockedAdjusted.Remove(tile);
        else
            g_tilesBlockedAdjusted.Add(tile);
    }
    /// <summary>
    /// 处理扇区边缘的tile
    /// </summary>
    /// <param name="ms">所在扇区</param>
    /// <param name="side">所在边</param>
    private static void ProcessSideTile(LowSector ms, Side side)
    {
        MapSector neighbourms = g_map.Findneighbour(ms, side) ;
        if (neighbourms != null)
        {
            if (g_map.SectorLevelNum > 1)
            {
                MapSector high1 = ms.GetParentMapSector(g_map);
                MapSector high2 = neighbourms.GetParentMapSector(g_map);
                // 还是高级扇区的边缘。
                if (high1 != high2)
                {
                    NeighbourSector nb = new NeighbourSector();
                    if (ms.ID < neighbourms.ID)
                    {
                        nb.side = side;
                        nb.A = high1;
                        nb.B = high2;
                    }
                    else
                    {
                        nb.side = Util.FlipDirection(side);
                        nb.A = high2;
                        nb.B = high1;
                    }

                    if (!g_sectorEdgeChangesHighLevel.Contains(nb))
                        g_sectorEdgeChangesHighLevel.Add(nb);
                }
            }
            //
            if (g_sectorEdgeChangesLowLevel.ContainsKey(neighbourms as LowSector))
            {
                if (g_sectorEdgeChangesLowLevel[neighbourms as LowSector].Contains(Util.FlipDirection(side)))// other side already filled in
                {
                    if (!g_sectorChanges.Contains(ms as LowSector))// other sector exist and the side. add our sector for general change
                        g_sectorChanges.Add(ms as LowSector);
                }
                else if (!g_sectorEdgeChangesLowLevel[ms as LowSector].Contains(side)) //  other sector exist but not the side. add our sector for Edge change
                    g_sectorEdgeChangesLowLevel[ms as LowSector].Add(side);
            }
            else// other sector not (yet? )added.   add ourselves and other sector for genral change
            {
                if (!g_sectorChanges.Contains(neighbourms as LowSector))
                    g_sectorChanges.Add(neighbourms as LowSector);

                if (!g_sectorEdgeChangesLowLevel[ms].Contains(side))
                    g_sectorEdgeChangesLowLevel[ms].Add(side);
            }
        }
        else if (!g_sectorEdgeChangesLowLevel[ms as LowSector].Contains(side))// other sector does not exist, add ourselves
            g_sectorEdgeChangesLowLevel[ms as LowSector].Add(side);
    }




    public static void InputChanges()
    {
        if (g_tilesBlockedAdjusted.Count > 0)
        {
            g_sectorChanges.Clear();
            g_sectorEdgeChangesLowLevel.Clear();
            g_sectorEdgeChangesHighLevel.Clear();
            for (int i = 0; i < g_tilesBlockedAdjusted.Count; i++)
            {
                ParseChangeSector(g_tilesBlockedAdjusted[i]);
            }
            // rebuild sector edges
            ProcessLowSector();
            // 重构高级扇区的way point cost
            ProcessHighSector(1, g_map.SectorLevelNum, g_sectorHighChanges, g_sectorEdgeChangesHighLevel);
            g_tilesBlockedAdjusted.Clear();
            // 通知寻路
            PathFind.instance.WorldHasBeenChanged(g_sectorChanges);
        }

    }

    /// <summary>
    /// 分析影响到的扇区
    /// </summary>
    /// <param name="tile"></param>
    private static void ParseChangeSector(MapTile tile)
    {
        if (tile == null)
            return;

        LowSector ms = g_map.FindMapSector(0, tile.sectorIndex) as LowSector;
        if (ms == null)
            return;
        // 是否为边上的点
        bool tileOnEdge = false;

        if (!g_sectorEdgeChangesLowLevel.ContainsKey(ms))
            g_sectorEdgeChangesLowLevel.Add(ms, new List<Side>());

        if (tile.Pos.y == ms.top && ms.top != 0) //top
        {
            tileOnEdge = true;
            if (!g_sectorEdgeChangesLowLevel[ms].Contains(Side.Top))
                ProcessSideTile(ms, Side.Top);
        }
        if (tile.Pos.y == ms.bottom && ms.Pos.y < g_map.levelDimensions[0].numHeight - 1)
        {
            tileOnEdge = true;
            if (!g_sectorEdgeChangesLowLevel[ms].Contains(Side.Down))
                ProcessSideTile(ms, Side.Down);
        }
        if (tile.Pos.x == ms.left && ms.left != 0)//left
        {
            tileOnEdge = true;
            if (!g_sectorEdgeChangesLowLevel[ms].Contains(Side.Left))
                ProcessSideTile(ms, Side.Left);
        }
        if (tile.Pos.x == ms.right && ms.Pos.y < g_map.levelDimensions[0].numWidth - 1) //right
        {
            tileOnEdge = true;
            if (!g_sectorEdgeChangesLowLevel[ms].Contains(Side.Right))
                ProcessSideTile(ms, Side.Right);
        }

        if (!tileOnEdge)
        {
            if (!g_sectorChanges.Contains(ms as LowSector))
                g_sectorChanges.Add(ms as LowSector);
        }
    }
    /// <summary>
    /// 处理低级扇区,并返回高级扇区
    /// </summary>
    private static void ProcessLowSector()
    {
        //处理low 扇区的边
        g_sectorHighChanges.Clear();
        foreach (LowSector ms in g_sectorEdgeChangesLowLevel.Keys)
        {
            ms.BuildEdgeGate(g_sectorEdgeChangesLowLevel[ms], g_map);
            ms.RemoveAllWapPointCostInSector();
            if (!g_sectorChanges.Contains(ms))
                g_sectorChanges.Add(ms);
        }
        // 处理low 扇区的cost
        foreach (LowSector ms in g_sectorChanges)
        {
            ms.BuildSectorWayPointCost(g_map);
            MapSector highSector = g_map.GetParentMapSector(ms);
            if (highSector != null)
            {
                if (!g_sectorHighChanges.Contains(highSector))
                    g_sectorHighChanges.Add(highSector);
            }
        }
    }

    /// <summary>
    /// 处理高级扇区及边
    /// </summary>
    /// <param name="lev"></param>
    /// <param name="LevelNum"></param>
    /// <param name="listSector"></param>
    /// <param name="listNb"></param>
    private static void ProcessHighSector(int curLev, int LevelNum, List<MapSector> listSector, List<NeighbourSector>listNb)
    {
        // 重构高级扇区的边
        if (listNb != null && listNb.Count > 0)
        {
            foreach (NeighbourSector nb in listNb)
            {
                if (nb.B != null && nb.A != null)
                {
                    nb.B.RemoveAllWapPointOnSectorEdge(Util.FlipDirection(nb.side), g_map);
                    nb.A.RemoveAllWapPointOnSectorEdge(nb.side, g_map);
                    nb.A.BuildOneEdgeGate(nb.side, g_map);
                }
            }
        }
        // 重构高级扇区的way point cost
        if (listSector != null && listSector.Count > 0)
        {
            foreach (HighSector hs in listSector)
            {
                hs.AdjustedSectorWayPointCost(g_map);
            }
        }
        // 判断是否为最高等级扇区
        if (curLev < LevelNum - 1)
        {
            ProcessHighSector(curLev + 1, LevelNum,
                                g_map.GetParentSectorList(listSector, curLev + 1),
                                g_map.GetParentNeighbourSectorList(listNb, curLev + 1));
        }


    }

}
/// <summary>
/// 邻居扇区
/// </summary>
public struct NeighbourSector
{
    public MapSector A;
    public MapSector B;
    public Side side;

    /// <summary>
    /// 构建结构体
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="s"></param>
    public NeighbourSector(MapSector a, MapSector b, Side s)
    {
        this.A = a;
        this.B = b;
        this.side = s;
    }

    public static bool operator !=(NeighbourSector v1, NeighbourSector v2)
    {
        return v1.A != v2.A || v1.B != v2.B || v1.side != v2.side;
    }

    public static bool operator ==(NeighbourSector v1, NeighbourSector v2)
    {
        return v1.A == v2.A && v1.B == v2.B && v1.side == v2.side;
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null)
        {
            return false;
        }

        NeighbourSector p = (NeighbourSector)obj;
        if ((System.Object)p == null)
        {
            return false;
        }
        return (A == p.A) && (B == p.B) && (side == p.side);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}