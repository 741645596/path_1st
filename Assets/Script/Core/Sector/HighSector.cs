using System.Collections.Generic;

/// <summary>
/// 低级扇区，第1层及以上扇区，包含子扇区
/// </summary>
public class HighSector : MapSector
{
    /// <summary>
    /// 高等级扇区way point建立，使用A* way point算法
    /// </summary>
    protected override void CreateOneWayPointCost(PathNode start, Map map, int maxRoad)
    {
        if (map == null || start == null)
            return;
        List<PathNode> listAllGate = GetPathNodeInEdge();
        if (listAllGate != null && listAllGate.Count > 0)
        {
            int lowerLevel = level - 1;
            List<ushort> listSectorIndexes = map.GetChildSectorindex(level, this.ID);
            PathNode LowLevelStart = map.GetWayPointPathNode(lowerLevel, start.tileConnection);
            if (LowLevelStart == null)
                return;
            foreach (PathNode dest in listAllGate)
            {
                if (start == dest || start.connections.ContainsKey(dest))
                    continue;
                PathNode LowLeveldest = map.GetWayPointPathNode(lowerLevel, dest.tileConnection);
                if (LowLeveldest == null)
                    continue;
                // way point astar 寻路
                int distance = AStar.SearchPathWayPointRoadDistance(LowLevelStart, LowLeveldest, listSectorIndexes);
                // 判断能否走通
                if (distance != -1)
                {
                    PathNode.LinkSectorNode(start, dest, distance, null);
                }
            }
        }
    }
    /// <summary>
    /// 构建扇区某条边的gate
    /// </summary>
    /// <param name="map"></param>
    public override void BuildOneEdgeGate(Side edge, Map map)
    {
        // 获取邻居扇区
        Side neighbourEdge = Util.FlipDirection(edge);
        MapSector ms = Findneighbour(edge, map);
        if (ms == null)
            return;
        List<MapSector> listChild = map.GetChildSector(this);
        if (listChild == null || listChild.Count == 0)
            return;

        foreach (MapSector lowerSector in listChild)
        {
            // 先判断父子扇区在指定的边是否匹配，核心目的是为了得到父扇区的边。
            if (SectorHelp.LowerSectorEdgeMatchesHigher(this, lowerSector, edge)) // match edge
            {
                List<PathNode> listPathNode = lowerSector.GetPathNodeInEdge(edge);
                foreach (PathNode node in listPathNode) // get nodes to copy from the edge
                {
                    SectorHelp.CreateGate(this, edge, node.tileConnection, ms, neighbourEdge, node.LinkOtherSectorGate.tileConnection);
                }
            }
        }
    }
}
