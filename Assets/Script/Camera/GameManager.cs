using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private Dictionary<IntVector2, GameObject> obstacles = new Dictionary<IntVector2, GameObject>();
    private string[] selStrings = new string[] { "新增小兵", "寻路", "添加障碍物", "移除障碍物","移动地图" };
    private string[] selPathStrings = new string[] { "1组", "2组", "3组", "4组", "5组", "6组", "7组", "8组", "9组", "10组" };
    /// <summary>
    /// 控制多组小兵运动
    /// </summary>
    private Dictionary<int, List<PathRun>> m_DicUnits = new Dictionary<int, List<PathRun>>();
    private int m_UnitKey = 0;
    public GameObject unitGo;
    public GameObject unitblock;
    public Transform tnode;
    private int m_SelUnitKey = 0;
    public int m_operation;


    private GUIStyle m_style;


    void Start()
    {
        m_operation = (int)operation.AddUnit;
        m_style = new GUIStyle();
        m_style.normal.background = null;
        m_style.normal.textColor = Color.green;
        m_style.fontSize = 25;
    }

    void Update()
    {
        Inputs();
    }


    private void Inputs()
    {
        Vector3 s = PathFind.instance.GetMousePosition();
        if (s == Vector3.up)
            return;
        MapTile tile = PathFind.instance.m_map.GetMapTile(s);
        if (tile != null)
        {
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.y < Screen.height - 200)
            {
                if (m_operation == (int)operation.AddUnit)
                {
                    AddUnit(tile);
                }
                else if (m_operation == (int)operation.SearchPath)
                {
                    if (m_DicUnits.ContainsKey(m_SelUnitKey) == true)
                    {
                        PathFind.instance.FindPaths(tile, m_DicUnits[m_SelUnitKey]);
                    }
                }
                else if (m_operation == (int)operation.AddBlock)
                {
                    if (CheckAddObstacles(tile) == true)
                    {
                        //m_operation = (int)operation.MoveMap;
                        MapChangeManger.InputChanges();
                    }
                }
                else if (m_operation == (int)operation.RemoveBlock)
                {
                    if (CheckRemoveObstacles(tile) == true)
                    {
                        //m_operation = (int)operation.MoveMap;
                        MapChangeManger.InputChanges();
                    }
                }

            }
        }
    }
    /// <summary>
    /// 检查添加障碍物
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool CheckAddObstacles(MapTile tile)
    {
        if (tile == null || tile.blocked == true)
            return false;
        List<MapTile> l = new List<MapTile>();
        if (TileHelp.GetAreaTile(tile.Pos, 2, false, ref l) == true)
        {
            GameObject b = Instantiate(unitblock, PathFind.instance.m_map.GetMapTileWorldPos(tile), Quaternion.identity) as GameObject;
            b.transform.parent = transform;
            obstacles.Add(tile.Pos, b);
            MapChangeManger.BlockTile(l);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 检查移除障碍物
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool CheckRemoveObstacles(MapTile tile)
    {
        // block 为5*5建筑
        if (tile == null || tile.blocked == false)
            return false;
        IntVector2 pos = tile.Pos;
        foreach (IntVector2 p in obstacles.Keys)
        {
            if (Mathf.Abs(pos.x - p.x) <= 2 && Mathf.Abs(pos.y - p.y) <= 2)
            {
                List<MapTile> l = new List<MapTile>();
                if (TileHelp.GetAreaTile(p, 2, true, ref l) == true)
                {
                    Destroy(obstacles[p]);
                    obstacles.Remove(p);
                    MapChangeManger.UnBlockTile(l);
                    return true;
                }
            }
        }
        return false;
    }

    void OnGUI()
    {            
        GUI.Label(new Rect(Screen.width - 250, 10, 250, 35), "地图size：" + PathFind.instance.m_map.TileXnum * PathFind.instance.m_map.TileYnum, m_style);
        m_operation = GUI.SelectionGrid(new Rect(0, 10, 500, 30), m_operation, selStrings, selStrings.Length);
        if (m_UnitKey > 0)
        {
            GUI.Label(new Rect(0, 40, 250, 35), "请选择寻路小兵:", m_style);
            GUI.Label(new Rect(250, 40, 250, 35), "小兵组数:" + m_UnitKey, m_style);
            m_SelUnitKey = GUI.SelectionGrid(new Rect(0, 80, 800, 30), m_SelUnitKey, selPathStrings, selPathStrings.Length);
        }
        IntVector2 pos = PathFind.instance.GetScreenMapCenter();
        GUI.Label(new Rect(Screen.width -200, Screen.height -40, 200, 35), "位置：" + pos.x + "," + pos.y, m_style);
    }


    private void AddUnit(MapTile t)
    {
        if (t != null && m_UnitKey < 10)
        {
            List<PathRun> list = new List<PathRun>();
            foreach (MapTile tile in TileHelp.GetAllNeighbours(t))
            {
                GameObject g = GameObject.Instantiate(unitGo);
                g.transform.parent = tnode;
                Vector3 v = PathFind.instance.m_map.GetMapTileWorldPos(tile);
                g.transform.position = v;
                PathRun s = g.GetComponent<PathRun>();
                s.group = m_UnitKey;
                s.currentTile = tile;
                list.Add(s);
            }
            m_DicUnits.Add(m_UnitKey++, list);
        }


    }


}

/// <summary>
/// 操作模式
/// </summary>
public enum operation {
    AddUnit     = 0,
    SearchPath  = 1,
    AddBlock    = 2,
    RemoveBlock = 3,
    MoveMap     = 4
}