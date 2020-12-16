using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 地图数据结构
/// </summary>
[Serializable]
public class Map
{
    public Vector3 mapStartPos;
    /// <summary>
    /// 瓦片大小
    /// </summary>
    public ushort TileSize ;
    /// <summary>
    /// low sector 包含的tile大小
    /// </summary>
    public ushort SectorSize ;
    /// <summary>
    /// 父子扇区缩放比例
    /// </summary>
    public ushort SectorlevelScale ;
    /// <summary>
    /// 拥有的瓦片数组
    /// </summary>
    public MapTile[][] tileArray = null;
    /// <summary>
    /// 瓦片X的num
    /// </summary>
    public ushort TileXnum;
    /// <summary>
    /// 瓦片Z的num
    /// </summary>
    public ushort TileYnum;
    /// <summary>
    /// 扇区层级数
    /// </summary>
    public ushort SectorLevelNum;
    /// <summary>
    /// 记录扇区层次结构信息结构
    /// </summary>
    public SectorLevelInfo[] levelDimensions;

    /// <summary>
    /// 扇区描述数组，数组0:Level， 数组1: 扇区索引
    /// </summary>
    public MapSector[][] sectorArray = null;

    /// <summary>
    /// lookUpLowerSectors[sectorlevel-1][hih level sectorID]，最后[][]包含低层扇区的索引
    /// </summary>
    public SectorIndexList[][] lookUpLowerSectors;
    /// <summary>
    /// 各扇区层级下的way point点，
    /// 组成way point的数据为1：各个扇区的出入口 2：寻路起点及终点，3：子地图间的连接点。
    /// 各个扇区的出入口可以理解为静态数据，只有这个扇区内发生挡格变化才可能会起变化。
    /// 其中way point 为Dictionary<MapTile, PathNode> 的keys，也就MapTile
    /// </summary>
    private Dictionary<MapTile, PathNode>[] SectorLevelWayPointLink;
    /// <summary>
    /// 初始化一些地图参数
    /// </summary>


    public void Init_EditMode()
    {
        TileSize = 1;
        SectorSize = 20;
        TileXnum = 1200;
        TileYnum = 1200;
        SectorLevelNum = 2;
        SectorlevelScale = 5;
        mapStartPos = new Vector3(-0.5f * TileXnum, 0, -0.5f * TileYnum);
        InitAPI();
        BuildMap();
        BuildSectorWayPointCost();
    }

    public void Init_RumTimeMode(MapData data)
    {
        this.TileSize = data.TileSize;
        this.SectorSize = data.SectorSize;
        this.TileXnum = data.TileXnum;
        this.TileYnum = data.TileYnum;
        this.SectorLevelNum = data.SectorLevelNum;
        this.SectorlevelScale = data.SectorlevelScale;
        this.mapStartPos = new Vector3(-0.5f * TileXnum, 0, -0.5f * TileYnum);
        InitAPI();
        BuildMap();
        BuildSectorWayPointCost(data);
    }

    private void InitAPI()
    {
        TileHelp.Init(this);
        SectorHelp.Init(this);
        AStar.Init(this);
        FlowField.Init(this);
        JumpFlowFiled.Init(this);
        IggPathFinder.Init(this);
        MapChangeManger.Init(this);
    }

