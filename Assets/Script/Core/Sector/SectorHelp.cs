using System.Collections.Generic;

public class SectorHelp
{
    public static Map g_Map = null;
    public static int maxGateSize = 500;
    public static void Init(Map map)
    {
        g_Map = map;
    }
    /// <summary>
    /// Rebuild 扇区边上的节点     
    /// </summary>
    /// <param name="sector">指定的扇区</param>
    /// <param name="edge">扇区的某条边</param>
    /// <param name="startInSector">扇区中tile的起始位置</param>
    /// <param name="NeighbourSector">相领扇区</param>
    /// <param name="edgeNeighbourSector">邻居扇区上的边</param>
    /// <param name="startInNeighbourSector">邻居扇区中tile的起始位置</param>
    /// <param name="direction">方向</param>
    public static void RebuildNodesOnSectorEdge(MapSector sector, Side edge, IntVector2 startInSector,
                                         MapSector NeighbourSector, Side edgeNeighbourSector, IntVector2 startInNeighbourSector,
                                         IntVector2 direction)
    {
        if (g_Map == null)
            return;
        // 移除本扇区edgeIndex 边上的所有节点
        sector.RemoveAllWapPointOnSectorEdge(edge, g_Map);
        if (NeighbourSector != null)
        {
            NeighbourSector.RemoveAllWapPointOnSectorEdge(edgeNeighbourSector, g_Map);
        }

        int maxStep = 0;
        if (direction == IntVector2.right)
            maxStep = sector.tilesInWidth;
        else
            maxStep = sector.tilesInHeight;

        //判断邻居扇区是否有包含Tile，没有tile就是边缘了，那这条边节点为空就好。
        if (NeighbourSector != null) // if we havent found any tiles, no reason to try and build connections
        {
            List < MapTile > listGate = CalcGate(startInSector, startInNeighbourSector, direction, maxStep);
            CreateGate(sector, edge, NeighbourSector, edgeNeighbourSector, listGate);
        }
    }


