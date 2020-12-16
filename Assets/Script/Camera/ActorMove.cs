using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMove : MonoBehaviour
{
    public Transform tActor = null;
    public GpuSkinAction Ani = null;
    /// <summary>
    /// 运动并朝向运行的方向
    /// </summary>
    /// <param name="movement"></param>
    public void MoveAndRotate(Vector3 movement, ActorState state)
    {
        movement.y = 0;
        if (tActor != null)
        {
            tActor.position = tActor.position + movement;
            // rotation
            tActor.LookAt(tActor.position + movement);
        }
        SetActorState(state);

    }

    public void SetActorState(ActorState state)
    {
        if (Ani != null)
        {
            if (state == ActorState.Idle)
            {
                Ani.Play("wait1");
            }
            else if (state == ActorState.Moving)
            {
                Ani.Play("run");
            }
        }
    }

    public void SetGroup(int group)
    {
        if (Ani != null)
        {
            Ani.SetColorIndex(group);
        }
    }
}


public enum ActorState
{
    Idle,
    Moving
}