    /// <summary>
    /// 构建地图
    /// </summary>
    private void BuildMap()
    {
        // 构建tile
        tileArray = new MapTile[TileYnum][];
        for (short y = 0; y < TileYnum; y++)
        {
            tileArray[y] = new MapTile[TileXnum];
            for (short x = 0; x < TileXnum; x++)
            {
                MapTile tile = new MapTile();
                tile.Pos = new IntVector2(x, y);
                tile.SetSectorIndex(TileXnum, TileYnum, SectorSize);
                tile.integrationValue = TileHelp.tileResetIntegrationValue;
                tileArray[y][x] = tile;
            }
        }
        ushort sectorWidth = SectorSize;
        ushort sectorHeight = SectorSize;
        //
        SectorLevelWayPointLink = new Dictionary<MapTile, PathNode>[SectorLevelNum];
        lookUpLowerSectors = new SectorIndexList[SectorLevelNum - 1][];
        levelDimensions = new SectorLevelInfo[SectorLevelNum];
        sectorArray = new MapSector[SectorLevelNum][];
        for (ushort level = 0; level < SectorLevelNum; level++)
        {
            SectorLevelWayPointLink[level] = new Dictionary<MapTile, PathNode>();
            levelDimensions[level] = new SectorLevelInfo(sectorWidth, sectorHeight, 
                (ushort)(Mathf.CeilToInt((TileXnum / (float)sectorWidth))), (ushort)(Mathf.CeilToInt((TileYnum / (float)sectorHeight))));

            sectorArray[level] = new MapSector[levelDimensions[level].numWidth * levelDimensions[level].numHeight];
            for (short i = 0; i < levelDimensions[level].numHeight; i++)
            {
                for (short j = 0; j < levelDimensions[level].numWidth; j++)
                {
                    int index = (i * levelDimensions[level].numWidth) + j;
                    if (level == 0)
                    {
                        sectorArray[level][index] = new LowSector();
                    }
                    else
                    {
                        sectorArray[level][index] = new HighSector();
                    }
                    sectorArray[level][index].Pos = new IntVector2(j, i);
                    sectorArray[level][index].ID = (ushort)index;
                    sectorArray[level][index].level = level;
                    sectorArray[level][index].top = (ushort)(i * levelDimensions[level].sectorWidth);
                    sectorArray[level][index].bottom = (ushort)(i * levelDimensions[level].sectorWidth + levelDimensions[level].sectorWidth - 1);
                    sectorArray[level][index].left = (ushort)(j * levelDimensions[level].sectorHeight);
                    sectorArray[level][index].right = (ushort)(j * levelDimensions[level].sectorHeight + levelDimensions[level].sectorHeight - 1);
                    sectorArray[level][index].tilesInWidth = (ushort)(Mathf.Min(TileXnum - sectorArray[level][index].left, levelDimensions[level].sectorWidth));
                    sectorArray[level][index].tilesInHeight = (ushort)(Mathf.Min(TileYnum - sectorArray[level][index].top, levelDimensions[level].sectorHeight));
                    sectorArray[level][index].Init();
                }
            }
            // hight level 包含更多的格子数
            sectorWidth *= SectorlevelScale;
            sectorHeight *= SectorlevelScale;
            if (level > 0)
            {
                lookUpLowerSectors[level - 1] = new SectorIndexList[sectorArray[level].Length];
            }
        }
        FillInLookUpLowerSectors();
    }
    /// <summary>
    /// 构建sector way point cost,用于生成数据使用。
    /// </summary>
    private void BuildSectorWayPointCost()
    {
        for (int lev = 0; lev < SectorLevelNum; lev++)
        {
            foreach (MapSector sector in sectorArray[lev])
            {
                sector.BuildSectorEdgeWaypoint2Cost(this);
            }
        }
    }

    /// <summary>
    /// runtime 时使用
    /// </summary>
    private void BuildSectorWayPointCost(MapData data)
    {
        for (int lev = 0; lev < SectorLevelNum; lev++)
        {
            for (int i = 0; i < sectorArray[lev].Length; i++)
            {
                sectorArray[lev][i].BuildSectorEdgeWaypoint2Cost(this, data.sectorArray[lev].listms[i]);
            }
        }
    }


