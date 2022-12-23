using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private List<Vector3> points;

    private Rect boundary;
    private int depth;
    private int capacity;
    private bool isDivided;

    QuadTree[] nodes;

    Plane L = new Plane(Vector3.forward, Vector3.zero);
    //Constructor
    public QuadTree(Rect boundary, int depth, int capacity)
    {
        this.boundary = boundary;
        this.depth = depth;
        this.capacity = capacity;

        points = new List<Vector3>();

        nodes = new QuadTree[4];
    }

    //Split Rect to four part
    private void Subdivid()
    {
        float subWidth = boundary.width / 2;
        float subHeight = boundary.height / 2;
        float centerX = boundary.x + subWidth;
        float centerY = boundary.y + subHeight;

        Rect topRight = new Rect(centerX, centerY, subWidth, subHeight);
        Rect topLeft = new Rect(centerX - subWidth, centerY, subWidth, subHeight);
        Rect downLeft = new Rect(centerX - subWidth, centerY - subHeight, subWidth, subHeight);
        Rect downRight = new Rect(centerX - subWidth, centerY, subWidth, subHeight);

        isDivided = true;

        nodes[0] = new QuadTree(topRight, depth + 1, capacity);
        nodes[1] = new QuadTree(topLeft, depth + 1, capacity);
        nodes[2] = new QuadTree(downLeft, depth + 1, capacity);
        nodes[3] = new QuadTree(downRight, depth + 1, capacity);

    }

    public void Insert(Vector3 p)
    {
       
        if (boundary.Contains(p))
            return;


        if (points.Count < capacity)
        {
            points.Add(p);
        }
        else
        {
            if (!isDivided)
                Subdivid();

            nodes[0].Insert(p);
            nodes[1].Insert(p);
            nodes[2].Insert(p);
            nodes[3].Insert(p);
        }
    }

    public void QueryRange(Rect range, List<Vector3> found)
    {
        if (!boundary.Overlaps(range))
            return;

        for (int i = 0; i < points.Count; i++)
        {
            if (range.Contains(points[i]))
                found.Add(points[i]);
        }

        if (isDivided)
        {
            nodes[0].QueryRange(range, found);
            nodes[1].QueryRange(range, found);
            nodes[2].QueryRange(range, found);
            nodes[3].QueryRange(range, found);
        }
    }

    public void Clear()
    {
        points.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (isDivided)
            {
                nodes[i].Clear();
                nodes[i] = null;
                isDivided = false;
            }
        }
    }

    public void Show()
    {
        GameObject content = new GameObject("Contents");
        Vector3[] boundaryVertexs = new Vector3[4];
        
    }


    
    
}
