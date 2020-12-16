using UnityEngine;
using System;

[Serializable]
public class MapTile
{
    /// <summary>
    /// 扇区索引，第0层的扇区索引
    /// </summary>
    public ushort sectorIndex = 0;
    /// <summary>
    /// 是否阻挡
    /// </summary>
    public bool blocked = false;
    /// <summary>
    /// 流场算法，
    /// </summary>
    public ushort integrationValue = 0;
    /// <summary>
    /// 瓦片坐标
    /// </summary>
    public IntVector2 Pos = new IntVector2(-1, -1);

    // 是为又PathNode跟这个tile 关联
    /// <summary>
    /// tile 是否这个tile创建为一个way point 点。
    /// </summary>
    public bool hasPathNodeConnection = false;
    /// <summary>
    /// 联通邻居属性，bit 0 表示通，1 表示不通
    /// </summary>
    public byte linkNeighbourValue = 0xff; 

    public void SetSectorIndex(ushort tileXnum, ushort tileYnum, ushort sectorSize)
    {
        ushort x = (ushort)(Pos.x);
        ushort y = (ushort)(Pos.y);
        int t = GetNum(tileXnum, sectorSize) * GetNum(y, sectorSize) + GetNum(x, sectorSize);
        sectorIndex = (ushort)t;
    }

    /// <summary>
    /// 获取长度
    /// </summary>
    /// <param name="w"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public int GetNum(ushort w, ushort size)
    {
        return (w / size);  
    }


    /// <summary>
    /// 获取瓦片所在的扇区
    /// </summary>
    /// <param name="level">扇区层次</param>
    /// <param name="map">地图</param>
    /// <returns></returns>
    public MapSector GetMapSector(int level,Map map)
    {
        if (map == null)
            return null;
        int x = Mathf.FloorToInt(Pos.x / (float)map.levelDimensions[level].sectorWidth);
        int y = Mathf.FloorToInt(Pos.y / (float)map.levelDimensions[level].sectorHeight);
        return map.FindMapSector(level, new IntVector2((short)x, (short)y));
    }


}