    private void FillInLookUpLowerSectors()
    {
        int s = 0;
        for (int i = 0; i < lookUpLowerSectors.Length; i++)
        {
            int level = i + 1;

            foreach (MapSector sector in sectorArray[level])
            {
                int lowerLevelX = sector.Pos.x * SectorlevelScale;
                int lowerLevelY = sector.Pos.y * SectorlevelScale;

                // get lower sector in the top left corner
                int lowerIndex = (lowerLevelY * levelDimensions[level - 1].numWidth) + lowerLevelX;

                int width = levelDimensions[level - 1].numWidth - lowerLevelX;
                int arrayWidth = Mathf.Min(width, SectorlevelScale);
                int height = levelDimensions[level - 1].numHeight - lowerLevelY;
                int arrayHeight = Mathf.Min(height, SectorlevelScale);
                lookUpLowerSectors[level - 1][sector.ID] = new SectorIndexList();

                // get surrounding sectors
                for (int x = 0; x < arrayWidth; x++)
                {
                    for (int y = 0; y < arrayHeight; y++)
                    {
                        s = lowerIndex + x + (y * levelDimensions[level - 1].numWidth);
                        lookUpLowerSectors[level - 1][sector.ID].AddLowSectorIndex((ushort)s);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 清理地图数据，占不实现
    /// </summary>
    public void Clear()
    {

    }

    /// <summary>
    /// 获取某层的扇区
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public MapSector[] GetLevelSectors(int level)
    {
        if (level < 0 || level >= sectorArray.Length)
            return null;
        else return sectorArray[level];
    }
    /// <summary>
    /// 获取某层指定索引的扇区
    /// </summary>
    /// <param name="level"></param>
    /// <param name="sectorIndex"></param>
    /// <returns></returns>
    public MapSector FindMapSector(int level, int sectorIndex)
    {
        MapSector[] ms = GetLevelSectors(level);
        if (ms != null && ms.Length > sectorIndex && sectorIndex >= 0)
        {
            return ms[sectorIndex];
        }
        else  return null;
    }
    /// <summary>
    /// 根据坐标获取扇区
    /// </summary>
    /// <returns></returns>
    public MapSector FindMapSector(int level, IntVector2 pos)
    {
        int sectorIndex = levelDimensions[level].numWidth * pos.y + pos.x;
        return FindMapSector(level, sectorIndex);
    }
    /// <summary>
    /// 获取邻居扇区
    /// </summary>
    /// <param name="neighour"></param>
    /// <returns></returns>
    public MapSector Findneighbour(MapSector cur, Side neighour)
    {
        if (cur == null)
            return null;

        if (neighour == Side.Top)
        {
            return FindMapSector(cur.level, cur.Pos.Top);
        }
        else if (neighour == Side.Down)
        {
            return FindMapSector(cur.level, cur.Pos.Down);
        }
        else if (neighour == Side.Left)
        {
            return FindMapSector(cur.level, cur.Pos.Left);
        }
        else if (neighour == Side.Right)
        {
            return FindMapSector(cur.level, cur.Pos.Right);
        }
        return null;
    }
    /// <summary>
    /// 获取邻居扇区
    /// </summary>
    /// <param name="neighour"></param>
    /// <returns></returns>
    public MapSector Findneighbour(MapSector cur, IntVector2 neighour)
    {
        if (cur == null)
            return null;
        if (neighour == IntVector2.top)
        {
            return FindMapSector(cur.level, cur.Pos.Top);
        }
        else if (neighour == IntVector2.down)
        {
            return FindMapSector(cur.level, cur.Pos.Down);
        }
        else if (neighour == IntVector2.left)
        {
            return FindMapSector(cur.level, cur.Pos.Left);
        }
        else if (neighour == IntVector2.right)
        {
            return FindMapSector(cur.level, cur.Pos.Right);
        }
        return null;
    }

    /// <summary>
    /// 获取邻居扇区
    /// </summary>
    /// <param name="neighour"></param>
    /// <returns></returns>
    public List<MapSector> FindneighbourMapSector(MapSector cur)
    {
        if (cur == null)
            return null;
        List<MapSector> ListMs = new List<MapSector>();
        MapSector ms = Findneighbour(cur, IntVector2.top);
        if (ms != null)
        {
            ListMs.Add(ms);
        }
        ms = Findneighbour(cur, IntVector2.down);
        if (ms != null)
        {
            ListMs.Add(ms);
        }
        ms = Findneighbour(cur, IntVector2.left);
        if (ms != null)
        {
            ListMs.Add(ms);
        }
        ms = Findneighbour(cur, IntVector2.right);
        if (ms != null)
        {
            ListMs.Add(ms);
        }
        return ListMs;
    }

    /// <summary>
    /// 获取邻居扇区的索引
    /// </summary>
    /// <param name="neighour"></param>
    /// <returns></returns>
    public List<int> FindneighbourMapSectorID(MapSector cur)
    {
        List<int> lID = new List<int>();
        List<MapSector> l = FindneighbourMapSector(cur);
        if (l == null || l.Count == 0)
            return null;
        foreach (MapSector ms in l)
        {
            lID.Add(ms.ID);
        }
        return lID;
    }

    /// <summary>
    /// 获取指定扇区的子扇区
    /// </summary>
    /// <param name="curSector"></param>
    /// <returns></returns>
    public List<MapSector> GetChildSector(MapSector curSector)
    {
        List<MapSector> listmc = new List<MapSector>();
        List<ushort> list = GetChildSectorindex(curSector.level, curSector.ID);
        if (list == null)
            return null;
        foreach (ushort index in list)
        {
            MapSector mc = FindMapSector(curSector.level - 1, index);
            if (mc != null)
            {
                listmc.Add(mc);
            }
        }
        return listmc;
    }

    /// <summary>
    /// 获取父扇区
    /// </summary>
    /// <param name="sector"></param>
    /// <returns></returns>
    public MapSector GetParentMapSector(MapSector sector)
    {
        if (sector == null && sector.level >=SectorLevelNum -1)
            return null;
        int x = Mathf.FloorToInt(sector.left / (float)levelDimensions[sector.level + 1].sectorWidth);
        int y = Mathf.FloorToInt(sector.top / (float)levelDimensions[sector.level + 1].sectorHeight);
        return FindMapSector(sector.level + 1, new IntVector2((short)x, (short)y));
    }

    /// <summary>
    /// 获取瓦片所在的扇区
    /// </summary>
    /// <param name="level">扇区层次</param>
    /// <param name="tile">瓦片</param>
    /// <returns></returns>
    public MapSector FindSectorOfTile(int level, MapTile tile)
    {
        if (tile == null)
            return null;
        return tile.GetMapSector(level, this);
    }

    /// <summary>
    /// 根据low 扇区索引，获取扇区的坐标
    /// </summary>
    /// <param name="SectorID"></param>
    /// <returns></returns>
    public IntVector2 GetLowSectorPos(int SectorID)
    {
        int x = SectorID / levelDimensions[0].numWidth;
        int y = SectorID % levelDimensions[0].numHeight;
        return new IntVector2(x , y);
    }


    /// <summary>
    /// 获取父扇区列比奥
    /// </summary>
    /// <param name="list"></param>
    /// <param name="curLev"></param>
    /// <returns></returns>
    public List<MapSector> GetParentSectorList(List<MapSector> list, int curLev)
    {
        // 列表为空或已经是最大等级了
        if (list == null || list.Count == 0 || curLev >= SectorLevelNum - 1)
            return null;
        List<MapSector> l = new List<MapSector>();
        foreach (MapSector mc in list)
        {
            if (mc != null && mc.level == curLev)
            {
                MapSector high = GetParentMapSector(mc);
                if (l.Contains(high) == false)
                {
                    l.Add(high);
                }
            }
        }
        return l;
    }

    /// <summary>
    /// 获取受到影响下一级高级相领扇区
    /// </summary>
    /// <param name="list"></param>
    /// <param name="curLev"></param>
    /// <param name="LevelNum"></param>
    /// <returns></returns>
    public List<NeighbourSector> GetParentNeighbourSectorList(List<NeighbourSector> list, int curLev)
    {
        // 列表为空或已经是最大等级了
        if (list == null || list.Count == 0 || curLev >= SectorLevelNum - 1)
            return null;
        List<NeighbourSector> l = new List<NeighbourSector>();
        foreach (NeighbourSector mc in list)
        {
            if (mc.A != null && mc.B != null && mc.A.level == curLev && mc.B.level == curLev)
            {
                MapSector high1 = GetParentMapSector(mc.A);
                MapSector high2 = GetParentMapSector(mc.B);
                if (high1 != high2)
                {
                    NeighbourSector next = new NeighbourSector(high1, high2, mc.side);
                    if (l.Contains(next) == false)
                    {
                        l.Add(next);
                    }
                }
            }
        }
        return l;
    }

    /// <summary>
    /// 获取所有子扇区的索引
    /// </summary>
    /// <returns></returns>
    public List<ushort> GetChildSectorindex(ushort sectorLevel, ushort sectorID)
    {
        if (sectorLevel == 0)
            return null;
        SectorIndexList sectorIndexes = lookUpLowerSectors[sectorLevel - 1][sectorID];
        return sectorIndexes.lowSectorIndex;
    }
    /// <summary>
    /// 获取指定扇区的子扇区
    /// </summary>
    /// <param name="curSector"></param>
    /// <returns></returns>
    public List<ushort> GetChildSectorindex(MapSector curSector)
    {
        if (curSector == null || curSector.level == 0)
        {
            return null;
        }
        else
        {
            return GetChildSectorindex(curSector.level, curSector.ID);
        }
    }

    /// <summary>
    /// 根据坐标获取瓦片
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public MapTile GetMapTile(int x, int y)
    {
        if (x < 0 || y < 0 || x > TileXnum - 1 || y > TileYnum - 1)
        {
            return null;
        }
        else
            return tileArray[y][x];
    }

    /// <summary>
    /// 根据坐标获取瓦片
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public MapTile GetMapTile(IntVector2 pos)
    {
        return GetMapTile(pos.x, pos.y);
    }

    /// <summary>
    /// 安全获取tile
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public MapTile GetMapTileSafe(IntVector2 pos)
    {
        //return tileArray[pos.y][pos.x];
        return GetMapTile(pos);
    }

    /// <summary>
    /// 安全获取tile
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public MapTile GetMapTileSafe(int x, int y)
    {
        //return tileArray[y][x];
        return GetMapTile(x, y);
    }
    /// <summary>
    /// 获取指定扇区层级level的way point图
    /// </summary>
    /// <param name="sectorLevel"></param>
    /// <returns></returns>
    private Dictionary<MapTile, PathNode> GetSectorLevelWayPointLink(int sectorLevel)
    {
        if (SectorLevelWayPointLink == null || SectorLevelWayPointLink.Length == 0 || SectorLevelWayPointLink.Length <= sectorLevel || sectorLevel < 0)
            return null;
        else return SectorLevelWayPointLink[sectorLevel];
    }

    /// <summary>
    /// 在指定的扇区层级Wap point数据中删除指定的way point点，一个way point可能又多个属性，
    /// </summary>
    /// <param name="sectorLevel">扇区层级</param>
    /// <param name="GateNode">扇区出入口节点</param>
    public void RemoveWayPointNode(int sectorLevel, PathNode WayPointNode)
    {
        Dictionary<MapTile, PathNode> WayPointGraph = GetSectorLevelWayPointLink(sectorLevel);
        if (WayPointGraph == null || WayPointNode == null || WayPointNode.tileConnection == null)
            return;
        //清理连接关系。
        foreach (PathNode n in WayPointNode.connections.Keys)
        {
            if (n != null && n.connections != null)
            {
                if (n.connections.ContainsKey(WayPointNode) == true)
                {
                    n.connections.Remove(WayPointNode);
                }
            }
        }
        WayPointNode.connections.Clear();
        WayPointNode.connections = null;
        //
        MapTile tile = WayPointNode.tileConnection;
        if (WayPointGraph.ContainsKey(tile) == true)
        {
            WayPointGraph.Remove(tile);
            tile.hasPathNodeConnection = false;
            MapSector s = FindMapSector(sectorLevel, WayPointNode.sector);
            if (s != null)
            {
                s.RemovePathNodeNum();
            }
        }
    }

    /// <summary>
    /// 在指定的扇区层级添加way point节点
    /// </summary>
    /// <param name="sectorLevel">扇区层级</param>
    /// <param name="WayPointNode">扇区出入口节点</param>
    public void AddWayPointNode(int sectorLevel, PathNode WayPointNode)
    {
        Dictionary<MapTile, PathNode> WayPointGraph = GetSectorLevelWayPointLink(sectorLevel);
        if (WayPointGraph == null || WayPointNode == null || WayPointNode.tileConnection == null)
            return;

        MapTile tile = WayPointNode.tileConnection;
        if (WayPointGraph.ContainsKey(tile) == false)
        {
            WayPointGraph.Add(tile, WayPointNode);
        }            
    }
    /// <summary>
    /// 获取指定way point相关联的path node
    /// </summary>
    /// <param name="sectorLevel"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public PathNode GetWayPointPathNode(int sectorLevel, MapTile tile)
    {
        Dictionary<MapTile, PathNode> WayPointGraph = GetSectorLevelWayPointLink(sectorLevel);
        if (WayPointGraph == null || tile == null)
            return null;
        if (WayPointGraph.ContainsKey(tile) == true)
        {
            return WayPointGraph[tile];
        }
        return null;
    }

    /// <summary>
    /// 构建扇区edge及way point
    /// </summary>
    public void BuildSectorEdge2WayPoint()
    {
        if (sectorArray == null)
            return;
        for (int level = 0; level < SectorLevelNum; level++)
        {
            for (int sectorIndex = 0; sectorIndex < sectorArray[level].Length; sectorIndex++)
            {
                sectorArray[level][sectorIndex].BuildSectorEdgeWaypoint2Cost(this);
            }
        }
    }

    public MapTile GetMapTile(Vector3 location)
    {
        int x = (int)((location.x - mapStartPos.x) / TileSize);
        int z = (int)(((location.z - mapStartPos.z) / TileSize));
        return GetMapTile(x, z);
    }

    public Vector3 GetMapTileWorldPos(MapTile tile)
    {
        if (tile == null)
        {
            return mapStartPos;
        }
        else return mapStartPos + 
                new Vector3(tile.Pos.x * TileSize, 0, tile.Pos.y * TileSize) + 
                new Vector3(0.5f * TileSize, 0, 0.5f * TileSize);
    }

    public void DrawGizmos()
    {
        return;
        // 绘制地图范围
        
        Gizmos.DrawLine(mapStartPos, mapStartPos + new Vector3(TileXnum * TileSize, 0, 0));
        Gizmos.DrawLine(mapStartPos, mapStartPos + new Vector3(0,0,TileXnum * TileSize));
        Gizmos.DrawLine(mapStartPos + new Vector3(TileXnum * TileSize, 0, 0), mapStartPos + new Vector3(TileXnum * TileSize, 0, TileXnum * TileSize));
        Gizmos.DrawLine(mapStartPos + new Vector3(0, 0, TileXnum * TileSize), mapStartPos + new Vector3(TileXnum * TileSize, 0, TileXnum * TileSize));
        
        // 绘制地图扇区
        Vector3 start;
        int level = 0;
        foreach (MapSector sector in sectorArray[level])
        {
            start = mapStartPos + new Vector3((sector.left * TileSize), 0, ((sector.top * TileSize)));
            float width = levelDimensions[level].sectorWidth * TileSize;
            float length = levelDimensions[level].sectorHeight * TileSize;

            Gizmos.DrawLine(start, start + new Vector3(width, 0, 0));
            Gizmos.DrawLine(start + new Vector3(width, 0, 0), start + new Vector3(width, 0, length));
            Gizmos.DrawLine(start + new Vector3(width, 0, length), start + new Vector3(0, 0, length));
            Gizmos.DrawLine(start + new Vector3(0, 0, length), start);
        }
        // way point 联通网络
        foreach (MapSector sector in sectorArray[level])
        {
            Gizmos.color = Color.black;

            List<PathNode> l = sector.GetPathNodeInEdge();
            foreach (PathNode node in l)
            {
                foreach (PathNode v in node.connections.Keys)
                {
                    Gizmos.DrawLine(GetMapTileWorldPos(node.tileConnection), GetMapTileWorldPos(v.tileConnection));
                }
            }
        }
    }


}
