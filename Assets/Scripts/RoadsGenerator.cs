using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public float cost;
    public int index;
    public int previous;
    public List<int> adjacencies;
    public bool visited;
    public GraphNode(float cost, int previous, List<int> adjacencies, int index)
    {
        this.cost = cost;
        this.previous = previous;
        this.adjacencies = adjacencies;
        this.index = index;
    }
}

public class Graph
{
    GraphNode[] nodes;
}

public class RoadsGenerator
{
    
}
