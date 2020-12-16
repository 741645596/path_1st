using System.Collections.Generic;
using System;

/// <summary>
/// 跳点流场寻路算法。
/// </summary>
public class JumpFlowFiled 
{
    private static int g_MaxSearchRoad = 0;
    private static Heap<JumpPathNode> g_openSet = null;
    private static HashSet<JumpPathNode> g_closedSet = null;
    private static Map g_map = null;
    private static IntVector2[] g_DirArray = new IntVector2[8] 
                  {IntVector2.top,
                   IntVector2.top + IntVector2.right,
                   IntVector2.right,
                   IntVector2.right + IntVector2.down,
                   IntVector2.down,
                   IntVector2.down + IntVector2.left,
                   IntVector2.left,
                   IntVector2.left + IntVector2.top};
    /// <summary>
    /// 直线方向
    /// </summary>
    private static JumpDirections[] g_StraightDirArray = new JumpDirections[4];
    /// <summary>
    /// 直线方向数量
    /// </summary>
    private static int g_StraightDirNum = 0;
    /// <summary>
    /// 对角方向
    /// </summary>
    private static JumpDirections[] g_DiagonalDirArray = new JumpDirections[4];
    /// <summary>
    /// 对角方向数量
    /// </summary>
    private static int g_DiagonalDirNum = 0;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="map"></param>
    public static void Init(Map map)
    {
        g_map = map;
        g_openSet = new Heap<JumpPathNode>(100);
        g_closedSet = new HashSet<JumpPathNode>();
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
    /// jump 搜寻路点
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public static void CalcSectorWayPointCost(PathNode start, int MaxSearchRoad)
    {
        g_MaxSearchRoad = MaxSearchRoad;
        ClearCacheData();
        g_StraightDirNum = 0;
        g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Top;
        g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Right;
        g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Down;
        g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Left;
        g_DiagonalDirNum = 0;
        g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightTop;
        g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightDown;
        g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftDown;
        g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftTop;
        JumpPathNode jumpStart = new JumpPathNode(start.tileConnection.Pos, null, g_StraightDirArray, g_StraightDirNum, g_DiagonalDirArray, g_DiagonalDirNum);
        g_openSet.Add(jumpStart);
        while (g_openSet.Count > 0 && g_MaxSearchRoad > 0)
        {
            JumpPathNode currentNode = g_openSet.RemoveFirst();
            g_closedSet.Add(currentNode);
            // 先直角方向
            for (int i = 0; i < currentNode.StraightDirNum && g_MaxSearchRoad > 0; i++)
            {
                DoStraightDirJump(start, jumpStart, currentNode.Pos, start.tileConnection.sectorIndex, currentNode.StraightDirArray[i], currentNode);
            }
            // 再斜角方向
            for (int i = 0; i < currentNode.DiagonalDirNum && g_MaxSearchRoad > 0; i++)
            {
                DoDiagonalDirJump(start, jumpStart, currentNode.Pos, start.tileConnection.sectorIndex, currentNode.DiagonalDirArray[i], currentNode);
            }
        }
    }
    /// <summary>
    /// 直线方向jump
    /// </summary>
    private static void DoStraightDirJump(PathNode start, JumpPathNode jStart, IntVector2 startPos, ushort sectorIndex, JumpDirections dir, JumpPathNode parent)
    {
        IntVector2 Dir = g_DirArray[(int)dir];
        MapTile Cur = g_map.GetMapTile(startPos + Dir);
        while (Cur != null && Cur.blocked == false && Cur.sectorIndex == sectorIndex)
        {
            MapTile next = g_map.GetMapTile(Cur.Pos + Dir);
            // 判断出界或阻断。
            if (next == null || next.blocked == true || next.sectorIndex != sectorIndex)
            {
                break;
            }
            if (next.hasPathNodeConnection)
            {
                SetPathResult(jStart, parent, start, g_map.GetWayPointPathNode(0, next));
                g_MaxSearchRoad--;
                if (g_MaxSearchRoad <= 0)
                    break;
            }

            // 判断是否有强迫邻居
            if (CheckHaveForceNeighborsStraightDir(Cur.Pos, sectorIndex, dir) == true)
            {
                JumpPathNode j = new JumpPathNode(Cur.Pos, parent, g_StraightDirArray, g_StraightDirNum, g_DiagonalDirArray, g_DiagonalDirNum);
                g_openSet.Add(j);
                g_openSet.UpdateItem(j);
                break;
            }
            else
            {
                Cur = next;
            }
        }
    }
    /// <summary>
    /// 判断直角方向是否有强制邻居
    /// </summary>
    /// <returns></returns>
    private static bool CheckHaveForceNeighborsStraightDir(IntVector2 startPos, ushort sectorIndex, JumpDirections dir)
    {
        bool ret = false;
        g_StraightDirNum = 0;
        g_DiagonalDirNum = 0;
        if (dir == JumpDirections.Right)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.right, IntVector2.top) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightTop;
                ret = true;
            }
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.right, IntVector2.down) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightDown;
                ret = true;
            }
        }
        else if (dir == JumpDirections.Left)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.left, IntVector2.top) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftTop;
                ret = true;
            }
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.left, IntVector2.down) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftDown;
                ret = true;
            }
        }
        else if (dir == JumpDirections.Top)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.top, IntVector2.left) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftTop;
                ret = true;
            }
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.top, IntVector2.right) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightTop;
                ret = true;
            }
        }
        else if (dir == JumpDirections.Down)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.down, IntVector2.left) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftDown;
                ret = true;
            }
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.down, IntVector2.right) == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightDown;
                ret = true;
            }
        }
        if (ret == true)
        {
            g_StraightDirArray[g_StraightDirNum++] = dir;
        }
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Pos"></param>
    /// <param name="sectorIndex"></param>
    /// <param name="Parent"></param>
    /// <param name="Neighbors"></param>
    /// <returns></returns>
    private static bool CheckForceNeighbors(IntVector2 Pos, ushort sectorIndex, IntVector2 Parent, IntVector2 Neighbors)
    {
        MapTile t1 = g_map.GetMapTile(Pos + Neighbors);
        if (t1 != null && t1.blocked == true && t1.sectorIndex == sectorIndex)
        {
            MapTile temp = g_map.GetMapTile(Pos + Neighbors);
            if (temp != null && temp.sectorIndex == sectorIndex && temp.blocked == false)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 对角方向jump
    /// </summary>
    private static void DoDiagonalDirJump(PathNode start, JumpPathNode jStart, IntVector2 startPos, ushort sectorIndex, JumpDirections dir, JumpPathNode parent)
    {
        IntVector2 Dir = g_DirArray[(int)dir];
        MapTile Cur = g_map.GetMapTile(startPos + Dir);
        while (Cur != null && Cur.blocked == false && Cur.sectorIndex == sectorIndex)
        {
            if (Cur.hasPathNodeConnection)
            {
                SetPathResult(jStart, parent, start, g_map.GetWayPointPathNode(0, Cur));
                if (g_MaxSearchRoad <= 0)
                    break;
            }
            // 先判断是否为强制邻居
            if (CheckHaveForceNeighborsDiagonalDir(Cur.Pos, sectorIndex, dir) == true)
            {
                JumpPathNode j = new JumpPathNode(Cur.Pos, parent, g_StraightDirArray, g_StraightDirNum, g_DiagonalDirArray, g_DiagonalDirNum);
                g_openSet.Add(j);
                g_openSet.UpdateItem(j);
                break;
            }
            if (dir == JumpDirections.LeftDown)
            {
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Left, parent);
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Down, parent);
            }
            else if (dir == JumpDirections.LeftTop)
            {
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Left, parent);
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Top, parent);
            }
            else if (dir == JumpDirections.RightDown)
            {
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Right, parent);
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Down, parent);
            }
            else if (dir == JumpDirections.RightTop)
            {
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Right, parent);
                DoStraightDirJump(start, jStart, Cur.Pos, sectorIndex, JumpDirections.Top, parent);
            }
            // 对角通路问题
            if (CheckDiagonalDirCross(Cur.Pos, dir) == false)
                break;
            Cur = g_map.GetMapTile(Cur.Pos + Dir);
        }
    }
    /// <summary>
    /// 判断斜角是通路
    /// </summary>
    /// <returns></returns>
    private static bool CheckDiagonalDirCross(IntVector2 Pos, JumpDirections dir)
    {
        MapTile t1 = null;
        MapTile t2 = null;
        if (dir == JumpDirections.RightTop)
        {
            t1 = g_map.GetMapTile(Pos + IntVector2.top);
            t2 = g_map.GetMapTile(Pos + IntVector2.right);

        }
        else if (dir == JumpDirections.RightDown)
        {
            t1 = g_map.GetMapTile(Pos + IntVector2.down);
            t2 = g_map.GetMapTile(Pos + IntVector2.right);
        }
        else if (dir == JumpDirections.LeftTop)
        {
            t1 = g_map.GetMapTile(Pos + IntVector2.top);
            t2 = g_map.GetMapTile(Pos + IntVector2.left);
        }
        else if (dir == JumpDirections.LeftDown)
        {
            t1 = g_map.GetMapTile(Pos + IntVector2.left);
            t2 = g_map.GetMapTile(Pos + IntVector2.down);
        }
        if (t1 != null && t1.blocked == true && t2 != null && t2.blocked == true)
        {
            return false;
        }
        else return true;
    }
    /// <summary>
    /// 判断对角方向是否有强制邻居
    /// </summary>
    /// <returns></returns>
    private static bool CheckHaveForceNeighborsDiagonalDir(IntVector2 startPos, ushort sectorIndex, JumpDirections dir)
    {
        bool ret = false;
        g_StraightDirNum = 0;
        g_DiagonalDirNum = 0;
        JumpDirections AddDir = dir;
        if (dir == JumpDirections.RightTop)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.rightTop, IntVector2.left) == true)
            {
                AddDir = JumpDirections.LeftTop;
                ret = true;
            }
            else if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.rightTop, IntVector2.down) == true)
            {
                AddDir = JumpDirections.RightDown;
                ret = true;
            }
            if (ret == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightTop;
                g_DiagonalDirArray[g_DiagonalDirNum++] = AddDir;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Top;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Right;
            }
        }
        else if (dir == JumpDirections.RightDown)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.rightDown, IntVector2.left) == true)
            {
                AddDir = JumpDirections.LeftDown;
                ret = true;
            }
            else if(CheckForceNeighbors(startPos, sectorIndex, IntVector2.rightDown, IntVector2.top) == true)
            {
                AddDir = JumpDirections.RightTop;
                ret = true;
            }
            if (ret == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.RightDown;
                g_DiagonalDirArray[g_DiagonalDirNum++] = AddDir;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Down;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Right;
            }
        }
        else if (dir == JumpDirections.LeftTop)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.leftTop, IntVector2.right) == true)
            {
                AddDir = JumpDirections.RightTop;
                ret = true;
            }
            else if(CheckForceNeighbors(startPos, sectorIndex, IntVector2.leftTop, IntVector2.down) == true)
            {
                AddDir = JumpDirections.LeftDown;
                ret = true;
            }
            if (ret == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftTop;
                g_DiagonalDirArray[g_DiagonalDirNum++] = AddDir;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Top;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Left;
            }
        }
        else if (dir == JumpDirections.LeftDown)
        {
            if (CheckForceNeighbors(startPos, sectorIndex, IntVector2.leftDown, IntVector2.right) == true)
            {
                AddDir = JumpDirections.RightDown;
                ret = true;
            }
            else if(CheckForceNeighbors(startPos, sectorIndex, IntVector2.leftDown, IntVector2.top) == true)
            {
                AddDir = JumpDirections.LeftTop;
                ret = true;
            }
            if (ret == true)
            {
                g_DiagonalDirArray[g_DiagonalDirNum++] = JumpDirections.LeftDown;
                g_DiagonalDirArray[g_DiagonalDirNum++] = AddDir;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Down;
                g_StraightDirArray[g_StraightDirNum++] = JumpDirections.Left;
            }
        }
        return ret;
    }

    /// <summary>
    /// 获取线路
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="lastNode"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private static void SetPathResult(JumpPathNode startNode, JumpPathNode lastNode, PathNode start, PathNode end)
    {
        int distance = 0;
        // 线路经过的中间节点
        List<IntVector2> l = new List<IntVector2>();
        // 先处理node 到curNode的线路
        IntVector2 diff = end.tileConnection.Pos - lastNode.Pos;
        if (Math.Abs(diff.x) == Math.Abs(diff.y)) // 对角方向
        {
            distance = Math.Abs(diff.x) * TileHelp.DiagonalAddIntegrationValue;
        }
        else if (diff.x == 0 || diff.y == 0)  // 直线方向
        {
            distance =  (Math.Abs(diff.x) + Math.Abs(diff.y)) * TileHelp.StraightAddIntegrationValue;
        }
        else
        {
            if (Math.Abs(diff.y) > Math.Abs(diff.x))
            {
                distance = Math.Abs(diff.x) * TileHelp.DiagonalAddIntegrationValue;
                distance += (Math.Abs(diff.y) - Math.Abs(diff.x)) * TileHelp.StraightAddIntegrationValue;
                // 计算中间点
                short x = end.tileConnection.Pos.x;
                int y = diff.y > 0 ? lastNode.Pos.y + Math.Abs(diff.x) : lastNode.Pos.y - Math.Abs(diff.x);
                l.Add(new IntVector2(x, y));
            }
            else
            {
                distance = Math.Abs(diff.y) * TileHelp.DiagonalAddIntegrationValue;
                distance += (Math.Abs(diff.x) - Math.Abs(diff.y)) * TileHelp.StraightAddIntegrationValue;
                // 计算中间点。
                short y = end.tileConnection.Pos.y;
                int x = diff.x > 0 ? lastNode.Pos.x + Math.Abs(diff.y) : lastNode.Pos.x - Math.Abs(diff.y);
                l.Add(new IntVector2(x, y));
            }
        }
        JumpPathNode t = lastNode;
        // 然后处理curNode 到start的线路
        while (t != null && t != startNode)
        {
            distance += t.IntegrationValue;
            l.Add(t.Pos);
            if (t.Mid != IntVector2.invalid)
            {
                l.Add(t.Mid);
            }
            t = t.Parent;
        }
        l.Reverse();
        PathNode.LinkSectorNode(start, end, distance, l);
    }
}


