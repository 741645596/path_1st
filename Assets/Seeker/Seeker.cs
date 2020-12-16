using UnityEngine;
using System.Collections.Generic;

public class Seeker : MonoBehaviour
{
    public MapTile currentTile;
    public MapTile DestTile;

    float flowWeight = 0.9f;
    float sepWeight = 2.5f;
    float sepWeightIdle = 2.75f;
    float alignWeight = 0.3f;
    float cohWeight = 0f;

    float maxForce = 4; // maximun magnitude of the (combined) force vector that is applied each tick
    float maxMoveSpeed = 4; // maximun magnitude of the (combined) velocity vector
    float maxIdleSpeed = 0.8f; // maximun magnitude of the (combined) velocity vector

    public static float neighbourRadiusMoving = 1.2f;
    private float neighbourRadiusSquaredMoving = 0;
    public static float neighbourRadiusIdle = 0.5f;
    private float neighbourRadiusSquaredIdle = 0;

    public float neighbourRadius = 0;
    public float neighbourRadiusSquared = 0;
    

    [HideInInspector]
    public Vector3 movement = new Vector3();
    [HideInInspector]
    public Vector2 velocity = Vector2.zero;
    public Seeker[] neighbours;

    private Vector2[] combinedForces = new Vector2[4];


    private ActorMove controller;
    private ActorState seekerState = ActorState.Idle;
    public int group = 0;
    public Rect searchQuad = new Rect(0, 0, 0, 0);


    private Vector2 m_Pos;
    public Vector2 Pos
    {
        get { return m_Pos; }
        set { m_Pos = value; }
    }



    // Use this for initialization
    void Start()
    {
        Pos = new Vector2(transform.position.x - PathFind.instance.m_map.mapStartPos.x, transform.position.z - PathFind.instance.m_map.mapStartPos.z);
        neighbourRadiusSquaredMoving = neighbourRadiusMoving * neighbourRadiusMoving;
        neighbourRadiusSquaredIdle = neighbourRadiusIdle * neighbourRadiusIdle;
        SetNeighbourRadius(seekerState);
        neighbours = new Seeker[SeekerMovementManager.maxNeighbourCount];
        SeekerMovementManager.AddSeeker(this);

        controller = GetComponent<ActorMove>();
        if (controller != null)
        {
            controller.SetActorState(seekerState);
            controller.SetGroup(group);
        }
    }

    private void SetNeighbourRadius(ActorState currentState)
    {
        switch (seekerState)
        {
            case ActorState.Idle:
                {
                    neighbourRadius = neighbourRadiusIdle;
                    neighbourRadiusSquared = neighbourRadiusSquaredIdle;

                }
                break;
            case ActorState.Moving:
                {
                    neighbourRadius = neighbourRadiusMoving;
                    neighbourRadiusSquared = neighbourRadiusSquaredMoving;

                }
                break;
        }
    }

    // Update is called once per frame
    public void Tick()
    {
        switch (seekerState)
        {
            case ActorState.Idle:
                Idle();
                break;
            case ActorState.Moving:
                Move();
                break;
        }
        // 判断由没出界
        CheckIfMovementLegit();
        //movement
        controller.MoveAndRotate(movement, seekerState);
        Pos = new Vector2(transform.position.x - PathFind.instance.m_map.mapStartPos.x, transform.position.z - PathFind.instance.m_map.mapStartPos.z);
    }

    private void Idle()
    {
        if (neighbours[0] == null)
        {
            velocity = Vector2.zero;
            movement = Vector3.zero;
        }
        else
        {
            Vector2 netForce = Vector2.zero;

            // 4 steering Vectors in order: Flow, separation, alignment, cohesion
            // adjusted with user defined weights
            FlowFieldFollow();

            combinedForces[0] = sepWeightIdle * Separation(neighbourRadiusSquaredIdle); // seperation
            combinedForces[1] = Vector2.zero;
            combinedForces[2] = Vector2.zero;
            combinedForces[3] = Vector2.zero;

            // calculate the combined force, but dont go over the maximum force
            netForce = CombineForces(maxForce, combinedForces);
            // velocity gets adjusted by the calculated force
            velocity += netForce * Time.deltaTime;

            // dont go over the maximum movement speed possible
            if (velocity.magnitude > maxIdleSpeed)
                velocity = (velocity / velocity.magnitude) * maxIdleSpeed;

            // move
            movement = new Vector3(velocity.x * Time.deltaTime, 0, velocity.y * Time.deltaTime);
        }
    }


