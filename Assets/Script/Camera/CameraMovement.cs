using UnityEngine;
/// <summary>
/// 相机移动
/// </summary>
public class CameraMovement : MonoBehaviour
{
    public Camera cam;
    public float moveSpeed = 0.5f;
	public float zoom = 3;

    public GameManager gm;

    Vector2 p1, p2;//用来记录鼠标的位置，以便计算移动距离

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            cam.orthographicSize -= zoom;
            cam.transform.position += cam.transform.forward;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            cam.orthographicSize += zoom;
            cam.transform.position -= cam.transform.forward;
        }

        if (gm != null && gm.m_operation == (int)operation.MoveMap)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //鼠标左键按下时记录鼠标位置p1 
                p1 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }

            if (Input.GetMouseButton(0))
            {
                //鼠标左键拖动时记录鼠标位置p2   
                Vector3 newVec = Quaternion.AngleAxis(-45.0f, Vector3.right) * Vector3.forward;
                p2 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                float dx = p2.x - p1.x;
                float dy = p2.y - p1.y;
                //鼠标左右移动  
                transform.Translate(-dx * Vector3.right * Time.deltaTime * moveSpeed);
                transform.Translate(-dy * newVec * Time.deltaTime * moveSpeed);
            }
        }

    }

}
