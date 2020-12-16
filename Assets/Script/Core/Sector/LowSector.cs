/// <summary>
/// 低级扇区，第0层扇区，不包含子扇区
/// </summary>
public class LowSector : MapSector
{
    /// <summary>
    /// 创建way point cost
    /// </summary>
    protected override void CreateOneWayPointCost(PathNode start, Map map, int maxRoad)
    {
        if (start == null)
            return;
        //FlowField.CalcSectorWayPointCost(start);
        JumpFlowFiled.CalcSectorWayPointCost(start, maxRoad);
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
        if (edge == Side.Top)
        {
            SectorHelp.RebuildNodesOnSectorEdge(this, edge, new IntVector2(this.left, this.top),
                            ms, neighbourEdge, new IntVector2(this.left, this.top - 1),
                            IntVector2.right);
        }
        else if (edge == Side.Down)
        {
            SectorHelp.RebuildNodesOnSectorEdge(this, edge, new IntVector2(this.left, this.bottom),
                                    ms, neighbourEdge, new IntVector2(this.left, this.bottom + 1),
                                    IntVector2.right);
        }
        else if (edge == Side.Left)
        {
            SectorHelp.RebuildNodesOnSectorEdge(this, edge, new IntVector2(this.left, this.top),
                                                    ms, neighbourEdge, new IntVector2(this.left - 1, this.top),
                                                    IntVector2.down);
        }
        else if (edge == Side.Right)
        {
            SectorHelp.RebuildNodesOnSectorEdge(this, edge, new IntVector2(this.right, this.top),
                                                    ms, neighbourEdge, new IntVector2(this.right + 1, this.top),
                                                    IntVector2.down);
        }
    }
}