    private void Move()
    {
        if (currentTile == DestTile)
            ReachedDestination();
        else
        {
            // 4 steering Vectors in order: Flow, separation, alignment, cohesion
            // adjusted with user defined weights
            combinedForces[0] = flowWeight * FlowFieldFollow();
            combinedForces[1] = sepWeight * Separation(neighbourRadiusSquaredMoving);
            combinedForces[2] = alignWeight * Alignment();
            combinedForces[3] = cohWeight * Cohesion();
            // calculate the combined force, but dont go over the maximum force
            Vector2 netForce = CombineForces(maxForce, combinedForces);

            // velocity gets adjusted by the calculated force
            velocity += netForce * Time.deltaTime;
            // dont go over the maximum movement speed possible
            if (velocity.magnitude > maxMoveSpeed)
                velocity = (velocity / velocity.magnitude) * maxMoveSpeed;

            // move
            movement = new Vector3(velocity.x * Time.deltaTime, 0, velocity.y * Time.deltaTime);
        }
    }
    /// <summary>
    /// 到达目标
    /// </summary>
    private void ReachedDestination()
    {
        velocity = Vector2.zero;
        movement = Vector3.zero;
        seekerState = ActorState.Idle;
        SetNeighbourRadius(seekerState);

        SetSearchBoxQuad();
        ClearNeighbours();
        SeekerMovementManager.UpdateNeighbour(5f, searchQuad, this);


        for (int i = 0; i < neighbours.Length; i ++ )
        {
            if (neighbours[i] == null)
                break;

            neighbours[i].seekerState = ActorState.Idle;
            neighbours[i].SetNeighbourRadius(neighbours[i].seekerState);
            neighbours[i].velocity = Vector2.zero;
            neighbours[i].movement = Vector3.zero;
        }
    }

    /// <summary>
    /// 合力
    /// </summary>
    /// <param name="maxForce"></param>
    /// <param name="forces"></param>
    /// <returns></returns>
    Vector2 CombineForces(float maxForce, Vector2[] forces)
    {
        Vector2 force = Vector2.zero;

        for (int i = 0; i < forces.Length; i++)
        {
            Vector2 newForce = force + forces[i];

            if (newForce.magnitude > maxForce)
            {
                float amountNeeded = maxForce - force.magnitude;
                float amountAdded = forces[i].magnitude;
                float division = amountNeeded / amountAdded;

                force += division * forces[i];

                return force;
            }
            else
                force = newForce;
        }
        return force;
    }


    private Vector2 FlowFieldFollow()
    {
        return Vector2.zero;
    }


    /// <summary>
    /// 计算排斥力
    /// </summary>
    /// <param name="squaredRadius"></param>
    /// <returns></returns>
    private Vector2 Separation(float squaredRadius)
    {
        if (neighbours[0] == null)
            return Vector2.zero;

        Vector2 totalForce = Vector2.zero;

        int neighbourAmount = 0;
        // get avarge push force away from neighbours
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == null)
                break;

            Vector2 pushforce = this.Pos - neighbours[i].Pos;
            totalForce += pushforce.normalized * Mathf.Max(0.05f,(squaredRadius - pushforce.magnitude));
            neighbourAmount++;
        }

        totalForce /= neighbourAmount;//neighbours.Count;
        totalForce *= maxForce;

        return totalForce;
    }

    /// <summary>
    /// 凝聚力
    /// </summary>
    /// <returns></returns>
    private Vector2 Cohesion()
    {
        if (neighbours[0] == null)
            return Vector2.zero;

        Vector2 centerOfMass = Pos;

        int neighbourAmount = 0;
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == null)
                break;   
            centerOfMass += neighbours[i].Pos;
            neighbourAmount++;
        }
        centerOfMass /= neighbourAmount;

        Vector2 desired = centerOfMass - Pos;
        desired *= (maxMoveSpeed / desired.magnitude);

        Vector2 force = desired - velocity;
        return force * (maxForce / maxMoveSpeed);
    }


    private Vector2 Alignment()
    {
        if (neighbours[0] == null)
            return Vector2.zero;

        // get avarge velocity from neighbours
        Vector2 averageHeading = velocity.normalized;

        int neighbourAmount = 0;
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == null)
                break;
                
            averageHeading += neighbours[i].velocity.normalized;
            neighbourAmount++;
        }

        averageHeading /= neighbourAmount;

        Vector2 desired = averageHeading * maxMoveSpeed;

        Vector2 force = desired - velocity;
        return force * (maxForce / maxMoveSpeed);
    }

    public void SetSearchBoxQuad()
    {
        searchQuad.xMin = Pos.x - neighbourRadius;
        searchQuad.yMin = Pos.y - neighbourRadius;
        searchQuad.width = searchQuad.height = neighbourRadius * 2f;
    }

    public void AddNeighbours(int index,Seeker foundNeighbours)
    {
        neighbours[index] = foundNeighbours;
    }
    /// <summary>
    /// 清理邻居
    /// </summary>
    public void ClearNeighbours()
    {
        for (int i = 0; i < neighbours.Length; i++)
            neighbours[i] = null;
    }
    /// <summary>
    /// 判断移动是否合法
    /// </summary>
    /// <param name="seeker"></param>
    public void CheckIfMovementLegit()
    {
        // 先不考虑
    }

}

