using System.Collections.Generic;

/// <summary>
/// Tile API
/// </summary>
public class TileHelp 
{
    public static Map g_Map = null;
    public static MapTileSearchResult g_Result = null;
    private static IntVector2[] g_DirArray = new IntVector2[4];
    public static void Init(Map map)
    {
        g_Map = map;
        g_Result = new MapTileSearchResult();
    }

    /// <summary>
    /// 清理
    /// </summary>
    public static void Clear()
    {
        g_Map = null;
        g_Result.Clear();
        g_Result = null;
        g_DirArray = null;
    }
    /// <summary>
    /// 直角增加的IntegrationValue
    /// </summary>
    public static readonly ushort StraightAddIntegrationValue = 10;
    /// <summary>
    /// 对角增加的IntegrationValue
    /// </summary>
    public static readonly ushort DiagonalAddIntegrationValue = 14;
    /// <summary>
    /// 流场算法的初始默认值
    /// </summary>
    public static readonly ushort tileResetIntegrationValue = 60000; 
    /// <summary>
    /// 获取tile同一个扇区的周边邻居，并更新field 值。for FlowField算法，
    /// </summary>
    /// <param name="tile">指定的tile</param>
    /// <returns></returns>
    public static MapTileSearchResult GetAllNeighboursInSectorFlowFieldSearch(MapTile tile)
    {
        g_Result.ClearData();
        //straight
        Set4StraightDirections(tile.Pos);
        int newintegrationValue = tile.integrationValue + TileHelp.StraightAddIntegrationValue;
        MapTile r  =AddNeighbourMapTile(g_DirArray[0], tile.sectorIndex, (ushort)newintegrationValue);
        MapTile l = AddNeighbourMapTile(g_DirArray[1], tile.sectorIndex, (ushort)newintegrationValue);
        MapTile d = AddNeighbourMapTile(g_DirArray[2], tile.sectorIndex, (ushort)newintegrationValue);
        MapTile t = AddNeighbourMapTile(g_DirArray[3], tile.sectorIndex, (ushort)newintegrationValue);
        // diagonal
        Set4DiagonalDirections(tile.Pos);
        newintegrationValue = tile.integrationValue + TileHelp.DiagonalAddIntegrationValue;
        if (r != null || t != null)
        {
            AddNeighbourMapTile(g_DirArray[0], tile.sectorIndex, (ushort)newintegrationValue);
        }
        if (r != null || d != null)
        {
            AddNeighbourMapTile(g_DirArray[1], tile.sectorIndex, (ushort)newintegrationValue);
        }
        if (l != null || d != null)
        {
            AddNeighbourMapTile(g_DirArray[2], tile.sectorIndex, (ushort)newintegrationValue);
        }
        if (l != null || t != null)
        {
            AddNeighbourMapTile(g_DirArray[3], tile.sectorIndex, (ushort)newintegrationValue);
        }
        return g_Result;
    }
    /// <summary>
    /// 获取Tile的直角4个邻居，不包含block，且integrationValue小的，并在searchField标记区间内的。
    /// 并更新integrationValue
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static MapTileSearchResult GetNeighboursExpansionSearch(MapTile tile, List<ushort> lSector)
    {
        g_Result.ClearData();
        //straight
        Set4StraightDirections(tile.Pos);
        int newintegrationValue = tile.integrationValue + TileHelp.StraightAddIntegrationValue;
        MapTile r = AddNeighbourMapTile(g_DirArray[0], lSector, (ushort)newintegrationValue);
        MapTile l = AddNeighbourMapTile(g_DirArray[1], lSector, (ushort)newintegrationValue);
        MapTile d = AddNeighbourMapTile(g_DirArray[2], lSector, (ushort)newintegrationValue);
        MapTile t = AddNeighbourMapTile(g_DirArray[3], lSector, (ushort)newintegrationValue);
        // diagonal
        Set4DiagonalDirections(tile.Pos);
        newintegrationValue = tile.integrationValue + TileHelp.DiagonalAddIntegrationValue;
        if (r != null || t != null)
        {
            AddNeighbourMapTile(g_DirArray[0], lSector, (ushort)newintegrationValue);
        }
        if (r != null || d != null)
        {
            AddNeighbourMapTile(g_DirArray[1], lSector, (ushort)newintegrationValue);
        }
        if (l != null || d != null)
        {
            AddNeighbourMapTile(g_DirArray[2], lSector, (ushort)newintegrationValue);
        }
        if (l != null || t != null)
        {
            AddNeighbourMapTile(g_DirArray[3], lSector, (ushort)newintegrationValue);
        }
        return g_Result;

    }
    /// <summary>
    /// 获取tile及其8个邻居中，integrationValue最小的Tile。
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static MapTile GetLowestIntergrationCostTile(MapTile tile)
    {
        MapTile neighbour;
        MapTile lowestCostNode = tile;
        MapTile t1 = null;
        MapTile t2 = null;
        // 先4个直角
        //straight
        Set4StraightDirections(tile.Pos);
        for (int i = 0; i < g_DirArray.Length; i++)
        {
            neighbour = g_Map.GetMapTile(g_DirArray[i]);
            if (neighbour != null && neighbour.integrationValue < lowestCostNode.integrationValue)
            {
                    lowestCostNode = neighbour;
            }
        }
        // diagonal
        Set4DiagonalDirections(tile.Pos);
        for (int i = 0; i < g_DirArray.Length; i++)
        {
            neighbour = g_Map.GetMapTile(g_DirArray[i]);
            if (neighbour != null && neighbour.integrationValue < lowestCostNode.integrationValue)
            {
                //斜角联通的判断
                t1 = g_Map.GetMapTile(neighbour.Pos.x, tile.Pos.y);
                t2 = g_Map.GetMapTile(tile.Pos.x, neighbour.Pos.y);
                if ((t1 == null || t1.blocked) && (t2 == null || t2.blocked))
                {
                    // diagonal was blocked off
                }
                else
                {
                    lowestCostNode = neighbour;
                }
            }
        }
        if (lowestCostNode == tile)
        {
            return null;
        }
        else return lowestCostNode;
    }

