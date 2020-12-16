using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.Profiling;

public class PathFind : MonoBehaviour
{
    public MapData data;
    public static PathFind instance = null;
    public LayerMask obstacleLayer;
    public LayerMask groundLayer;
    public  Map m_map;
    public List<PathRun> m_ListPersion = new List<PathRun>();


    public void Awake()
    {
        instance = this;
        // 生成地图。
        m_map = new Map();
        m_map.Init_RumTimeMode(data);

        Vector3 scale = new Vector3(m_map.TileXnum * 0.1f, 1, m_map.TileYnum * 0.1f);
        transform.localScale = scale;
    }

    public float TestRun(MapTile start, MapTile dest)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        IggPathFinder.FindPaths(start, dest);
        sw.Stop();
        return sw.ElapsedTicks / 10000.0f; 

    }

    /// <summary>
    /// 测试动态挡格,以tile为中心5 * 5的添加动态挡格
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public float TestAddBlock(MapTile tile)
    {
        List<MapTile> l = new List<MapTile>();
        if (TileHelp.GetAreaTile(tile.Pos, 2, false, ref l) == true)
        {
            MapChangeManger.BlockTile(l);
        }
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Profiler.BeginSample("TestMethod");
        MapChangeManger.InputChanges();
        Profiler.EndSample();
        sw.Stop();
        return sw.ElapsedTicks / 10000.0f;

    }


    /// <summary>
    /// 测试动态挡格,以tile为中心5 * 5的添加动态挡格
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public float TestRemoveBlock(MapTile tile)
    {
        List<MapTile> l = new List<MapTile>();
        if (TileHelp.GetAreaTile(tile.Pos, 2, false, ref l) == true)
        {
            MapChangeManger.UnBlockTile(l);
        }
        Stopwatch sw = new Stopwatch();
        sw.Start();
        MapChangeManger.InputChanges();
        sw.Stop();
        return sw.ElapsedTicks /10000.0f;

    }



    public void FindPaths(MapTile dest, List<PathRun> units)
    {
        if (dest == null || units == null || units.Count == 0)
            return;
        foreach (PathRun unit in units)
        {
            FindPath(dest, unit);
        }
    }

    private void FindPath(MapTile dest, PathRun units)
    {
        if (dest != null && !dest.blocked &&
            units.currentTile != null && !units.currentTile.blocked
            && dest != units.currentTile)
        {
            RoadResult ret = IggPathFinder.FindPaths(units.currentTile, dest);
            units.SetRoad(ret);
            if(m_ListPersion.Contains(units) == false)
            {
                m_ListPersion.Add(units);
            }
        }
    }

    public Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1500f, groundLayer))
            return hit.point;
        return Vector3.up;
    }
    /// <summary>
    /// 获取屏幕中心地图位置
    /// </summary>
    /// <returns></returns>
    public IntVector2 GetScreenMapCenter()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/ 2, 0, Screen.height/ 2));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1500f, groundLayer))
        {
            MapTile tile = PathFind.instance.m_map.GetMapTile(hit.point);
            if (tile != null)
            {
                return tile.Pos;
            }
        }
        return IntVector2.zero;
    }


    public void WorldHasBeenChanged(List<LowSector> changedSectors)
    {
        List<ushort> ls = new List<ushort>();
        foreach (LowSector v in changedSectors)
        {
            if (v != null && ls.Contains(v.ID) == false)
            {
                ls.Add(v.ID);
            }
        }

        foreach (PathRun p in m_ListPersion)
        {
            if(p != null  && p.CheckHaveSector(ls))
            {
                FindPath(p.dest, p);
            }
        }
    }

    void OnDrawGizmos()
    {
        // draw world bounding box
        Gizmos.color = Color.blue;

        if (m_map != null)
            m_map.DrawGizmos();
    }

}
