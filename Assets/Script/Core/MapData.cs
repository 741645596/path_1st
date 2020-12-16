using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapData : ScriptableObject
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
    /// 扇区描述数组，数组0:Level， 数组1: 扇区索引
    /// </summary>
    public List<MapSectorWriteList> sectorArray = new List<MapSectorWriteList>();

    public void Init(Map m)
    {
        this.mapStartPos = m.mapStartPos;
        this.TileSize = m.TileSize;
        this.SectorSize = m.SectorSize;
        this.SectorlevelScale = m.SectorlevelScale;
        this.TileXnum = m.TileXnum;
        this.TileYnum = m.TileYnum;
        this.SectorLevelNum = m.SectorLevelNum;
        foreach (MapSector[] msarr in m.sectorArray)
        {
            sectorArray.Add(new MapSectorWriteList(msarr));
        }
    }
}

[Serializable]
public class MapSectorWriteList
{
    public List<MapSectorWrite> listms = new List<MapSectorWrite>();

    public MapSectorWriteList()
    { }

    public MapSectorWriteList(MapSector[] msArray)
    {
        foreach (MapSector ms in msArray)
        {
            listms.Add(ms.GetOutData());
        }
    }
}

[Serializable]
public class MapSectorWrite
{
    // 自己的位置
    public List<PathNodeWrite> Left = new List<PathNodeWrite>();
    public List<PathNodeWrite> top = new List<PathNodeWrite>();
    public List<PathNodeWrite> right = new List<PathNodeWrite>();
    public List<PathNodeWrite> down = new List<PathNodeWrite>();

    public MapSectorWrite()
    { }

    public LinkInfo GetDistance(IntVector2 pos1, IntVector2 pos2)
    {
        foreach (PathNodeWrite pn in Left)
        {
            if (pn.pos == pos1)
            {
                foreach (LinkWrite lw in pn.link)
                {
                    if (lw.pos == pos2)
                    {
                        return lw.linkinfo;
                    }
                }
            }
        }

        foreach (PathNodeWrite pn in top)
        {
            if (pn.pos == pos1)
            {
                foreach (LinkWrite lw in pn.link)
                {
                    if (lw.pos == pos2)
                    {
                        return lw.linkinfo;
                    }
                }
            }
        }


        foreach (PathNodeWrite pn in right)
        {
            if (pn.pos == pos1)
            {
                foreach (LinkWrite lw in pn.link)
                {
                    if (lw.pos == pos2)
                    {
                        return lw.linkinfo;
                    }
                }
            }
        }


        foreach (PathNodeWrite pn in down)
        {
            if (pn.pos == pos1)
            {
                foreach (LinkWrite lw in pn.link)
                {
                    if (lw.pos == pos2)
                    {
                        return lw.linkinfo;
                    }
                }
            }
        }

        return new LinkInfo(0);
    }
}

[Serializable]
public class PathNodeWrite
{
    // 自己的位置
    public IntVector2 pos;
    // 连接桥的位置
    public IntVector2 bridgegepos;
    //
    public List<LinkWrite> link = new List<LinkWrite>();
    public PathNodeWrite()
    { }
}

[Serializable]
public class LinkWrite
{
    // 自己的位置
    public IntVector2 pos;
    // 距离
    public LinkInfo linkinfo;
    //
    public LinkWrite()
    { }

    public LinkWrite(IntVector2 p, LinkInfo v)
    {
        this.pos = p;
        this.linkinfo = v;
    }
}