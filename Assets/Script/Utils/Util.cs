
public static class Util
{
    /// <summary>
    /// 获取反方向
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static Side FlipDirection(Side dir)
    {
        if (dir == Side.Top)
            return Side.Down;
        else if (dir == Side.Down)
            return Side.Top;
        else if (dir == Side.Left)
            return Side.Right;
        else if (dir == Side.Right)
            return Side.Left;
        else
            return Side.Top;

    }
}

public enum Side
{
    Top = 0,
    Down = 1,
    Left = 2,
    Right = 3
};