    /// <summary>
    /// 计算扇区间的通道
    /// </summary>
    /// <param name="startInSector">本扇区Tile的起点</param>
    /// <param name="startInNeighbourSector">对应邻居扇区对应的起点</param>
    /// <param name="direction">方向</param>
    /// <param name="maxStep">step 步数</param>
    /// <param name="map">指定地图</param>
    /// <returns>2i + 0的元素跟 2i + 1 的元素为一对门</returns>
    private static List<MapTile> CalcGate(IntVector2 startInSector, IntVector2 startInNeighbourSector, IntVector2 direction, int maxStep)
    {
        List<MapTile> l = new List<MapTile>();
        if (g_Map == null)
            return l;
        // build nodes on edge
        bool sectorNodesOpen = false;
        int openLength = -1;
        int startNodeOfGroup = 0;

        MapTile tile1;
        MapTile tile2;
        for (int i = 0; i < maxStep; i++)
        {
            // 本扇区的
            tile1 = g_Map.GetMapTileSafe(startInSector + direction * i);
            // 邻居扇区的
            tile2 = g_Map.GetMapTileSafe(startInNeighbourSector + direction * i);

            if (tile1 != null && tile2 != null && !tile1.blocked && !tile2.blocked)
            {
                // starting point of a new connection/gate between sectors
                if (!sectorNodesOpen)
                    sectorNodesOpen = true;

                openLength++;
            }
            else
            {
                if (sectorNodesOpen) // if we have had a couple of open nodes couples
                {
                    // small enough to represent with 1 transition
                    if (openLength < maxGateSize)
                    {
                        int steps = (openLength)/ 2 + startNodeOfGroup;
                        MapTile SelfTile = g_Map.GetMapTileSafe(startInSector + direction * steps);
                        MapTile neighbourTile = g_Map.GetMapTileSafe(startInNeighbourSector + direction * steps);
                        l.Add(SelfTile);
                        l.Add(neighbourTile);
                    }
                    else
                    {
                        // to large, 2 transitions. on on each end
                        int multiplyer = startNodeOfGroup;
                        MapTile SelfTile = g_Map.GetMapTileSafe(startInSector + direction * multiplyer);
                        MapTile neighbourTile = g_Map.GetMapTileSafe(startInNeighbourSector + direction * multiplyer);
                        l.Add(SelfTile);
                        l.Add(neighbourTile);

                        multiplyer = (startNodeOfGroup + openLength);
                        SelfTile = g_Map.GetMapTileSafe(startInSector + direction * multiplyer);
                        neighbourTile = g_Map.GetMapTileSafe(startInNeighbourSector + direction * multiplyer);
                        l.Add(SelfTile);
                        l.Add(neighbourTile);
                    }

                    openLength = -1;
                    sectorNodesOpen = false;
                }
                startNodeOfGroup = i + 1;
            }
        }

        if (sectorNodesOpen) // if we have had a couple of open nodes couples
        {
            if (openLength < maxGateSize)
            {
                int steps = (openLength) / 2 + startNodeOfGroup;
                MapTile SelfTile = g_Map.GetMapTileSafe(startInSector + direction * steps);
                MapTile neighbourTile = g_Map.GetMapTileSafe(startInNeighbourSector + direction * steps);
                l.Add(SelfTile);
                l.Add(neighbourTile);
            }
            else
            {
                // to large, 2 transitions. on on each end
                int multiplyer = startNodeOfGroup;
                MapTile SelfTile = g_Map.GetMapTileSafe(startInSector + direction * multiplyer);
                MapTile neighbourTile = g_Map.GetMapTileSafe(startInNeighbourSector + direction * multiplyer);
                l.Add(SelfTile);
                l.Add(neighbourTile);

                multiplyer = (startNodeOfGroup + openLength);
                SelfTile = g_Map.GetMapTileSafe(startInSector + direction * multiplyer);
                neighbourTile = g_Map.GetMapTileSafe(startInNeighbourSector + direction * multiplyer);
                l.Add(SelfTile);
                l.Add(neighbourTile);
            }
        }
        return l;
    }
    /// <summary>
    /// 创建门
    /// </summary>
    /// <param name="sector">指定的扇区</param>
    /// <param name="edge">sector与neighbourSector相邻的一条边</param>
    /// <param name="neighbourSector">sector 邻居扇区</param>
    /// <param name="neighbourSectoredge">neighbourSector与 sector 相邻的一条边</param>
    /// <param name="listGate">为CalcGate函数的结果，保持的是个门两边的Tile</param>
    private static void CreateGate(MapSector sector, Side edge, MapSector neighbourSector, Side neighbourSectoredge, List<MapTile> listGate)
    {
        if (sector == null || neighbourSector == null || listGate == null || listGate.Count == 0)
            return;
        if (listGate.Count % 2 == 1)
            return;
        int length = listGate.Count / 2;
        for (int i = 0; i < length; i++)
        {
            CreateGate(sector, edge,listGate[2 * i], neighbourSector, neighbourSectoredge, listGate[2 * i + 1]);
        }
    }

    public static void CreateGate(MapSector sector, Side edge, MapTile tile, MapSector neighbourSector, Side neighbourSectoredge, MapTile neighbourTile)
    {
        if (sector == null || neighbourSector == null || tile == null || neighbourTile == null)
            return;
        PathNode node = sector.CreateWayPointInSector(tile, edge, g_Map);
        PathNode neighbourNode = neighbourSector.CreateWayPointInSector(neighbourTile, neighbourSectoredge, g_Map);
        node.LinkOtherSectorGate = neighbourNode;
        neighbourNode.LinkOtherSectorGate = node;
        PathNode.LinkSectorNode(node, neighbourNode, 1, null);
    }


    /// <summary>
    /// 上下级的2个扇区，某条边是否匹配
    /// </summary>
    /// <param name="highSector">高一级的扇区</param>
    /// <param name="lowSector">低一级的扇区</param>
    /// <param name="edge">指定某条边</param>
    /// <returns></returns>
    public static bool LowerSectorEdgeMatchesHigher(MapSector highSector, MapSector lowSector, Side edge)
    {
        if (highSector == null || lowSector == null)
            return false;
        if (edge == Side.Top && highSector.top == lowSector.top)
            return true;
        if (edge == Side.Down && highSector.bottom == lowSector.bottom)
            return true;
        if (edge == Side.Left && highSector.left == lowSector.left)
            return true;
        if (edge == Side.Right && highSector.right == lowSector.right)
            return true;

        return false;
    }


}
