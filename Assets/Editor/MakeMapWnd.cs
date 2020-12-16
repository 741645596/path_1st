using UnityEngine;
using UnityEditor;

public class MakeMapWnd : EditorWindow
{
    //MapData m_map = new MapData();

    [MenuItem("地图工具/制作寻路地图")]
    static void ShowMakeMapWnd()
    {
        EditorUtility.ClearProgressBar();
        MakeMapWnd wnd = EditorWindow.GetWindow<MakeMapWnd>("制作制作寻路地图工具");
        wnd.minSize = new Vector2(200, 250);
    }



    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 200, 100, 30), "制作寻路地图") == true)
        {
            MapData mm = ScriptableObject.CreateInstance<MapData>();
            Map m = new Map();
            m.Init_EditMode();
            mm.Init(m);
            AssetDatabase.CreateAsset(mm, "Assets/Data/BigMap.asset");
            EditorUtility.DisplayDialog("提示", "保存地图数据成功", "确认");
        }
    }
}