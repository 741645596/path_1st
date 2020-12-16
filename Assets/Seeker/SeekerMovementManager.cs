using UnityEngine;
using System.Collections.Generic;

public class SeekerMovementManager
{
    public static QuadTree quadTree;
    private static List<Seeker> allSeekers = new List<Seeker>();

    public static int maxAmountOfTrees = 1;
    public static List<QuadTree> openQuadTreeList;
    public static int maxNeighbourCount = 10;

    public static void Setup()
    {
        quadTree = new QuadTree(0, new Rect(0, 0, PathFind.instance.m_map.TileXnum, PathFind.instance.m_map.TileYnum));
        quadTree.Setup();

        int currentStep = 1;
        for (int i = 1; i < QuadTree.maxDepthLevel + 1; i++)
        {
            currentStep = currentStep * 4;
            maxAmountOfTrees += currentStep;
        }
        openQuadTreeList = new List<QuadTree>(maxAmountOfTrees);
    }

        // make seekers move and update their "neighbour" array  for proper steering forces
    public static void Update()
    {
        // empty tree and input all seekers
        quadTree.clear();
        for (int i = 0; i < allSeekers.Count; i++)
            quadTree.insert(allSeekers[i]);

        for (int i = 0; i < allSeekers.Count; i++)
        {
            // search box that exactly matches the search radius/circle
            allSeekers[i].SetSearchBoxQuad();
            allSeekers[i].ClearNeighbours();
            UpdateNeighbour(allSeekers[i].neighbourRadiusSquared, allSeekers[i].searchQuad, allSeekers[i]);
            allSeekers[i].Tick();
        }
    }


    public static void AddSeeker(Seeker seeker)
    {
        allSeekers.Add(seeker);
    }

    public static void RemoveSeeker(Seeker seeker)
    {
        allSeekers.Remove(seeker);
    }

    /// <summary>
    /// 更新邻居
    /// </summary>
    /// <param name="radiusSquared"></param>
    /// <param name="searchQuad"></param>
    /// <param name="Target"></param>
    public static void UpdateNeighbour(float radiusSquared, Rect searchQuad, Seeker Target)
    {
        openQuadTreeList.Clear();
        openQuadTreeList.Add(quadTree);
        int foundCount = 0;
        while (openQuadTreeList.Count > 0 && foundCount < maxNeighbourCount)
        {
            QuadTree current = openQuadTreeList[0];
            if (current.bounds.Overlaps(searchQuad))
            {
                for (int i = 0; i < current.objects.Count; i++)
                {
                    if (foundCount == maxNeighbourCount)
                        break;
                    if (current.objects[i] != Target)
                    {
                        if ((current.objects[i].Pos - Target.Pos).sqrMagnitude < radiusSquared)
                        {
                            Target.AddNeighbours(foundCount, current.objects[i]);
                            foundCount++;
                        }
                    }
                }
                if (current.nodesInUse)
                {
                    openQuadTreeList.Add(current.nodes[0]);
                    openQuadTreeList.Add(current.nodes[1]);
                    openQuadTreeList.Add(current.nodes[2]);
                    openQuadTreeList.Add(current.nodes[3]);
                }
            }
            openQuadTreeList.Remove(current);
        }
    }

}