    /// <summary>
    /// 获取范围内的grid，满足挡格要求
    /// </summary>
    /// <param name="center">中心</param>
    /// <param name="radius">半径</param>
    /// <param name="isBlock">是否挡格</param>
    /// <param name="listAll">返回列表</param>
    /// <returns>正确匹配true：否则false</returns>
    public static bool GetAreaTile(IntVector2 center, int radius, bool isBlock, ref List<MapTile> listAll)
    {
        IntVector2 lt = center - IntVector2.one * radius;
        int lenght = radius * 2 +1;
        for (int x = 0; x < lenght; x++)
        {
            for (int y = 0; y < lenght; y++)
            {
                MapTile t = g_Map.GetMapTile(lt.x + x, lt.y + y);
                if (t == null || t.blocked != isBlock)
                {
                    return false;
                }
                else
                {
                    listAll.Add(t);
                }
            }
        }
        return true;
    }
    /// <summary>
    /// 设置直角4个方向
    /// </summary>
    /// <returns></returns>
    private static void Set4StraightDirections(IntVector2 pos)
    {;
        g_DirArray[0] = pos.Right;
        g_DirArray[1] = pos.Left;
        g_DirArray[2] = pos.Down;
        g_DirArray[3] = pos.Top;
    }
    /// <summary>
    /// 设置斜角4个方向
    /// </summary>
    /// <returns></returns>
    private static void Set4DiagonalDirections(IntVector2 pos)
    {
        g_DirArray[0] = pos.RightTop;
        g_DirArray[1] = pos.RightDown;
        g_DirArray[2] = pos.LeftDown;
        g_DirArray[3] = pos.LeftTop;
    }
    /// <summary>
    /// 添加邻居
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="integrationValue"></param>
    /// <param name="sectorIndex"></param>
    /// <returns></returns>
    private static MapTile AddNeighbourMapTile(IntVector2 pos, ushort sectorIndex, ushort newIntegration)
    {
        MapTile neighbour = g_Map.GetMapTile(pos);
        if (neighbour != null && !neighbour.blocked && neighbour.sectorIndex == sectorIndex)
        {
            if (newIntegration < neighbour.integrationValue)
            {
                neighbour.integrationValue = newIntegration;
                g_Result.Add(neighbour);
            }
            return neighbour;
        }
        return null;
    }
    /// <summary>
    /// 添加邻居
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="integrationValue"></param>
    /// <param name="sectorIndex"></param>
    /// <returns></returns>
    private static MapTile AddNeighbourMapTile(IntVector2 pos, List<ushort> lSector, ushort newIntegration)
    {
        MapTile neighbour = g_Map.GetMapTile(pos);
        if (neighbour != null && !neighbour.blocked && lSector.Contains(neighbour.sectorIndex) == true)
        {
            if (newIntegration < neighbour.integrationValue)
            {
                neighbour.integrationValue = newIntegration;
                g_Result.Add(neighbour);
            }
            return neighbour;
        }
        return null;
    }


    public static List<MapTile> GetAllNeighbours(MapTile tile)
    {
        List<MapTile> neighbours = new List<MapTile>();
        /*MapTile neighbour = null;
        MapTile t1 = null;
        MapTile t2 = null;
        if (tile == null || g_Map == null)
            return null;
        //straight
        Set4StraightDirections(tile.Pos);
        for (int i = 0; i < g_DirArray.Length; i++)
        {
            neighbour = g_Map.GetMapTile(g_DirArray[i]);
            if (neighbour != null && !neighbour.blocked)
            {
                neighbours.Add(neighbour);
            }
        }
        // diagonal
        Set4DiagonalDirections(tile.Pos);
        for (int i = 0; i < g_DirArray.Length; i++)
        {
            neighbour = g_Map.GetMapTile(g_DirArray[i]);
            if (neighbour != null && !neighbour.blocked)
            {
                neighbours.Add(neighbour);
            }
        }*/
        if (!tile.blocked)
        {
            neighbours.Add(tile);
        }
        return neighbours;
    }

}


/// <summary>
/// 地图搜索结果
/// </summary>
public class MapTileSearchResult
{
    /// <summary>
    /// 邻居数组
    /// </summary>
    private MapTile[] NeighboursArray = new MapTile[9];
    /// <summary>
    /// 数量
    /// </summary>
    private int m_Count = 0;
    public int Count
    {
        get { return m_Count; }
    }
    /// <summary>
    /// 清理数据
    /// </summary>
    public void ClearData()
    {
        m_Count = 0;
    }
    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        NeighboursArray = null;
    }
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="tile"></param>
    public void Add(MapTile tile)
    {
        if (m_Count >= 9)
            return;
        NeighboursArray[m_Count] = tile;
        m_Count++;
    }
    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public MapTile Get(int i)
    {
        if (m_Count > i)
        {
            return NeighboursArray[i];
        }
        return null;
    }
}
