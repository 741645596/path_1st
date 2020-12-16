using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Reflection;
public class Test : MonoBehaviour
{
    public IntVector2 ms = new IntVector2(1,1);
    public IntVector2 es = new IntVector2(1199,1198);

    public List<string> lWriteText = new List<string>();
    private int m_isRun = 0;
    // Start is called before the first frame update
    private GUIStyle m_style;

    void Start()
    {
        m_style = new GUIStyle();
        m_style.normal.background = null;
        m_style.normal.textColor = Color.green;
        m_style.fontSize = 25;
        WarmUpCode();
    }


    void OnGUI()
    {
        GUI.Label(new Rect(300, 10, 200, 40), "终点[" + es.x + "," + es.y + "]", m_style);
        if (GUI.Button(new Rect(10, 10, 100, 40), "随机寻路终点X"))
        {
            int x = UnityEngine.Random.Range(0, 1199);
            es.x = (short)x;
        }

        if (GUI.Button(new Rect(130, 10, 100, 40), "随机寻路终点Y"))
        {
            int y = UnityEngine.Random.Range(0, 1199);
            es.y = (short)y;
        }


        if (m_isRun == 0)
        {
            GUI.Label(new Rect(Screen.width / 2, 10, 300, 60), "开始测试");
            if (GUI.Button(new Rect(10, 100, 300, 60), "批量测试寻路"))
            {
                startTest();
            }
        }
        else if (m_isRun == 1)
        {
            GUI.Label(new Rect(Screen.width / 2, 10, 300, 60), "批量测试运行中");
        }
        else if (m_isRun == 2)
        {
            GUI.Label(new Rect(Screen.width / 2, 10, 300, 60), "寻路完成,测试数据文件在datapath");
        }


        if (GUI.Button(new Rect(10, 200, 300, 60), "批量添加动态挡格测试"))
        {
            AddBlockTest();
        }

        if (GUI.Button(new Rect(10, 300, 300, 60), "批量移除动态挡格测试"))
        {
            UnBlockTest();
        }
    }

    public void WriteFile()
    {
        foreach (string str in lWriteText)
        {
            File.AppendAllText(Application.dataPath +"/寻路性能测试.txt", str + "\n", Encoding.UTF8);
        }
        lWriteText.Clear();
        m_isRun = 0;
    }


    public void startTest()
    {
        lWriteText.Add("开始批量寻路测试");
        int ss = UnityEngine.Random.Range(0, 99);
        m_isRun = 1;
        for (int i = 0; i < 22; i++)
        {
            MapTile s = PathFind.instance.m_map.GetMapTileSafe(new IntVector2(ss + 50 * i, ss + 50 * i));
            MapTile e = PathFind.instance.m_map.GetMapTileSafe(es);

            for (int jj = 0; jj < 20; jj++)
            {
                float timeElapsed = PathFind.instance.TestRun(s, e);
                string str = "起点[" + s.Pos.x + "," + s.Pos.y + "]" + "终点[" + e.Pos.x + "," + e.Pos.y + "]寻路耗时:" + timeElapsed.ToString() + "ms";
                lWriteText.Add(str);
            }

        }
        m_isRun = 2;
        StartCoroutine(waitwrite());
    }

    public void AddBlockTest()
    {
        lWriteText.Add("开始添加动态挡格测试");
        m_isRun = 1;
        for (int i = 3; i < 1197; i+=50)
        {
            for (int jj = 3; jj < 1197; jj+=50)
            {
                MapTile s = PathFind.instance.m_map.GetMapTileSafe(new IntVector2(i, jj));
                float timeElapsed = PathFind.instance.TestAddBlock(s);
                string str = "添加以[" + s.Pos.x + "," + s.Pos.y + "]" + "为中心5*5范围的动态挡格耗时:" + timeElapsed.ToString() + "ms";
                lWriteText.Add(str);
            }

        }
        m_isRun = 2;
        StartCoroutine(waitwrite());
        /*MapTile s = PathFind.instance.m_map.GetMapTileSafe(new IntVector2(3, 3));
        float timeElapsed = PathFind.instance.TestAddBlock(s);
        string str = "添加以[" + s.Pos.x + "," + s.Pos.y + "]" + "为中心5*5范围的动态挡格耗时:" + timeElapsed.ToString() + "ms";
        Debug.Log(str);*/

    }

    public void UnBlockTest()
    {
        lWriteText.Add("开始移除动态挡格测试");
        m_isRun = 1;
        for (int i = 3; i < 1197; i += 50)
        {
            for (int jj = 3; jj < 1197; jj += 50)
            {
                MapTile s = PathFind.instance.m_map.GetMapTileSafe(new IntVector2(i, jj));
                float timeElapsed = PathFind.instance.TestRemoveBlock(s);
                string str = "移除以[" + s.Pos.x + "," + s.Pos.y + "]" + "为中心5*5范围的动态挡格耗时:" + timeElapsed.ToString() + "ms";
                lWriteText.Add(str);
            }

        }
        m_isRun = 2;
        StartCoroutine(waitwrite());
    }


    private IEnumerator waitwrite()
    {
        yield return new WaitForSeconds(0.1f);
        WriteFile();
    }

    private void WarmUpCode()
    {
        Assembly ass = Assembly.GetExecutingAssembly();

        Type type = ass.GetType("MapChangeManger");
        DoType(type);
        //
        type = ass.GetType("JumpFlowFiled");
        DoType(type);

        type = ass.GetType("FlowField");
        DoType(type);
        type = ass.GetType("JumpFlowFiled");
        DoType(type);
        type = ass.GetType("AStar");
        DoType(type);
        type = ass.GetType("IggPathFinder");
        DoType(type);
        type = ass.GetType("PathNode");
        DoType(type);
        type = ass.GetType("JumpPathNode");
        DoType(type);
        type = ass.GetType("PathFind");
        DoType(type);
        //
        type = ass.GetType("MapSector");
        DoType(type);
        type = ass.GetType("LowSector");
        DoType(type);
        type = ass.GetType("HighSector");
        DoType(type);
        type = ass.GetType("SectorHelp");
        DoType(type);
        //
        type = ass.GetType("TileHelp");
        DoType(type);
        type = ass.GetType("MapTile");
        DoType(type);
        //
        type = ass.GetType("Map");
        DoType(type);
        //
        type = ass.GetType("IntVector2");
        DoType(type);
        type = ass.GetType("Util");
        DoType(type);
    }

    private void DoType(Type type)
    {
        MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly |
                                BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.Instance |
                                BindingFlags.Static);
        for (int j = 0, jmax = methods.Length; j < jmax; ++j)
        {
            MethodInfo method = methods[j];
            if (!method.IsGenericMethod && !method.IsAbstract && !method.IsConstructor)
            {
                if (method != null && method.MethodHandle != null)
                {
                    method.MethodHandle.GetFunctionPointer();
                }
            }
        }
    }
}
