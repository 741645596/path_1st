using System;

public class JumpPathNode : IHeapItem<JumpPathNode>
{
    public JumpPathNode()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="parent"></param>
    /// <param name="straightDirArray">直线方向</param>
    /// <param name="diagonalDirArray">对角方向</param>
    public JumpPathNode(IntVector2 tilePos, JumpPathNode parent, 
        JumpDirections[] straightDirArray, int straightDirCount, 
        JumpDirections[] diagonalDirArray, int diagonalDirCount)
    {
        Pos = tilePos;
        Parent = parent;
        if (straightDirArray != null && straightDirCount > 0)
        {
            StraightDirNum = straightDirCount;
            for (int i = 0; i < StraightDirNum; i++)
            {
                StraightDirArray[i] = straightDirArray[i];
            }
            
        }
        if (diagonalDirArray != null && diagonalDirCount > 0)
        {
            DiagonalDirNum = diagonalDirCount;
            for (int i = 0; i < DiagonalDirNum; i++)
            {
                DiagonalDirArray[i] = diagonalDirArray[i];
            }
        }
        CalcMid2IntegrationValue();
    }

    /// <summary>
    /// 计算中间点。
    /// </summary>
    private void CalcMid2IntegrationValue()
    {
        Mid = IntVector2.invalid;
        IntegrationValue = 0;
        if (Parent == null)
            return;
        IntVector2 diff = Parent.Pos - Pos;
        if (Math.Abs(diff.x) == Math.Abs(diff.y)) // 对角方向
        {
            IntegrationValue = Parent.IntegrationValue + Math.Abs(diff.x) * TileHelp.DiagonalAddIntegrationValue;
        }
        else if (diff.x == 0 || diff.y == 0)  // 直线方向
        {
            IntegrationValue = Parent.IntegrationValue + (Math.Abs(diff.x) + Math.Abs(diff.y)) * TileHelp.StraightAddIntegrationValue;
        }
        else
        {
            if (Math.Abs(diff.y) > Math.Abs(diff.x))
            {
                IntegrationValue = Parent.IntegrationValue + Math.Abs(diff.x) * TileHelp.DiagonalAddIntegrationValue;
                IntegrationValue +=(Math.Abs(diff.y) - Math.Abs(diff.x)) * TileHelp.StraightAddIntegrationValue;
                // 计算中间点
                short x = Pos.x;
                int y = diff.y > 0 ? Parent.Pos.y + Math.Abs(diff.x) : Parent.Pos.y - Math.Abs(diff.x);
                Mid = new IntVector2(x, y);
            }
            else
            {
                IntegrationValue = Parent.IntegrationValue + Math.Abs(diff.y) * TileHelp.DiagonalAddIntegrationValue;
                IntegrationValue += (Math.Abs(diff.x) - Math.Abs(diff.y)) * TileHelp.StraightAddIntegrationValue;
                // 计算中间点。
                short y = Pos.y;
                int x = diff.x > 0 ? Parent.Pos.x + Math.Abs(diff.y) : Parent.Pos.x - Math.Abs(diff.y);
                Mid = new IntVector2(x, y);
            }
        }
    }
    /// <summary>
    /// 连接的Tile位置
    /// </summary>
    public IntVector2 Pos;
    /// <summary>
    /// 跟父节点之间的中间点。
    /// </summary>
    public IntVector2 Mid = IntVector2.invalid;
    /// <summary>
    /// 反推路径点需要，知道前一个路径点哪里来的。
    /// </summary>
    public JumpPathNode Parent = null;

    public int IntegrationValue = 0;
    /// <summary>
    /// 直线方向
    /// </summary>
    public JumpDirections[] StraightDirArray = new JumpDirections[4];
    /// <summary>
    /// 直线方向数量
    /// </summary>
    public int StraightDirNum = 0;
    /// <summary>
    /// 对角方向
    /// </summary>
    public JumpDirections[] DiagonalDirArray = new JumpDirections[4];
    /// <summary>
    /// 对角方向数量
    /// </summary>
    public int DiagonalDirNum = 0;




    private int heapIndex;
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }


    public int CompareTo(JumpPathNode nodeToCompare)
    {
        int compare = IntegrationValue.CompareTo(nodeToCompare.IntegrationValue);
        return -compare;
    }


}


/// <summary>
/// 定义8方向
/// </summary>
public enum JumpDirections
{
    Top = 0,
    RightTop = 1,
    Right = 2,
    RightDown = 3,
    Down = 4,
    LeftDown = 5,
    Left = 6,
    LeftTop = 7,
}
