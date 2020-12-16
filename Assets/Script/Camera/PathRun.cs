using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRun : MonoBehaviour
{
    private Mesh Linemesh = null;
    public Material material;
    public float speed = 1;  //用于控制移动速度
    float des;             //用于存储与目标点的距离     
                           // Use this for initialization
    public Vector3 movement = new Vector3();
    private List<Vector3> listPath = new List<Vector3>();
    private List<ushort> lSector = new List<ushort>();
    public MapTile dest;
    private int CurPathNode = 0;

    private ActorMove controller;
    private ActorState seekerState = ActorState.Idle;
    public int group = 0;
    public MapTile currentTile;

    void Start()
    {
        controller = GetComponent<ActorMove>();
        if (controller != null)
        {
            controller.SetActorState(seekerState);
            controller.SetGroup(group);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Linemesh != null)
        {
            Graphics.DrawMesh(Linemesh, Vector3.zero, Quaternion.identity, material, 0);
        }
        if (listPath == null || listPath.Count == 0 || CurPathNode >= listPath.Count)
        {
            seekerState = ActorState.Idle;
            controller.SetActorState(seekerState);
            ClearCurve();
        }
        else
        {
            //计算与目标点间的距离
            des = Vector3.Distance(this.transform.position, listPath[CurPathNode]);

            movement = (listPath[CurPathNode] - this.transform.position).normalized * speed * Time.deltaTime;
            //移向目标
            //movement
            controller.MoveAndRotate(movement, seekerState);


            currentTile = PathFind.instance.m_map.GetMapTile(this.transform.position);

            //如果移动到当前目标点，就移动向下个目标
            if (des < 0.1f && CurPathNode < listPath.Count)
            {
                CurPathNode++;
            }
            seekerState = ActorState.Moving;
            controller.MoveAndRotate(movement, seekerState);
        }
    }

    public void SetRoad(RoadResult result)
    {
        ClearCurve();
        dest = result.dest;
        lSector.Clear();
        lSector.AddRange(result.lSector);
        listPath.Clear();
        CurPathNode = 0;
        foreach (IntVector2 p in result.lRoad)
        {
            listPath.Add(PathFind.instance.m_map.GetMapTileWorldPos(PathFind.instance.m_map.GetMapTileSafe(p)));
        }
        Linemesh = CreateLine(listPath, 0.3f, true);
    }


    private Mesh CreateLine(List<Vector3> lv, float Width, bool IsPull = false)
    {
        if (lv == null || lv.Count < 2)
            return null;
        Mesh mesh = new Mesh();

        int vlen = lv.Count;
        Vector3[] vertices = new Vector3[vlen * 2];
        Vector2[] uvs = new Vector2[vlen * 2];

        float dis = 0;

        for (int i = 0; i < vlen; i++)
        {
            if (i == 0)
            {
                dis = 0;
                vertices[i] = CalcLinePoint(lv[0], lv[1] - lv[0], -Width, IsPull);
                uvs[i] = new Vector2(0.25f, dis);
                vertices[i + vlen] = CalcLinePoint(lv[0], lv[1] - lv[0], Width, IsPull);
                uvs[i + vlen] = new Vector2(0.75f, dis);
            }
            else
            {
                dis += Vector3.Distance(lv[i], lv[i - 1]) / (2 * Width);
                vertices[i] = CalcLinePoint(lv[i], lv[i] - lv[i - 1], -Width, IsPull);
                uvs[i] = new Vector2(0.25f, dis);
                vertices[i + vlen] = CalcLinePoint(lv[i], lv[i] - lv[i - 1], Width, IsPull);
                uvs[i + vlen] = new Vector2(0.75f, dis);
            }
        }


        int[] triangles = new int[(vlen - 1) * 6];
        for (int i = 0, vi = 0; vi < vlen - 1; i += 6, vi++)
        {
            triangles[i] = vi;
            triangles[i + 1] = vi + vlen;
            triangles[i + 2] = vi + 1;

            triangles[i + 3] = vi + vlen;
            triangles[i + 4] = vi + 1 + vlen;
            triangles[i + 5] = vi + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }
    /// <summary>
    /// 计算线上的另外一个点
    /// </summary>
    /// <param name="start"></param>
    /// <param name="dir"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private Vector3 CalcLinePoint(Vector3 start, Vector3 dir, float width, bool IsPull)
    {
        Vector3 normal = Vector3.Cross(dir, new Vector3(0, 1, 0)).normalized;
        if (IsPull == false)
        {
            return start + normal * width;
        }
        else
        {
            return start + normal * width + new Vector3(0, 0.1f, 0);
        }

    }

    /// <summary>
    /// 清理曲线
    /// </summary>
    private bool ClearCurve()
    {
        Linemesh = null;
        return true;
    }


    /// <summary>
    /// 确定是否包含指定的道路
    /// </summary>
    /// <param name="ls"></param>
    /// <returns></returns>
    public bool CheckHaveSector(List<ushort> ls)
    {
        if (ls == null || ls.Count == 0)
            return false;
        foreach (ushort sector in ls)
        {
            if (lSector.Contains(sector) == true)
            {
                return true;
            }
        }
        return false;
    }
}
