using System;

[Serializable]
public struct IntVector2
{
    public short x, y;

    public IntVector2(int x, int y)
    {
        this.x = (short)x;
        this.y = (short)y;
    }

    private static readonly IntVector2 mInvalid = new IntVector2(-1, -1);
    /// <summary>
    /// 特殊的标识，无效位置标识
    /// </summary>
    public static IntVector2 invalid
    {
        get { return mInvalid; }
    }


    private static readonly IntVector2 mZero = new IntVector2(0, 0);
    public static IntVector2 zero
    {
        get {return mZero;}
    }

    private static readonly IntVector2 mOne = new IntVector2(1, 1);
    public static IntVector2 one
    {
        get { return mOne; }
    }

    private static readonly IntVector2 mTop = new IntVector2(0, -1);
    public static IntVector2 top
    {
        get { return mTop; }
    }

    private static readonly IntVector2 mDown = new IntVector2(0, 1);
    public static IntVector2 down
    {
        get { return mDown; }
    }

    private static readonly IntVector2 mLeft = new IntVector2(-1, 0);
    public static IntVector2 left
    {
        get { return mLeft; }
    }

    private static readonly IntVector2 mRight = new IntVector2(1, 0);
    public static IntVector2 right
    {
        get { return mRight; }
    }


    public static IntVector2 rightTop
    {
        get { return right + top; }
    }

    public static IntVector2 rightDown
    {
        get { return right + down; }
    }

    public static IntVector2 leftTop
    {
        get { return left + top; }
    }

    public static IntVector2 leftDown
    {
        get { return left + down; }
    }


    public static bool operator !=(IntVector2 vector1, IntVector2 vector2)
    {
        return vector1.x != vector2.x || vector1.y != vector2.y;
    }

    public static bool operator ==(IntVector2 vector1, IntVector2 vector2)
    {
        return vector1.x == vector2.x && vector1.y == vector2.y;
    }

    public static IntVector2 operator +(IntVector2 vector1, IntVector2 vector2)
    {
        int xx = vector1.x + vector2.x;
        int yy = vector1.y + vector2.y;
        return new IntVector2((short)xx, (short)yy);
    }

    public static IntVector2 operator -(IntVector2 vector1, IntVector2 vector2)
    {
        int xx = vector1.x - vector2.x;
        int yy = vector1.y - vector2.y;
        return new IntVector2((short)xx, (short)yy);
    }

    public static IntVector2 operator *(short k, IntVector2 vector)
    {
        int xx = vector.x * k;
        int yy = vector.y * k;
        return new IntVector2((short)xx, (short)yy);
    }

    public static IntVector2 operator *(IntVector2 vector, int k)
    {
        int xx = vector.x * k;
        int yy = vector.y * k;
        return new IntVector2((short)xx, (short)yy);
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null)
        {
            return false;
        }

        IntVector2 p = (IntVector2)obj;
        if ((System.Object)p == null)
        {
            return false;
        }
        return (x == p.x) && (y == p.y);
    }

    /// <summary>
    /// 获取向量的方向
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static IntVector2 GetDir(IntVector2 start, IntVector2 end)
    {
        IntVector2 def = end - start;
        if (def.x > 0)
        {
            def.x = 1;
        }
        else if (def.x < 0)
        {
            def.x = -1;
        }

        if (def.y > 0)
        {
            def.y = 1;
        }
        else if (def.y < 0)
        {
            def.y = -1;
        }
        return def;
    }


    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// left 邻居
    /// </summary>
    public IntVector2 Left
    {
        get { return this + IntVector2.left; }
    }
    /// <summary>
    /// 右邻居
    /// </summary>
    public IntVector2 Right
    {
        get { return this + IntVector2.right; }
    }

    /// <summary>
    /// 上邻居
    /// </summary>
    public IntVector2 Top
    {
        get { return this + IntVector2.top; }
    }

    /// <summary>
    /// 下邻居
    /// </summary>
    public IntVector2 Down
    {
        get { return this + IntVector2.down; }
    }

    /// <summary>
    /// 左上邻居
    /// </summary>
    public IntVector2 LeftTop
    {
        get { return this + IntVector2.top + IntVector2.left; }
    }

    /// <summary>
    /// 左下邻居
    /// </summary>
    public IntVector2 LeftDown
    {
        get { return this + IntVector2.down + IntVector2.left; }
    }
    /// <summary>
    /// 右上邻居
    /// </summary>
    public IntVector2 RightTop
    {
        get { return this + IntVector2.top + IntVector2.right; }
    }

    /// <summary>
    /// 右下邻居
    /// </summary>
    public IntVector2 RightDown
    {
        get { return this + IntVector2.down + IntVector2.right; }
    }
}