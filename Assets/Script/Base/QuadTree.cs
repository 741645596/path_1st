using UnityEngine;
using System.Collections.Generic;


public class QuadTree
{
        private int maxObjectsPerQuad = 300;
        public static int maxDepthLevel = 5;
        private int level;
        public List<Seeker> objects;
        public Rect bounds;
        public Vector2 center;
        public QuadTree[] nodes;

        public bool nodesInUse = false; //if false, the child nodes are not used

        public QuadTree(int _level, Rect _bounds)
        {
            level = _level;
            objects = new List<Seeker>(maxObjectsPerQuad + 1);
            bounds = _bounds;
            center.x = bounds.xMin + (bounds.width / 2f);
            center.y = bounds.yMin + (bounds.height / 2f);
            nodes = new QuadTree[4];
        }

        public void Setup()
        {       
            if (level < maxDepthLevel)
            {
                split();
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i].Setup();
            }
        }

        public void clear()
        {    
            objects.Clear();
            if (nodesInUse)
            {
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i].clear();
            }
            nodesInUse = false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void split()
        {
            float subWidth = bounds.width / 2f;
            float subHeight = bounds.height / 2f;
            float x = bounds.xMin;
            float y = bounds.yMin;
            nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
            nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight));
            nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
            nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="seeker"></param>
        /// <returns></returns>
        private int getIndex(Vector2 pos)
        {
            bool topQuadrant = pos.y < center.y;
            bool leftQuadrant = pos.x < center.x;

            if (leftQuadrant)
            {
                if (topQuadrant) return 1;
                else return 2;
            }
            else
            {
                if (topQuadrant) return 0;
                else return 3;
            }
        }
        /// <summary>
        /// 插入4叉树中
        /// </summary>
        /// <param name="seeker"></param>
        public void insert(Seeker seeker)
        {
            if (nodesInUse)
            {
                int index = getIndex(seeker.Pos);
                nodes[index].insert(seeker);
            }
            else
            {
                objects.Add(seeker);
                if (objects.Count > maxObjectsPerQuad && level < maxDepthLevel)
                {
                    nodesInUse = true;
                    int i = 0;
                    while (i < objects.Count)
                    {
                        Seeker obj = objects[i];
                        int index = getIndex(obj.Pos);
                        nodes[index].insert(obj);
                        objects.Remove(obj);
                    }
                }
            }
        }
}