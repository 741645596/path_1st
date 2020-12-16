using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 扇区Level信息结构
/// </summary>
[System.Serializable]
public struct SectorLevelInfo
{
    /// <summary>
    /// 单个扇区包含的Tile的数量 sectorWidth * sectorHeight
    /// </summary>
    public ushort sectorWidth;
    /// <summary>
    /// 单个扇区包含的Tile的数量 sectorWidth * sectorHeight
    /// </summary>
    public ushort sectorHeight;
    /// <summary>
    /// 这个level包含的扇区数量 numWidth * numHeight
    /// </summary>
    public ushort numWidth;
    /// <summary>
    /// 这个level包含的扇区数量 numWidth * numHeight
    /// </summary>
    public ushort numHeight;

    /// <summary>
    /// 设置扇区level信息
    /// </summary>
    /// <param name="sectorWidth"></param>
    /// <param name="sectorHeight"></param>
    /// <param name="numWidth"></param>
    /// <param name="numHeight"></param>
    public SectorLevelInfo(ushort sectorWidth, ushort sectorHeight, ushort numWidth, ushort numHeight)
    {
        this.sectorWidth = sectorWidth;
        this.sectorHeight = sectorHeight;
        this.numWidth = numWidth;
        this.numHeight = numHeight;
    }
    public SectorLevelInfo(SectorLevelInfo v)
    {
        this.sectorWidth = v.sectorWidth;
        this.sectorHeight = v.sectorHeight;
        this.numWidth = v.numWidth;
        this.numHeight = v.numHeight;
    }
    /// <summary>
    /// 判断是否相等
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static bool operator !=(SectorLevelInfo vector1, SectorLevelInfo vector2)
    {
        return vector1.sectorWidth != vector2.sectorWidth
            || vector1.sectorHeight != vector2.sectorHeight
            || vector1.numWidth != vector2.numWidth
            || vector1.numHeight != vector2.numHeight;
    }
    /// <summary>
    /// 判断是否相等
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static bool operator ==(SectorLevelInfo vector1, SectorLevelInfo vector2)
    {
        return vector1.sectorWidth == vector2.sectorWidth
            && vector1.sectorHeight == vector2.sectorHeight
            && vector1.numWidth == vector2.numWidth
            && vector1.numHeight == vector2.numHeight;
    }
    /// <summary>
    /// 判断是否相等
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(System.Object obj)
    {
        if (obj == null)
        {
            return false;
        }

        SectorLevelInfo p = (SectorLevelInfo)obj;
        if ((System.Object)p == null)
        {
            return false;
        }
        return (sectorWidth == p.sectorWidth)
            && (sectorHeight == p.sectorHeight)
            && (numWidth == p.numWidth)
            && (numHeight == p.numHeight);
    }


    /// <summary>
    /// 获取hash
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// 单个扇区包含的格子数量
    /// </summary>
    /// <returns></returns>
    public int NumTile()
    {
        return sectorWidth * sectorHeight;
    }
}