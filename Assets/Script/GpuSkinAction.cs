using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GpuSkinAction : IAnimation {

	private MeshRenderer render;
    public Color[] ColorArray = new Color[10];
     // Use this for initialization
     void Start () {
		render = gameObject.GetComponent<MeshRenderer>();
    }


	public override void DoAnim(int Frame)
	{
		render.material.SetFloat("_frame", Frame);
	}


    public void SetColorIndex(int ColorIndex)
    {
        if (ColorIndex >= 0 && ColorIndex < ColorArray.Length)
        {
            if (render != null)
            {
                render.sharedMaterial.SetColor("_GroupColor", ColorArray[ColorIndex]);
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().material.SetColor("_GroupColor", ColorArray[ColorIndex]);
            }
        }
    }
}